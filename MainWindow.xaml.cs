﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using Ookii.Dialogs.Wpf;
using RT.Util;
using RT.Util.Dialogs;
using RT.Util.ExtensionMethods;
using RT.Util.Forms;
using RT.Util.Lingo;
using RT.Util.Xml;
using TankIconMaker.Layers;
using WotDataLib;
using WpfCrutches;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace TankIconMaker
{
    partial class MainWindow : ManagedWindow
    {
        private DispatcherTimer _updateIconsTimer = new DispatcherTimer(DispatcherPriority.Background);
        private DispatcherTimer _updatePropertiesTimer = new DispatcherTimer(DispatcherPriority.Background);
        private CancellationTokenSource _cancelRender = new CancellationTokenSource();
        private Dictionary<string, RenderTask> _renderResults = new Dictionary<string, RenderTask>();
        private static BitmapImage _warningImage;
        private ObservableValue<bool> _rendering = new ObservableValue<bool>(false);
        private ObservableValue<bool> _dataMissing = new ObservableValue<bool>(false);
        private ObservableCollection<string> _dataWarnings = new ObservableCollection<string>();
        private ObservableCollection<Warning> _otherWarnings = new ObservableCollection<Warning>();

        private LanguageHelperWpfOld<Translation> _translationHelper;

        public MainWindow()
            : base(App.Settings.MainWindow)
        {
            InitializeComponent();
            GlobalStatusShow(App.Translation.Misc.GlobalStatus_Loading);
            ContentRendered += InitializeEverything;
            Closing += MainWindow_Closing;
        }

        /// <summary>
        /// Shows a message in large letters in an overlay in the middle of the window. Must be called on the UI thread
        /// and won't become visible until the UI thread returns (into the dispatcher).
        /// </summary>
        private void GlobalStatusShow(string message)
        {
            (ctGlobalStatusBox.Child as TextBlock).Text = message;
            ctGlobalStatusBox.Visibility = Visibility.Visible;
            IsEnabled = false;
            ctIconsPanel.Opacity = 0.6;
        }

        /// <summary>
        /// Hides the message shown using <see cref="GlobalStatusShow"/>.
        /// </summary>
        private void GlobalStatusHide()
        {
            IsEnabled = true;
            ctGlobalStatusBox.Visibility = Visibility.Collapsed;
            ctIconsPanel.Opacity = 1;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_translationHelper != null && !_translationHelper.MayExitApplication())
                e.Cancel = true;
        }

        /// <summary>
        /// Performs most of the slow initializations. This method is only called after the UI becomes visible, to improve the
        /// perceived start-up performance.
        /// </summary>
        private void InitializeEverything(object ___, EventArgs ____)
        {
            ContentRendered -= InitializeEverything;

            OldFiles.DeleteOldFiles();

            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

            var mat = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            App.DpiScaleX = mat.M11;
            App.DpiScaleY = mat.M22;

            var lingoTypeDescProvider = new LingoTypeDescriptionProvider<Translation>(() => App.Translation);
            System.ComponentModel.TypeDescriptor.AddProvider(lingoTypeDescProvider, typeof(LayerBase));
            System.ComponentModel.TypeDescriptor.AddProvider(lingoTypeDescProvider, typeof(EffectBase));
            System.ComponentModel.TypeDescriptor.AddProvider(lingoTypeDescProvider, typeof(SelectorBase<string>));
            System.ComponentModel.TypeDescriptor.AddProvider(lingoTypeDescProvider, typeof(SelectorBase<BoolWithPassthrough>));
            System.ComponentModel.TypeDescriptor.AddProvider(lingoTypeDescProvider, typeof(SelectorBase<Color>));
            System.ComponentModel.TypeDescriptor.AddProvider(lingoTypeDescProvider, typeof(SelectorBase<Filename>));
#if DEBUG
            Lingo.AlsoSaveTranslationsTo = PathUtil.AppPathCombine(@"..\..\Resources\Translations");
            using (var translationFileGenerator = new Lingo.TranslationFileGenerator(PathUtil.AppPathCombine(@"..\..\Translation.g.cs")))
            {
                translationFileGenerator.TranslateWindow(this, App.Translation.MainWindow);
            }
#endif
            using (var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/TankIconMaker;component/Resources/Graphics/icon.ico")).Stream)
                _translationHelper = new LanguageHelperWpfOld<Translation>("Tank Icon Maker", "TankIconMaker", true,
                    App.Settings.TranslationFormSettings, new System.Drawing.Icon(iconStream), () => App.Settings.Lingo);
            _translationHelper.TranslationChanged += TranslationChanged;
            Translate(first: true);
            Title += " (v" + Assembly.GetExecutingAssembly().GetName().Version.Major.ToString("000") + ")";

            CommandBindings.Add(new CommandBinding(TankLayerCommands.AddLayer, cmdLayer_AddLayer));
            CommandBindings.Add(new CommandBinding(TankLayerCommands.AddEffect, cmdLayer_AddEffect, (_, a) => { a.CanExecute = isLayerOrEffectSelected(); }));
            CommandBindings.Add(new CommandBinding(TankLayerCommands.Rename, cmdLayer_Rename, (_, a) => { a.CanExecute = isLayerOrEffectSelected(); }));
            CommandBindings.Add(new CommandBinding(TankLayerCommands.Delete, cmdLayer_Delete, (_, a) => { a.CanExecute = isLayerOrEffectSelected(); }));
            CommandBindings.Add(new CommandBinding(TankLayerCommands.MoveUp, cmdLayer_MoveUp, (_, a) => { a.CanExecute = cmdLayer_MoveUp_IsAvailable(); }));
            CommandBindings.Add(new CommandBinding(TankLayerCommands.MoveDown, cmdLayer_MoveDown, (_, a) => { a.CanExecute = cmdLayer_MoveDown_IsAvailable(); }));
            CommandBindings.Add(new CommandBinding(TankLayerCommands.ToggleVisibility, cmdLayer_ToggleVisibility, (_, a) => { a.CanExecute = isLayerOrEffectSelected(); }));

            CommandBindings.Add(new CommandBinding(TankStyleCommands.Add, cmdStyle_Add));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.Delete, cmdStyle_Delete, (_, a) => { a.CanExecute = cmdStyle_UserStyleSelected(); }));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.ChangeName, cmdStyle_ChangeName, (_, a) => { a.CanExecute = cmdStyle_UserStyleSelected(); }));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.ChangeAuthor, cmdStyle_ChangeAuthor, (_, a) => { a.CanExecute = cmdStyle_UserStyleSelected(); }));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.Duplicate, cmdStyle_Duplicate));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.Import, cmdStyle_Import));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.Export, cmdStyle_Export));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.IconWidth, cmdStyle_IconWidth));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.IconHeight, cmdStyle_IconHeight));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.Centerable, cmdStyle_Centerable));

            _updateIconsTimer.Tick += UpdateIcons;
            _updateIconsTimer.Interval = TimeSpan.FromMilliseconds(100);

            if (App.Settings.LeftColumnWidth != null)
                ctLeftColumn.Width = new GridLength(App.Settings.LeftColumnWidth.Value);
            if (App.Settings.NameColumnWidth != null)
                ctLayerProperties.NameColumnWidth = App.Settings.NameColumnWidth.Value;
            ctDisplayMode.SelectedIndex = (int) App.Settings.DisplayFilter;

            ApplyBackground();
            ApplyBackgroundColors();

            _warningImage = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Graphics/warning.png"));

            // Styles: build the combined built-in and user-defined styles collection
            var styles = new CompositeCollection<Style>();
            RecreateBuiltInStyles();
            styles.AddCollection(_builtinStyles);
            styles.AddCollection(App.Settings.Styles);
            // Styles: update the active style
            if (App.Settings.ActiveStyle == null)
                App.Settings.ActiveStyle = _builtinStyles.First();
            else if (!App.Settings.Styles.Contains(App.Settings.ActiveStyle))
                App.Settings.ActiveStyle = styles.FirstOrDefault(s => s.Name == App.Settings.ActiveStyle.Name && s.Author == App.Settings.ActiveStyle.Author) ?? _builtinStyles.First();
            // Styles: configure the UI control
            ctStyleDropdown.ItemsSource = styles;
            ctStyleDropdown.DisplayMemberPath = "Display";
            ctStyleDropdown.SelectedItem = App.Settings.ActiveStyle;

            // Game installations: find/add all installations if blank
            if (App.Settings.GameInstallations.Count == 0)
                AddGameInstallations();
            // Game installations: make sure one of the installations is the active one
#pragma warning disable 0618 // ActiveInstallation should only be used for loading/saving the setting, which is what the code below does.
            if (!App.Settings.GameInstallations.Contains(App.Settings.ActiveInstallation)) // includes the "null" case
                App.Settings.ActiveInstallation = App.Settings.GameInstallations.FirstOrDefault();
            // Game installations: configure the UI control
            ctGamePath.ItemsSource = App.Settings.GameInstallations;
            ctGamePath.DisplayMemberPath = "DisplayName";
            ctGamePath.SelectedItem = App.Settings.ActiveInstallation;
#pragma warning restore 0618

            ctLayerProperties.EditorDefinitions.Add(new EditorDefinition { TargetType = typeof(ColorSelector), ExpandableObject = true });
            ctLayerProperties.EditorDefinitions.Add(new EditorDefinition { TargetType = typeof(ValueSelector<>), ExpandableObject = true });
            ctLayerProperties.EditorDefinitions.Add(new EditorDefinition { TargetType = typeof(Filename), EditorType = typeof(FilenameEditor) });
            ctLayerProperties.EditorDefinitions.Add(new EditorDefinition { TargetType = typeof(ExtraPropertyId), EditorType = typeof(DataSourceEditor) });
            ctLayerProperties.EditorDefinitions.Add(new EditorDefinition { TargetType = typeof(Anchor), EditorType = typeof(AnchorEditor) });

            ReloadData(first: true);

            // Set WPF bindings now that all the data we need is loaded
            BindingOperations.SetBinding(ctRemoveGamePath, Button.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = ctGamePath, Path = new PropertyPath(ComboBox.SelectedIndexProperty) },
                (int index) => index >= 0
            ));
            BindingOperations.SetBinding(ctGamePath, ComboBox.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = App.Settings.GameInstallations, Path = new PropertyPath("Count") },
                (int count) => count > 0
            ));
            BindingOperations.SetBinding(ctWarning, Image.VisibilityProperty, LambdaBinding.New(
                new Binding { Source = _dataWarnings, Path = new PropertyPath("Count") },
                new Binding { Source = _otherWarnings, Path = new PropertyPath("Count") },
                (int dataCount, int otherCount) => dataCount + otherCount == 0 ? Visibility.Collapsed : Visibility.Visible
            ));
            BindingOperations.SetBinding(ctSave, Button.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = _rendering, Path = new PropertyPath("Value") },
                new Binding { Source = _dataMissing, Path = new PropertyPath("Value") },
                (bool rendering, bool dataMissing) => !rendering && !dataMissing
            ));
            BindingOperations.SetBinding(ctLayersTree, TreeView.MaxHeightProperty, LambdaBinding.New(
                new Binding { Source = ctLeftBottomPane, Path = new PropertyPath(Grid.ActualHeightProperty) },
                (double paneHeight) => paneHeight * 0.4
            ));

            // Another day, another WPF crutch... http://stackoverflow.com/questions/3921712
            ctLayersTree.PreviewMouseDown += (_, __) => { FocusManager.SetFocusedElement(this, ctLayersTree); };

            // Bind the events now that all the UI is set up as desired
            Closing += (_, __) => SaveSettings();
            this.SizeChanged += SaveSettingsDelayed;
            this.LocationChanged += SaveSettingsDelayed;
            ctStyleDropdown.SelectionChanged += ctStyleDropdown_SelectionChanged;
            ctLayerProperties.PropertyValueChanged += ctLayerProperties_PropertyValueChanged;
            ctDisplayMode.SelectionChanged += ctDisplayMode_SelectionChanged;
            ctGamePath.SelectionChanged += ctGamePath_SelectionChanged;
            ctGamePath.PreviewKeyDown += ctGamePath_PreviewKeyDown;
            ctLayersTree.SelectedItemChanged += (_, e) => { _updatePropertiesTimer.Stop(); _updatePropertiesTimer.Start(); };
            _updatePropertiesTimer.Tick += (_, __) => { _updatePropertiesTimer.Stop(); ctLayerProperties.SelectedObject = ctLayersTree.SelectedItem; };
            _updatePropertiesTimer.Interval = TimeSpan.FromMilliseconds(200);

            // Refresh all the commands because otherwise WPF doesn’t realise the states have changed.
            CommandManager.InvalidateRequerySuggested();

            // Fire off some GUI events manually now that everything's set up
            ctStyleDropdown_SelectionChanged();

            // Done
            GlobalStatusHide();
            _updateIconsTimer.Start();
        }

        private void TranslationChanged(Translation t)
        {
            App.Translation = t;
            App.Settings.Lingo = t.Language;
            Translate();
        }

        private void Translate(bool first = false)
        {
            App.LayerTypes = translateTypes(App.LayerTypes);
            App.EffectTypes = translateTypes(App.EffectTypes);
            Lingo.TranslateWindow(this, App.Translation.MainWindow);
            DlgMessage.Translate(App.Translation.DlgMessage.OK,
                App.Translation.DlgMessage.CaptionInfo,
                App.Translation.DlgMessage.CaptionQuestion,
                App.Translation.DlgMessage.CaptionWarning,
                App.Translation.DlgMessage.CaptionError);

            foreach (var style in ctStyleDropdown.Items.OfType<Style>()) // this includes the built-in styles too, unlike App.Settings.Styles
                style.TranslationChanged();

            if (!first)
            {
                var wasSelected = ctLayerProperties.SelectedObject;
                ctLayerProperties.SelectedObject = null;
                ctLayerProperties.SelectedObject = wasSelected;

                ctLayersTree.ItemsSource = null;
                ctStyleDropdown_SelectionChanged();

                ReloadData();
                UpdateIcons();
            }
        }

        private static IList<TypeInfo<T>> translateTypes<T>(IList<TypeInfo<T>> types) where T : IHasTypeNameDescription
        {
            return types.Select(type =>
            {
                var obj = (T) type.Constructor();
                return new TypeInfo<T>
                {
                    Type = type.Type,
                    Constructor = type.Constructor,
                    Name = obj.TypeName,
                    Description = obj.TypeDescription,
                };
            }).OrderBy(ti => ti.Name).ToList().AsReadOnly();
        }

        private ObservableSortedList<Style> _builtinStyles = new ObservableSortedList<Style>();
        private void RecreateBuiltInStyles()
        {
            _builtinStyles.Clear();

            var assy = Assembly.GetExecutingAssembly();
            foreach (var resourceName in assy.GetManifestResourceNames().Where(n => n.Contains(".BuiltInStyles.")))
            {
                try
                {
                    XDocument doc;
                    using (var stream = assy.GetManifestResourceStream(resourceName))
                        doc = XDocument.Load(stream);
                    var style = XmlClassify.ObjectFromXElement<Style>(doc.Root);
                    style.Kind = style.Name == "Original" ? StyleKind.Original : style.Name == "Current" ? StyleKind.Current : StyleKind.BuiltIn;
                    _builtinStyles.Add(style);
                }
                catch { } // should not happen, but if it does, pretend the style doesn’t exist.
            }
        }

        /// <summary>
        /// Gets the installation currently selected in the GUI, or null if none are available.
        /// </summary>
        private TimGameInstallation ActiveInstallation
        {
            get { return ctGamePath.Items.Count == 0 ? null : (TimGameInstallation) ctGamePath.SelectedItem; }
        }

        [Obsolete("Use LoadContext instead!")] // to make sure this is not used accidentally; the correct use is to LoadContext(cached: true)
        private WotContext _context;

        /// <summary>
        /// Returns a WotContext based on the currently selected game installation and the last loaded game data.
        /// Returns null if a context cannot be obtained, e.g. due to missing data or configuration. Use the cached context
        /// except when all data must be reloaded off disk; store the cache in a local and pass to all the methods that need to
        /// finish the operation.
        /// </summary>
        private WotContext LoadContext(bool cached)
        {
#pragma warning disable 618 // this method is the only one allowed to use "_context"
            if (cached && _context != null && ActiveInstallation != null && _context.Installation.GameVersionId == ActiveInstallation.GameVersionId)
                return _context;
            _context = null;
            if (ActiveInstallation.GameVersionId != null)
                try { _context = WotData.Load(PathUtil.AppPathCombine("Data"), ActiveInstallation, App.Settings.DefaultPropertyAuthor, PathUtil.AppPathCombine("Data", "Exported")); }
                catch (WotDataException) { }
            FilenameEditor.LastContext = _context; // bit of a hack to give these instances access to the context, see comment on the field.
            return _context;
#pragma warning restore 618
        }

        /// <summary>
        /// Does a bunch of stuff necessary to reload all the data off disk and refresh the UI (except for drawing the icons:
        /// this must be done as a separate step).
        /// </summary>
        private void ReloadData(bool first = false)
        {
            _renderResults.Clear();
            ZipCache.Clear();
            ImageCache.Clear();

            foreach (var gameInstallation in App.Settings.GameInstallations.ToList()) // grab a list of all items because the source auto-resorts on changes
                gameInstallation.Reload();

            // Disable parts of the UI if some of the data is unavailable, and show warnings as appropriate
            _otherWarnings.RemoveWhere(w => w is Warning_DataMissing);
            WotContext context = null;
            if (ActiveInstallation == null || ActiveInstallation.GameVersionId == null)
            {
                // This means we don't have a valid WoT installation available. So we still can't show the correct lists of properties
                // in the drop-downs, and also can't render tanks because we don't know which ones.
                _dataMissing.Value = true;
                _otherWarnings.Add(new Warning_DataMissing(App.Translation.Error.DataMissing_NoWotInstallation));
                if (ActiveInstallation == null)
                    ctGameInstallationWarning.Text = App.Translation.Error.DataMissing_NoInstallationSelected;
                else if (!Directory.Exists(ActiveInstallation.Path))
                    ctGameInstallationWarning.Text = App.Translation.Error.DataMissing_DirNotFound;
                else
                    ctGameInstallationWarning.Text = App.Translation.Error.DataMissing_NoWotInstallation;
            }
            else
            {
                // Resolve the data to see if it's fine
                context = LoadContext(cached: false);
                // Update the list of warnings
                _dataWarnings.Clear();
                foreach (var warning in context.Warnings)
                    _dataWarnings.Add(warning);
                // See how complete of a context we managed to get
                if (context.VersionConfig == null)
                {
                    // The WoT installation is valid, but we don't have a suitable version config. Can list the right properties, but can't really render.
                    _dataMissing.Value = true;
                    var msg = App.Translation.Error.DataMissing_WotVersionTooOld.Fmt(ActiveInstallation.GameVersionName + " #" + ActiveInstallation.GameVersionId);
                    _otherWarnings.Add(new Warning_DataMissing(msg));
                    ctGameInstallationWarning.Text = msg;
                }
                else if (context.Tanks == null)
                {
                    // Everything's fine with the installation but we don't have any built-in data files for this version. Can edit styles but not render.
                    _dataMissing.Value = true;
                    _otherWarnings.Add(new Warning_DataMissing(App.Translation.Error.DataMissing_NoBuiltinData));
                    ctGameInstallationWarning.Text = null;
                }
                else
                {
                    // Everything's fine.
                    _dataMissing.Value = false;
                    ctGameInstallationWarning.Text = null;
                }
            }

            // Update the list of data sources currently available. This list is used by drop-downs which offer the user to select a property.
            foreach (var item in App.DataSources.Where(ds => ds.GetType() == typeof(DataSourceInfo)).ToArray())
            {
                var extra = context == null ? null : context.ExtraProperties.FirstOrDefault(df => df.PropertyId == item.PropertyId);
                if (extra == null)
                    App.DataSources.Remove(item);
                else
                    item.UpdateFrom(extra);
            }
            if (context != null)
                foreach (var extra in context.ExtraProperties)
                {
                    if (!App.DataSources.Any(item => extra.PropertyId == item.PropertyId))
                        App.DataSources.Add(new DataSourceInfo(extra));
                }

            if (_dataMissing)
            {
                // Clear the icons area. It will remain empty because icon rendering code exits if _dataMissing is true.
                // Various UI controls disable automatically whenever _dataMissing is true.
                ctIconsPanel.Children.Clear();
            }
            else
            {
                // Force a full re-render
                if (!first)
                {
                    _renderResults.Clear();
                    UpdateIcons();
                }
            }
        }

        /// <summary>
        /// Schedules an icon update to occur after a short timeout. If called again before the timeout, will re-set the timeout
        /// back to original value. If called during a render, the render is cancelled immediately. Call this if the event that
        /// invalidated the current icons can occur frequently. Call <see cref="UpdateIcons"/> for immediate response.
        /// </summary>
        private void ScheduleUpdateIcons()
        {
            _cancelRender.Cancel();

            _rendering.Value = true;
            foreach (var image in ctIconsPanel.Children.OfType<TankImageControl>())
                image.Opacity = 0.7;

            _updateIconsTimer.Stop();
            _updateIconsTimer.Start();
        }

        /// <summary>
        /// Begins an icon update immediately. The icons are rendered in the background without blocking the UI. If called during
        /// a previous render, the render is cancelled immediately. Call this if the event that invalidated the current icons occurs
        /// infrequently, to ensure immediate response to user action. For very frequent updates, use <see cref="ScheduleUpdateIcons"/>.
        /// Only the icons not in the render cache are re-rendered; remove some or all to force a re-render of the icon.
        /// </summary>
        private void UpdateIcons(object _ = null, EventArgs __ = null)
        {
            _rendering.Value = true;
            foreach (var image in ctIconsPanel.Children.OfType<TankImageControl>())
                image.Opacity = 0.7;
            _otherWarnings.RemoveWhere(w => w is Warning_RenderedWithErrWarn);

            _updateIconsTimer.Stop();
            _cancelRender.Cancel();
            _cancelRender = new CancellationTokenSource();
            var cancelToken = _cancelRender.Token; // must be a local so that the task lambda captures it; _cancelRender could get reassigned before a task gets to check for cancellation of the old one

            if (_dataMissing)
                return;

            var context = LoadContext(cached: true);
            var images = ctIconsPanel.Children.OfType<TankImageControl>().ToList();
            var renderTasks = ListRenderTasks(context);

            var style = App.Settings.ActiveStyle;
            foreach (var layer in style.Layers)
                TestLayer(style, layer);

            var tasks = new List<Action>();
            for (int i = 0; i < renderTasks.Count; i++)
            {
                if (i >= images.Count)
                    images.Add(CreateTankImageControl(style));
                var renderTask = renderTasks[i];
                var image = images[i];

                image.ToolTip = renderTasks[i].TankId;
                if (_renderResults.ContainsKey(renderTask.TankId))
                {
                    image.Source = _renderResults[renderTask.TankId].Image;
                    image.RenderTask = _renderResults[renderTask.TankId];
                    image.Opacity = 1;
                }
                else
                    tasks.Add(() =>
                    {
                        try
                        {
                            if (cancelToken.IsCancellationRequested) return;
                            RenderTank(style, renderTask);
                            if (cancelToken.IsCancellationRequested) return;
                            Dispatcher.Invoke(new Action(() =>
                            {
                                if (cancelToken.IsCancellationRequested) return;
                                _renderResults[renderTask.TankId] = renderTask;
                                image.Source = renderTask.Image;
                                image.RenderTask = renderTask;
                                image.Opacity = 1;
                                if (ctIconsPanel.Children.OfType<TankImageControl>().All(c => c.Opacity == 1))
                                    UpdateIconsCompleted();
                            }));
                        }
                        catch { }
                    });
            }
            foreach (var task in tasks)
                Task.Factory.StartNew(task, cancelToken, TaskCreationOptions.None, PriorityScheduler.Lowest);

            // Remove unused images
            foreach (var image in images.Skip(renderTasks.Count))
                ctIconsPanel.Children.Remove(image);
            if (ctIconsPanel.Children.OfType<TankImageControl>().All(c => c.Opacity == 1))
                UpdateIconsCompleted();
        }

        /// <summary>
        /// Called on the GUI thread whenever all the icon renders are completed.
        /// </summary>
        private void UpdateIconsCompleted()
        {
            _rendering.Value = false;

            // Update the warning messages
            if (_renderResults.Values.Any(rr => rr.Exception != null))
                _otherWarnings.Add(new Warning_RenderedWithErrWarn(App.Translation.Error.RenderWithErrors));
            else if (_renderResults.Values.Any(rr => rr.WarningsCount > 0))
                _otherWarnings.Add(new Warning_RenderedWithErrWarn(App.Translation.Error.RenderWithWarnings));

            // Clean up all those temporary images we've just created and won't be doing again for a while.
            // (this keeps "private bytes" when idle 10-15 MB lower)
            GC.Collect();
        }

        /// <summary>
        /// Tests the specified layer instance for its handling of missing extra properties (and possibly other problems). Adds an
        /// appropriate warning message if a problem is detected.
        /// </summary>
        private void TestLayer(Style style, LayerBase layer)
        {
            // Test missing extra properties
            _otherWarnings.RemoveWhere(w => w is Warning_LayerTest_MissingExtra);
            try
            {
                var tank = new TestTank("test", 5, Country.USSR, Class.Medium, Category.Normal);
                tank.LoadedImage = new BitmapRam(style.IconWidth, style.IconHeight);
                layer.Draw(tank);
            }
            catch (Exception e)
            {
                if (!(e is StyleUserError))
                    _otherWarnings.Add(new Warning_LayerTest_MissingExtra(("The layer {0} is buggy: it throws a {1} when presented with a tank that is missing some \"extra\" properties. Please report this to the developer.").Fmt(layer.GetType().Name, e.GetType().Name)));
                // The maker must not throw when properties are missing: firstly, for configurable properties the user could select "None"
                // from the drop-down, and secondly, hard-coded properties could simply be missing altogether.
                // (although this could, of course, be a bug in TankIconMaker itself)
            }

            // Test unexpected property values
            _otherWarnings.RemoveWhere(w => w is Warning_LayerTest_UnexpectedProperty);
            try
            {
                var tank = new TestTank("test", 5, Country.USSR, Class.Medium, Category.Normal);
                tank.PropertyValue = "z"; // very short, so substring/indexing can fail, also not parseable as integer. Hopefully "unexpected enough".
                tank.LoadedImage = new BitmapRam(style.IconWidth, style.IconHeight);
                layer.Draw(tank);
            }
            catch (Exception e)
            {
                if (!(e is StyleUserError))
                    _otherWarnings.Add(new Warning_LayerTest_UnexpectedProperty(("The layer {0} is buggy: it throws a {1} possibly due to a property value it didn't expect. Please report this to the developer.").Fmt(layer.GetType().Name, e.GetType().Name)));
                // The maker must not throw for unexpected property values: it could issue a warning using tank.AddWarning.
                // (although this could, of course, be a bug in TankIconMaker itself)
            }

            // Test missing images
            _otherWarnings.RemoveWhere(w => w is Warning_LayerTest_MissingImage);
            try
            {
                var tank = new TestTank("test", 5, Country.USSR, Class.Medium, Category.Normal);
                tank.PropertyValue = "test";
                layer.Draw(tank);
            }
            catch (Exception e)
            {
                if (!(e is StyleUserError))
                    _otherWarnings.Add(new Warning_LayerTest_MissingImage(("The layer {0} is buggy: it throws a {1} when some of the standard images cannot be found. Please report this to the developer.").Fmt(layer.GetType().Name, e.GetType().Name)));
                // The maker must not throw if the images are missing: it could issue a warning using tank.AddWarning though.
                // (although this could, of course, be a bug in TankIconMaker itself)
            }
        }

        /// <summary>
        /// Executes a render task. Will handle any exceptions in the maker and draw an appropriate substitute image
        /// to draw the user's attention to the problem.
        /// </summary>
        private static void RenderTank(Style style, RenderTask renderTask)
        {
            try
            {
                var result = new BitmapWpf(style.IconWidth, style.IconHeight);
                using (result.UseWrite())
                {
                    foreach (var layer in style.Layers.Where(l => l.Visible && l.VisibleFor.GetValue(renderTask.Tank) == BoolWithPassthrough.Yes))
                    {
                        var img = layer.Draw(renderTask.Tank);
                        if (img == null)
                            continue;
                        foreach (var effect in layer.Effects.Where(e => e.Visible && e.VisibleFor.GetValue(renderTask.Tank) == BoolWithPassthrough.Yes))
                        {
                            img = effect.Apply(renderTask.Tank, img.AsWritable());
                            if (effect is Effects.SizePosEffect)
                                if (img.Width < style.IconWidth || img.Height < style.IconHeight)
                                {
                                    var imgOrig = img;
                                    img = new BitmapRam(Math.Max(style.IconWidth, img.Width), Math.Max(style.IconHeight, img.Height));
                                    img.DrawImage(imgOrig);
                                }
                        }
                        result.DrawImage(img);
                    }
                }
                if (style.Centerable)
                {
                    var width = result.PreciseWidth();
                    var wantedWidth = Math.Min(width.Right + width.Left + 1, style.IconWidth);
                    var img = result;
                    result = new BitmapWpf(wantedWidth, img.Height);
                    result.CopyPixelsFrom(img);
                }
                result.MarkReadOnly();
                renderTask.Image = result.UnderlyingImage;
            }
            catch (Exception e)
            {
                renderTask.Image = Ut.NewBitmapWpf(style.IconWidth, style.IconHeight, dc =>
                {
                    dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)), null, new Rect(0.5, 1.5, style.IconWidth - 1, style.IconHeight - 3));
                    var pen = new Pen(e is StyleUserError ? Brushes.Green : Brushes.Red, 2);
                    dc.DrawLine(pen, new Point(1, 2), new Point(style.IconWidth - 1, style.IconHeight - 3));
                    dc.DrawLine(pen, new Point(style.IconWidth - 1, 2), new Point(1, style.IconHeight - 3));
                    dc.DrawRectangle(null, new Pen(Brushes.Black, 1), new Rect(0.5, 1.5, style.IconWidth - 1, style.IconHeight - 3));
                });
                renderTask.Exception = e;
            }
            // The tank info is no longer needed; drop the reference so it can get GC'd (it could potentially be large)
            renderTask.Tank = null;
        }

        /// <summary>
        /// Creates a TankImageControl and adds it to the scrollable tank image area. This involves a bunch of properties,
        /// event handlers, and bindings, and is hence abstracted into a method.
        /// </summary>
        private TankImageControl CreateTankImageControl(Style style)
        {
            var img = new TankImageControl
            {
                SnapsToDevicePixels = true,
                Margin = new Thickness { Right = 15 },
                Cursor = Cursors.Hand,
                Opacity = 0.7,
            };
            img.MouseLeftButtonDown += TankImage_MouseLeftButtonDown;
            img.MouseLeftButtonUp += TankImage_MouseLeftButtonUp;
            BindingOperations.SetBinding(img, TankImageControl.WidthProperty, LambdaBinding.New(
                new Binding { Source = ctZoomCheckbox, Path = new PropertyPath(CheckBox.IsCheckedProperty) },
                (bool check) => (double) style.IconWidth * (check ? 5 : 1) / App.DpiScaleX
            ));
            BindingOperations.SetBinding(img, TankImageControl.HeightProperty, LambdaBinding.New(
                new Binding { Source = ctZoomCheckbox, Path = new PropertyPath(CheckBox.IsCheckedProperty) },
                (bool check) => (double) style.IconHeight * (check ? 5 : 1) / App.DpiScaleY
            ));
            img.HorizontalAlignment = HorizontalAlignment.Left;
            ctIconsPanel.Children.Add(img);
            return img;
        }

        private object _lastTankImageDown;

        void TankImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _lastTankImageDown = sender;
        }

        private void TankImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool skip = _lastTankImageDown != sender;
            _lastTankImageDown = null;
            if (skip)
                return;

            var image = sender as TankImageControl;
            if (image == null)
                return;
            var renderResult = image.RenderTask;
            if (renderResult == null)
                return;

            var warnings = renderResult.WarningsCount == 0 ? "" : string.Join("\n\n", renderResult.Warnings.Select(s => "• " + s));
            var joiner = renderResult.WarningsCount == 0 ? "" : "\n\n";

            if (renderResult.Exception == null && renderResult.WarningsCount == 0)
                DlgMessage.ShowInfo(App.Translation.Error.RenderIconOK);

            else if (renderResult.Exception == null)
                DlgMessage.ShowWarning(warnings);

            else if (renderResult.Exception is StyleUserError)
                DlgMessage.ShowWarning(warnings + joiner + App.Translation.Error.RenderIconFail.Fmt(renderResult.Exception.Message));

            else
            {
                string hint = "";
                if (renderResult.Exception is InvalidOperationException && renderResult.Exception.Message.Contains("belongs to a different thread than its parent Freezable"))
                    hint = "Possible cause: the maker reuses a WPF drawing primitive (like Brush) for different tanks without calling Freeze() on it.\n";

                string message = hint
                    + "Exception details: {0}, {1}\n".Fmt(renderResult.Exception.GetType().Name, renderResult.Exception.Message)
                    + Ut.CollapseStackTrace(renderResult.Exception.StackTrace);

                bool copy = DlgMessage.ShowWarning(warnings + joiner + App.Translation.Prompt.ExceptionInRender + "\n\n" + message,
                    App.Translation.Prompt.ErrorToClipboard_Copy, App.Translation.Prompt.ErrorToClipboard_OK) == 0;

                if (copy)
                    try
                    {
                        Clipboard.SetText(message.ToString(), TextDataFormat.UnicodeText);
                        DlgMessage.ShowInfo(App.Translation.Prompt.ErrorToClipboard_Copied);
                    }
                    catch { DlgMessage.ShowInfo(App.Translation.Prompt.ErrorToClipboard_CopyFail); }
            }
        }

        private void SaveSettings()
        {
            _saveSettingsTimer.Stop();
            App.Settings.LeftColumnWidth = ctLeftColumn.Width.Value;
            App.Settings.NameColumnWidth = ctLayerProperties.NameColumnWidth;
            App.Settings.SaveThreaded();
        }

        private void SaveSettings(object _, EventArgs __)
        {
            SaveSettings();
        }

        private DispatcherTimer _saveSettingsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5), IsEnabled = false };

        private void SaveSettingsDelayed(object _, EventArgs __)
        {
            _saveSettingsTimer.Stop();
            _saveSettingsTimer.Tick -= SaveSettings;
            _saveSettingsTimer.Tick += SaveSettings;
            _saveSettingsTimer.Start();
        }

        private void ctStyleDropdown_SelectionChanged(object sender = null, SelectionChangedEventArgs __ = null)
        {
            _renderResults.Clear();
            ctIconsPanel.Children.Clear();
            App.Settings.ActiveStyle = (Style) ctStyleDropdown.SelectedItem;
            ctUpvote.Visibility = App.Settings.ActiveStyle.Kind == StyleKind.BuiltIn ? Visibility.Visible : Visibility.Collapsed;
            SaveSettings();
            ctLayersTree.ItemsSource = App.Settings.ActiveStyle.Layers;
            if (App.Settings.ActiveStyle.Layers.Count > 0)
            {
                App.Settings.ActiveStyle.Layers[0].TreeViewItem.IsSelected = true;
                ctLayerProperties.SelectedObject = App.Settings.ActiveStyle.Layers[0];
            }
            else
                ctLayerProperties.SelectedObject = null;
            UpdateIcons();
        }

        private static bool isMedHighTier(WotTank t)
        {
            switch (t.Class)
            {
                case Class.Light: return t.Tier >= 4;
                case Class.Artillery: return t.Tier >= 5;
                case Class.Destroyer: return t.Tier >= 5;
                case Class.Medium: return t.Tier >= 6;
                case Class.Heavy: return t.Tier >= 6;
                default: return false;
            }
        }

        private static bool isHighTier(WotTank t)
        {
            switch (t.Class)
            {
                case Class.Light: return t.Tier >= 7;
                case Class.Artillery: return t.Tier >= 8;
                case Class.Destroyer: return t.Tier >= 8;
                case Class.Medium: return t.Tier >= 9;
                case Class.Heavy: return t.Tier >= 9;
                default: return false;
            }
        }

        /// <summary>
        /// Constructs a list of render tasks based on the current settings in the GUI. Will enumerate only some
        /// of the tanks if the user chose a smaller subset in the GUI.
        /// </summary>
        /// <param name="all">Forces the method to enumerate all tanks regardless of the GUI setting.</param>
        private static List<RenderTask> ListRenderTasks(WotContext context, bool all = false)
        {
            if (context.Tanks.Count == 0)
                return new List<RenderTask>(); // happens when there are no built-in data files

            IEnumerable<WotTank> selection = null;
            switch (all ? DisplayFilter.All : App.Settings.DisplayFilter)
            {
                case DisplayFilter.All: selection = context.Tanks; break;
                case DisplayFilter.OneOfEach:
                    selection = context.Tanks.Select(t => new { t.Category, t.Class, t.Country }).Distinct()
                        .SelectMany(p => SelectTiers(context.Tanks.Where(t => t.Category == p.Category && t.Class == p.Class && t.Country == p.Country)));
                    break;

                case DisplayFilter.China: selection = context.Tanks.Where(t => t.Country == Country.China); break;
                case DisplayFilter.France: selection = context.Tanks.Where(t => t.Country == Country.France); break;
                case DisplayFilter.Germany: selection = context.Tanks.Where(t => t.Country == Country.Germany); break;
                case DisplayFilter.Japan: selection = context.Tanks.Where(t => t.Country == Country.Japan); break;
                case DisplayFilter.UK: selection = context.Tanks.Where(t => t.Country == Country.UK); break;
                case DisplayFilter.USA: selection = context.Tanks.Where(t => t.Country == Country.USA); break;
                case DisplayFilter.USSR: selection = context.Tanks.Where(t => t.Country == Country.USSR); break;

                case DisplayFilter.Light: selection = context.Tanks.Where(t => t.Class == Class.Light); break;
                case DisplayFilter.Medium: selection = context.Tanks.Where(t => t.Class == Class.Medium); break;
                case DisplayFilter.Heavy: selection = context.Tanks.Where(t => t.Class == Class.Heavy); break;
                case DisplayFilter.Artillery: selection = context.Tanks.Where(t => t.Class == Class.Artillery); break;
                case DisplayFilter.Destroyer: selection = context.Tanks.Where(t => t.Class == Class.Destroyer); break;

                case DisplayFilter.Normal: selection = context.Tanks.Where(t => t.Category == Category.Normal); break;
                case DisplayFilter.Premium: selection = context.Tanks.Where(t => t.Category == Category.Premium); break;
                case DisplayFilter.Special: selection = context.Tanks.Where(t => t.Category == Category.Special); break;

                case DisplayFilter.TierLow: selection = context.Tanks.Where(t => !isMedHighTier(t)); break;
                case DisplayFilter.TierMedHigh: selection = context.Tanks.Where(t => isMedHighTier(t)); break;
                case DisplayFilter.TierHigh: selection = context.Tanks.Where(t => isHighTier(t)); break;
            }

            return selection.OrderBy(t => t.Country).ThenBy(t => t.Class).ThenBy(t => t.Tier).ThenBy(t => t.Category).ThenBy(t => t.TankId)
                .Select(tank =>
                {
                    var task = new RenderTask();
                    task.TankId = tank.TankId;
                    task.Tank = new Tank(
                        tank,
                        addWarning: task.AddWarning
                    );
                    return task;
                }).ToList();
        }

        /// <summary>
        /// Enumerates up to three tanks with tiers as different as possible. Ideally enumerates one tier 1, one tier 5 and one tier 10 tank.
        /// </summary>
        private static IEnumerable<WotTank> SelectTiers(IEnumerable<WotTank> tanks)
        {
            WotTank min = null;
            WotTank mid = null;
            WotTank max = null;
            foreach (var tank in tanks)
            {
                if (min == null || tank.Tier < min.Tier)
                    min = tank;
                if (mid == null || Math.Abs(tank.Tier - 5) < Math.Abs(mid.Tier - 5))
                    mid = tank;
                if (max == null || tank.Tier > max.Tier)
                    max = tank;
            }
            if (Math.Abs((mid == null ? 999 : mid.Tier) - (min == null ? 999 : min.Tier)) < 3)
                mid = null;
            if (Math.Abs((mid == null ? 999 : mid.Tier) - (max == null ? 999 : max.Tier)) < 3)
                mid = null;
            if (Math.Abs((min == null ? 999 : min.Tier) - (max == null ? 999 : max.Tier)) < 5)
                max = null;
            if (min != null)
                yield return min;
            if (mid != null)
                yield return mid;
            if (max != null)
                yield return max;
        }

        private void ctGamePath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
#pragma warning disable 0618 // ActiveInstallation should only be used for loading/saving the setting, which is what the code below does.
            App.Settings.ActiveInstallation = ctGamePath.SelectedItem as TimGameInstallation;
#pragma warning restore 0618

            ReloadData();
            SaveSettings();
        }

        private void ctGamePath_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ctGamePath.IsKeyboardFocusWithin && ctGamePath.IsDropDownOpen && e.Key == Key.Delete)
            {
                RemoveGamePath();
                e.Handled = true;
            }
        }

        private void ctLayerProperties_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            if (App.Settings.ActiveStyle.Kind != StyleKind.User)
            {
                GetEditableStyle(); // duplicate the style
                RecreateBuiltInStyles();
            }
            _renderResults.Clear();
            ScheduleUpdateIcons();
            SaveSettings();
        }

        private void ctDisplayMode_SelectionChanged(object _, SelectionChangedEventArgs __)
        {
            App.Settings.DisplayFilter = (DisplayFilter) ctDisplayMode.SelectedIndex;
            UpdateIcons();
            SaveSettings();
        }

        private void ctBackground_Click(object _, EventArgs __)
        {
            var menu = ctBackground.ContextMenu;
            menu.PlacementTarget = ctBackground;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menu.Items.Clear();

            Directory.CreateDirectory(PathUtil.AppPathCombine("Backgrounds"));
            foreach (var file in new DirectoryInfo(PathUtil.AppPathCombine("Backgrounds")).GetFiles("*.jpg").Where(f => f.Extension == ".jpg")) /* GetFiles has a bug whereby "blah.jpg2" is also matched */
                menu.Items.Add(new MenuItem { Header = Path.GetFileNameWithoutExtension(file.Name), Tag = file.Name });

            menu.Items.Add(new Separator());
            menu.Items.Add(new MenuItem { Header = App.Translation.MainWindow.BackgroundCheckered.ToString(), Tag = ":checkered" });
            menu.Items.Add(new MenuItem { Header = App.Translation.MainWindow.BackgroundSolidColor.ToString(), Tag = ":solid" });

            if (App.Settings.Background == ":checkered")
            {
                menu.Items.Add(new Separator());
                var menuitem = new MenuItem { Header = App.Translation.MainWindow.BackgroundChangeCheckered1.ToString() };
                menuitem.Click += delegate { ChangeColor(ref App.Settings.BackgroundCheckeredColor1); ApplyBackgroundColors(); };
                menu.Items.Add(menuitem);
                menuitem = new MenuItem { Header = App.Translation.MainWindow.BackgroundChangeCheckered2.ToString() };
                menuitem.Click += delegate { ChangeColor(ref App.Settings.BackgroundCheckeredColor2); ApplyBackgroundColors(); };
                menu.Items.Add(menuitem);
                menuitem = new MenuItem { Header = App.Translation.MainWindow.BackgroundRestoreDefaults.ToString() };
                menuitem.Click += delegate
                {
                    App.Settings.BackgroundCheckeredColor1 = Color.FromRgb(0xc0, 0xc0, 0xc0);
                    App.Settings.BackgroundCheckeredColor2 = Color.FromRgb(0xa0, 0xa0, 0xa0);
                    SaveSettings();
                    ApplyBackgroundColors();
                };
                menu.Items.Add(menuitem);
            }
            else if (App.Settings.Background == ":solid")
            {
                menu.Items.Add(new Separator());
                var menuitem = new MenuItem { Header = App.Translation.MainWindow.BackgroundChangeSolid.ToString() };
                menuitem.Click += delegate { ChangeColor(ref App.Settings.BackgroundSolidColor); ApplyBackgroundColors(); };
                menu.Items.Add(menuitem);
                menuitem = new MenuItem { Header = App.Translation.MainWindow.BackgroundRestoreDefaults.ToString() };
                menuitem.Click += delegate
                {
                    App.Settings.BackgroundSolidColor = Color.FromRgb(0x80, 0xc0, 0xff);
                    SaveSettings();
                    ApplyBackgroundColors();
                };
                menu.Items.Add(menuitem);
            }

            foreach (var item in menu.Items.OfType<MenuItem>().Where(i => i.Tag != null))
            {
                item.IsChecked = App.Settings.Background.EqualsNoCase(item.Tag as string);
                item.Click += delegate { App.Settings.Background = item.Tag as string; ApplyBackground(); };
            }
            menu.IsOpen = true;
        }

        private void ApplyBackground()
        {
            if (App.Settings.Background == ":checkered")
            {
                ctOuterGrid.Background = (Brush) Resources["bkgCheckered"];
            }
            else if (App.Settings.Background == ":solid")
            {
                ctOuterGrid.Background = (Brush) Resources["bkgSolidBrush"];
            }
            else
            {
                try
                {
                    var path = Path.Combine(PathUtil.AppPath, "Backgrounds", App.Settings.Background);
                    if (File.Exists(path))
                    {
                        var img = new BitmapImage();
                        img.BeginInit();
                        img.StreamSource = new MemoryStream(File.ReadAllBytes(path));
                        img.EndInit();
                        ctOuterGrid.Background = new ImageBrush
                        {
                            ImageSource = img,
                            Stretch = Stretch.UniformToFill,
                        };
                    }
                    else
                    {
                        // This will occur pretty much only at startup, when the image has been removed after the user selected it
                        App.Settings.Background = ":checkered";
                        ApplyBackground();
                    }
                }
                catch
                {
                    // The file was either corrupt or could not be opened
                    App.Settings.Background = ":checkered";
                    ApplyBackground();
                }
            }
        }

        private void ApplyBackgroundColors()
        {
            ((SolidColorBrush) Resources["bkgCheckeredBrush1"]).Color = App.Settings.BackgroundCheckeredColor1;
            ((SolidColorBrush) Resources["bkgCheckeredBrush2"]).Color = App.Settings.BackgroundCheckeredColor2;
            ((SolidColorBrush) Resources["bkgSolidBrush"]).Color = App.Settings.BackgroundSolidColor;
        }

        private void ChangeColor(ref Color color)
        {
            var dlg = new System.Windows.Forms.ColorDialog();
            dlg.Color = color.ToColorGdi();
            dlg.CustomColors = App.Settings.CustomColors;
            dlg.FullOpen = true;
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            color = dlg.Color.ToColorWpf();
            App.Settings.CustomColors = dlg.CustomColors;
            SaveSettings();
        }

        private void ctLanguage_Click(object _, EventArgs __)
        {
            var menu = ctLanguage.ContextMenu;
            menu.PlacementTarget = ctLanguage;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menu.Items.Clear();
            _translationHelper.PopulateMenuItems(menu.Items);
            menu.IsOpen = true;
        }

        private void ctWarning_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DlgMessage.ShowWarning(string.Join("\n\n", _dataWarnings.Concat(_otherWarnings.Select(w => w.Text)).Select(s => "• " + s)));
        }

        private void ctReload_Click(object sender, RoutedEventArgs e)
        {
            ReloadData();
        }

        string _overwriteAccepted = null; // icon path for which the user has last confirmed that the overwrite is OK

        private void saveIcons(string folder, object filter, bool promptEvenIfEmpty = false)
        {
            var context = LoadContext(cached: true);
            var path = folder ?? Path.Combine(context.Installation.Path, Ut.ExpandPath(context, context.VersionConfig.PathDestination));

            try
            {
                if (!_overwriteAccepted.EqualsNoCase(path) && (promptEvenIfEmpty || (Directory.Exists(path) && Directory.GetFileSystemEntries(path).Any())))
                    if (DlgMessage.ShowQuestion(App.Translation.Prompt.OverwriteIcons_Prompt
                        .Fmt(path, context.VersionConfig.TankIconExtension), App.Translation.Prompt.OverwriteIcons_Yes, App.Translation.Prompt.Cancel) == 1)
                        return;
                _overwriteAccepted = path;
                Directory.CreateDirectory(path);

                GlobalStatusShow(App.Translation.Misc.GlobalStatus_Saving);

                var style = App.Settings.ActiveStyle; // capture it in case the user selects a different one while the background task is running
                var renderTasks = ListRenderTasks(context, all: true).Where(rt => filter == null || rt.Tank.Class.Equals(filter) || rt.Tank.Country.Equals(filter)).ToList();
                var renders = _renderResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // The rest of the save process occurs off the GUI thread, while this method returns.
                Task.Factory.StartNew(() =>
                {
                    Exception exception = null;
                    try
                    {
                        foreach (var renderTask in renderTasks)
                            if (!renders.ContainsKey(renderTask.TankId))
                            {
                                renders[renderTask.TankId] = renderTask;
                                RenderTank(style, renderTask);
                            }
                        foreach (var renderTask in renderTasks)
                        {
                            var render = renders[renderTask.TankId];
                            if (render.Exception == null)
                                Ut.SaveImage(render.Image, Path.Combine(path, renderTask.TankId + context.VersionConfig.TankIconExtension), context.VersionConfig.TankIconExtension);
                        }
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                    finally
                    {
                        Dispatcher.Invoke((Action) (() =>
                        {
                            GlobalStatusHide();

                            // Cache any new renders that we don't already have
                            foreach (var kvp in renders)
                                if (!_renderResults.ContainsKey(kvp.Key))
                                    _renderResults[kvp.Key] = kvp.Value;
                            // Inform the user of what happened
                            if (exception == null)
                            {
                                int skipped = renders.Values.Count(rr => rr.Exception != null);
                                int choice = new DlgMessage
                                {
                                    Message = App.Translation.Prompt.IconsSaved.Fmt(path) +
                                        (skipped == 0 ? "" : ("\n\n" + App.Translation.Prompt.IconsSaveSkipped.Fmt(App.Translation, skipped))),
                                    Type = skipped == 0 ? DlgType.Info : DlgType.Warning,
                                    Buttons = new string[] { App.Translation.DlgMessage.OK, App.Translation.Prompt.IconsSavedGoToForum },
                                    AcceptButton = 0,
                                    CancelButton = 0,
                                }.Show();
                                if (choice == 1)
                                {
                                    var url = "http://forum.worldoftanks.ru/index.php?/topic/274782-tank-icon-maker";
                                    // will customize URL here based on language, when topics in other languages are available.
                                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                                }
                            }
                            else
                            {
                                DlgMessage.ShowError(App.Translation.Prompt.IconsSaveError.Fmt(exception.Message));
                            }
                        }));
                    }
                });
            }
            catch (Exception e)
            {
                DlgMessage.ShowError(App.Translation.Prompt.IconsSaveError.Fmt(e.Message));
            }
        }

        private bool _saveIconsToFolder = false;

        private void ctSave_Click(object _, RoutedEventArgs __)
        {
            if (!_saveIconsToFolder)
            {
                saveIcons(folder: null, filter: null);
            }
            else
            {
                if (App.Settings.SaveToFolderPath == null)
                    BrowseAndSaveIcons_All();
                else
                    saveIcons(App.Settings.SaveToFolderPath, App.Settings.SaveToFolderFilter, promptEvenIfEmpty: true /* so the user knows which folder is selected */);
            }
        }

        private void ctSaveToFolder_Click(object _, RoutedEventArgs __)
        {
            var menu = ctSaveToFolder.ContextMenu;
            menu.PlacementTarget = ctSaveToFolder;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            ctSaveIcons_AllToGameFolder.IsChecked = !_saveIconsToFolder;
            ctBrowseAndSaveIcons_All.IsChecked = _saveIconsToFolder && App.Settings.SaveToFolderFilter == null;
            ctBrowseAndSaveIcons_Light.IsChecked = _saveIconsToFolder && Class.Light.Equals(App.Settings.SaveToFolderFilter);
            ctBrowseAndSaveIcons_Medium.IsChecked = _saveIconsToFolder && Class.Medium.Equals(App.Settings.SaveToFolderFilter);
            ctBrowseAndSaveIcons_Heavy.IsChecked = _saveIconsToFolder && Class.Heavy.Equals(App.Settings.SaveToFolderFilter);
            ctBrowseAndSaveIcons_Artillery.IsChecked = _saveIconsToFolder && Class.Artillery.Equals(App.Settings.SaveToFolderFilter);
            ctBrowseAndSaveIcons_Destroyer.IsChecked = _saveIconsToFolder && Class.Destroyer.Equals(App.Settings.SaveToFolderFilter);
            ctBrowseAndSaveIcons_USSR.IsChecked = _saveIconsToFolder && Country.USSR.Equals(App.Settings.SaveToFolderFilter);
            ctBrowseAndSaveIcons_Germany.IsChecked = _saveIconsToFolder && Country.Germany.Equals(App.Settings.SaveToFolderFilter);
            ctBrowseAndSaveIcons_USA.IsChecked = _saveIconsToFolder && Country.USA.Equals(App.Settings.SaveToFolderFilter);
            ctBrowseAndSaveIcons_France.IsChecked = _saveIconsToFolder && Country.France.Equals(App.Settings.SaveToFolderFilter);
            ctBrowseAndSaveIcons_UK.IsChecked = _saveIconsToFolder && Country.UK.Equals(App.Settings.SaveToFolderFilter);
            ctBrowseAndSaveIcons_China.IsChecked = _saveIconsToFolder && Country.China.Equals(App.Settings.SaveToFolderFilter);
            ctBrowseAndSaveIcons_Japan.IsChecked = _saveIconsToFolder && Country.Japan.Equals(App.Settings.SaveToFolderFilter);
            menu.IsOpen = true;
        }

        private void BrowseAndSaveToFolder(object filter) // filter is either null or a Class/Country value
        {
            var dlg = new VistaFolderBrowserDialog();
            dlg.ShowNewFolderButton = true; // argh, the dialog requires the path to exist
            if (App.Settings.SaveToFolderPath != null && Directory.Exists(App.Settings.SaveToFolderPath))
                dlg.SelectedPath = App.Settings.SaveToFolderPath;
            if (dlg.ShowDialog() != true)
                return;
            _saveIconsToFolder = true;
            _overwriteAccepted = null; // force the prompt
            App.Settings.SaveToFolderPath = dlg.SelectedPath;
            App.Settings.SaveToFolderFilter = filter;
            SaveSettings();
            saveIcons(App.Settings.SaveToFolderPath, App.Settings.SaveToFolderFilter);
        }

        private void SaveIcons_AllToGameFolder(object _ = null, RoutedEventArgs __ = null)
        {
            _saveIconsToFolder = false;
            saveIcons(folder: null, filter: null);
        }
        private void BrowseAndSaveIcons_All(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(null); }
        private void BrowseAndSaveIcons_Light(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(Class.Light); }
        private void BrowseAndSaveIcons_Medium(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(Class.Medium); }
        private void BrowseAndSaveIcons_Heavy(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(Class.Heavy); }
        private void BrowseAndSaveIcons_Artillery(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(Class.Artillery); }
        private void BrowseAndSaveIcons_Destroyer(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(Class.Destroyer); }
        private void BrowseAndSaveIcons_USSR(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(Country.USSR); }
        private void BrowseAndSaveIcons_Germany(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(Country.Germany); }
        private void BrowseAndSaveIcons_USA(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(Country.USA); }
        private void BrowseAndSaveIcons_France(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(Country.France); }
        private void BrowseAndSaveIcons_UK(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(Country.UK); }
        private void BrowseAndSaveIcons_China(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(Country.China); }
        private void BrowseAndSaveIcons_Japan(object _ = null, RoutedEventArgs __ = null) { BrowseAndSaveToFolder(Country.Japan); }

        private void ctAbout_Click(object sender, RoutedEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string version = assembly.GetName().Version.Major.ToString().PadLeft(3, '0');
            string copyright = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false).OfType<AssemblyCopyrightAttribute>().Select(c => c.Copyright).FirstOrDefault();
            var icon = Icon as BitmapSource;
            new DlgMessage()
            {
                Message = "Tank Icon Maker\n" + App.Translation.Misc.ProgramVersion.Fmt(version) + "\nBy Romkyns\n\n" + copyright
                    + (App.Translation.Language == RT.Util.Lingo.Language.EnglishUK ? "" : ("\n\n" + App.Translation.TranslationCredits)),
                Caption = "Tank Icon Maker",
                Image = icon == null ? null : icon.ToBitmapGdi().GetBitmapCopy()
            }.Show();
        }

        private void AddGamePath(object _, RoutedEventArgs __)
        {
            // Add the very first path differently: by guessing where the game is installed
            if (App.Settings.GameInstallations.Count == 0)
            {
                AddGameInstallations();
                if (App.Settings.GameInstallations.Count > 0)
                {
                    ctGamePath.SelectedItem = App.Settings.GameInstallations.First(); // this triggers all the necessary work, like updating ActiveInstallation and re-rendering
                    return;
                }
            }

            var dlg = new VistaFolderBrowserDialog();
            if (ActiveInstallation != null && Directory.Exists(ActiveInstallation.Path))
                dlg.SelectedPath = ActiveInstallation.Path;
            if (dlg.ShowDialog() != true)
                return;

            var gameInstallation = new TimGameInstallation(dlg.SelectedPath);
            if (gameInstallation.GameVersionId == null)
            {
                if (DlgMessage.ShowWarning(App.Translation.Prompt.GameNotFound_Prompt,
                    App.Translation.Prompt.GameNotFound_Ignore, App.Translation.Prompt.Cancel) == 1)
                    return;
            }

            App.Settings.GameInstallations.Add(gameInstallation);
            SaveSettings();

            ctGamePath.SelectedItem = gameInstallation; // this triggers all the necessary work, like updating ActiveInstallation and re-rendering
        }

        private void RemoveGamePath(object _ = null, RoutedEventArgs __ = null)
        {
            // Looks rather hacky but seems to do the job correctly even when called with the drop-down visible.
            var index = ctGamePath.SelectedIndex;
            App.Settings.GameInstallations.RemoveAt(ctGamePath.SelectedIndex);
            ctGamePath.ItemsSource = null;
            ctGamePath.ItemsSource = App.Settings.GameInstallations;
            ctGamePath.SelectedIndex = Math.Min(index, App.Settings.GameInstallations.Count - 1);
            SaveSettings();
        }

        /// <summary>
        /// Finds all installations of World of Tanks on the user's computer and adds them to the list of installations.
        /// </summary>
        private void AddGameInstallations()
        {
            foreach (var path in Ut.EnumerateGameInstallations())
                App.Settings.GameInstallations.Add(new TimGameInstallation(path));
            SaveSettings();
        }

        private void ctStyleMore_Click(object sender, RoutedEventArgs e)
        {
            ctStyleMore.ContextMenu.PlacementTarget = ctStyleMore;
            ctStyleMore.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            ctStyleMore.ContextMenu.IsOpen = true;
        }

        private void ctUpvote_Click(object sender, RoutedEventArgs e)
        {
            var style = App.Settings.ActiveStyle;
            if (style.Kind != StyleKind.BuiltIn)
            {
                DlgMessage.ShowInfo(App.Translation.Prompt.Upvote_BuiltInOnly);
                return;
            }
            if (string.IsNullOrWhiteSpace(style.ForumLink) || (!style.ForumLink.StartsWith("http://") && !style.ForumLink.StartsWith("https://")))
            {
                DlgMessage.ShowInfo(App.Translation.Prompt.Upvote_NotAvailable);
                return;
            }

            if (DlgMessage.ShowInfo(App.Translation.Prompt.Upvote_Prompt
                .Fmt(style.Author, style.ForumLink.UrlUnescape()), App.Translation.Prompt.Upvote_Open, App.Translation.Prompt.Cancel) == 1)
                return;

            Process.Start(new ProcessStartInfo(style.ForumLink) { UseShellExecute = true });
        }

        private void ctLayersTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // ARGH: WPF does not select the item right-clicked on, nor does it make it easy to make this happen.
            var item = WpfUtil.VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject);

            if (item != null)
            {
                item.Focus();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Gets the currently selected style or, if it’s built-in, duplicates it first, selects the duplicate,
        /// and returns that.
        /// </summary>
        private Style GetEditableStyle()
        {
            if (App.Settings.ActiveStyle.Kind == StyleKind.User)
                return App.Settings.ActiveStyle;
            // Otherwise the active style is not editable. We must clone it, make the clone the active style, and cause as few changes
            // in the UI as possible.

            // Remember what was expanded and selected
            var layer = ctLayersTree.SelectedItem as LayerBase;
            var effect = ctLayersTree.SelectedItem as EffectBase;
            int selectedLayerIndex = layer == null && effect == null ? -1 : App.Settings.ActiveStyle.Layers.IndexOf(layer ?? effect.Layer);
            int selectedEffectIndex = effect == null ? -1 : effect.Layer.Effects.IndexOf(effect);
            var expandedIndexes = App.Settings.ActiveStyle.Layers.Select((l, i) => l.TreeViewItem.IsExpanded ? i : -1).Where(i => i >= 0).ToArray();
            // Duplicate
            var style = App.Settings.ActiveStyle.Clone();
            style.Kind = StyleKind.User;
            style.Name = App.Translation.Misc.NameOfCopied.Fmt(style.Name);
            App.Settings.Styles.Add(style);
            ctStyleDropdown.SelectedItem = style;
            SaveSettings();
            // Re-select/expand
            foreach (var i in expandedIndexes)
                style.Layers[i].TreeViewItem.IsExpanded = true;
            Dispatcher.Invoke((Action) delegate // must let the TreeView think about this before we can set IsSelected
            {
                layer = selectedLayerIndex < 0 ? null : style.Layers[selectedLayerIndex];
                effect = selectedEffectIndex < 0 ? null : layer.Effects[selectedEffectIndex];
                var tvi = effect != null ? effect.TreeViewItem : layer != null ? layer.TreeViewItem : null;
                if (tvi != null)
                {
                    tvi.IsSelected = true;
                    tvi.BringIntoView();
                }
            }, DispatcherPriority.Background);

            return style;
        }

        private void cmdLayer_AddLayer(object sender, ExecutedRoutedEventArgs e)
        {
            var newLayer = AddWindow.ShowAddLayer(this);
            if (newLayer == null)
                return;
            var style = GetEditableStyle();
            var curLayer = ctLayersTree.SelectedItem as LayerBase;
            var curEffect = ctLayersTree.SelectedItem as EffectBase;
            if (curEffect != null)
                curLayer = curEffect.Layer;
            if (curLayer != null)
                style.Layers.Insert(style.Layers.IndexOf(curLayer) + 1, newLayer);
            else
                style.Layers.Add(newLayer);
            newLayer.TreeViewItem.IsSelected = true;
            newLayer.TreeViewItem.BringIntoView();
            _renderResults.Clear();
            UpdateIcons();
            SaveSettings();
        }

        private bool isLayerOrEffectSelected()
        {
            return ctLayersTree.SelectedItem is LayerBase || ctLayersTree.SelectedItem is EffectBase;
        }

        private bool isLayerSelected()
        {
            return ctLayersTree.SelectedItem is LayerBase;
        }

        private void cmdLayer_AddEffect(object sender, ExecutedRoutedEventArgs e)
        {
            var newEffect = AddWindow.ShowAddEffect(this);
            if (newEffect == null)
                return;
            var style = GetEditableStyle();
            var curLayer = ctLayersTree.SelectedItem as LayerBase;
            var curEffect = ctLayersTree.SelectedItem as EffectBase;
            if (curLayer != null)
                curLayer.Effects.Add(newEffect);
            else if (curEffect != null)
                curEffect.Layer.Effects.Insert(curEffect.Layer.Effects.IndexOf(curEffect) + 1, newEffect);
            else
                return;
            if (!newEffect.Layer.TreeViewItem.IsExpanded)
                newEffect.Layer.TreeViewItem.IsExpanded = true;
            _renderResults.Clear();
            ScheduleUpdateIcons(); // schedule immediately so that they go semi-transparent instantly; then force the update later
            SaveSettings();
            Dispatcher.BeginInvoke((Action) delegate
            {
                newEffect.TreeViewItem.IsSelected = true;
                newEffect.TreeViewItem.BringIntoView();
                UpdateIcons();
            }, DispatcherPriority.Background);
        }

        private void cmdLayer_Rename(object sender, ExecutedRoutedEventArgs e)
        {
            var layer = ctLayersTree.SelectedItem as LayerBase;
            var effect = ctLayersTree.SelectedItem as EffectBase;
            var newName = layer != null
                ? PromptWindow.ShowPrompt(this, layer.Name, App.Translation.Prompt.RenameLayer_Title, App.Translation.Prompt.RenameLayer_Label)
                : PromptWindow.ShowPrompt(this, effect.Name, App.Translation.Prompt.RenameEffect_Title, App.Translation.Prompt.RenameEffect_Label);
            if (newName == null)
                return;
            var style = GetEditableStyle();
            if (layer != null)
            {
                layer = ctLayersTree.SelectedItem as LayerBase;
                layer.Name = newName;
            }
            else
            {
                effect = ctLayersTree.SelectedItem as EffectBase;
                effect.Name = newName;
            }
            SaveSettings();
        }

        private void cmdLayer_Delete(object sender, ExecutedRoutedEventArgs e)
        {
            if (DlgMessage.ShowQuestion(App.Translation.Prompt.DeleteLayerEffect_Prompt, App.Translation.Prompt.DeleteLayerEffect_Yes, App.Translation.Prompt.Cancel) == 1)
                return;
            var style = GetEditableStyle();
            var layer = ctLayersTree.SelectedItem as LayerBase;
            var effect = ctLayersTree.SelectedItem as EffectBase;
            if (layer != null)
            {
                int index = style.Layers.IndexOf(layer);
                safeReselect(style.Layers, null, index);
                style.Layers.RemoveAt(index);
            }
            else
            {
                int index = effect.Layer.Effects.IndexOf(effect);
                safeReselect(effect.Layer.Effects, effect.Layer, index);
                effect.Layer.Effects.RemoveAt(index);
            }
            _renderResults.Clear();
            UpdateIcons();
            SaveSettings();
        }

        private void safeReselect<T>(ObservableCollection<T> items, IHasTreeViewItem parent, int index) where T : IHasTreeViewItem
        {
            TreeViewItem item = null;
            if (items.Count > index + 1)
                item = items[index + 1].TreeViewItem;
            else if (items.Count >= 2)
                item = items[items.Count - 2].TreeViewItem;
            else if (parent != null)
                item = parent.TreeViewItem;

            if (item != null)
            {
                item.IsSelected = true;
                item.BringIntoView();
                item.Focus();
            }
        }

        private bool cmdLayer_MoveUp_IsAvailable()
        {
            return moveEffectOrLayer_IsAvailable((index, count) => index > 0);
        }

        private void cmdLayer_MoveUp(object sender, ExecutedRoutedEventArgs e)
        {
            moveEffectOrLayer(-1);
        }

        private bool cmdLayer_MoveDown_IsAvailable()
        {
            return moveEffectOrLayer_IsAvailable((index, count) => index < count - 1);
        }

        private void cmdLayer_MoveDown(object sender, ExecutedRoutedEventArgs e)
        {
            moveEffectOrLayer(1);
        }

        private bool moveEffectOrLayer_IsAvailable(Func<int, int, bool> check)
        {
            if (ctLayersTree.SelectedItem == null)
                return false;
            var style = App.Settings.ActiveStyle;
            var layer = ctLayersTree.SelectedItem as LayerBase;
            var effect = ctLayersTree.SelectedItem as EffectBase;
            if (layer != null)
                return check(style.Layers.IndexOf(layer), style.Layers.Count);
            else
                return check(effect.Layer.Effects.IndexOf(effect), effect.Layer.Effects.Count);
        }

        private void moveEffectOrLayer(int direction)
        {
            var style = GetEditableStyle();
            var layer = ctLayersTree.SelectedItem as LayerBase;
            var effect = ctLayersTree.SelectedItem as EffectBase;
            if (layer != null)
            {
                int index = style.Layers.IndexOf(layer);
                style.Layers.Move(index, index + direction);
                layer.TreeViewItem.BringIntoView();
            }
            else
            {
                int index = effect.Layer.Effects.IndexOf(effect);
                effect.Layer.Effects.Move(index, index + direction);
                effect.TreeViewItem.BringIntoView();
            }
            _renderResults.Clear();
            ScheduleUpdateIcons();
            SaveSettings();
        }

        private void cmdLayer_ToggleVisibility(object sender, ExecutedRoutedEventArgs e)
        {
            var style = GetEditableStyle();
            var layer = ctLayersTree.SelectedItem as LayerBase;
            var effect = ctLayersTree.SelectedItem as EffectBase;
            if (layer != null)
                layer.Visible = !layer.Visible;
            if (effect != null)
                effect.Visible = !effect.Visible;
            _renderResults.Clear();
            UpdateIcons();
            SaveSettings();
        }

        private void cmdStyle_Add(object sender, ExecutedRoutedEventArgs e)
        {
            var name = PromptWindow.ShowPrompt(this, App.Translation.Misc.NameOfNewStyle, App.Translation.Prompt.CreateStyle_Title, App.Translation.Prompt.CreateStyle_Label);
            if (name == null)
                return;
            var style = new Style();
            style.Name = name;
            style.Author = App.Translation.Misc.NameOfNewStyleAuthor;
            style.IconWidth = 80;
            style.IconHeight = 24;
            style.Centerable = true;
            style.Layers.Add(new TankImageLayer { Name = App.Translation.Misc.NameOfTankImageLayer });
            App.Settings.Styles.Add(style);
            ctStyleDropdown.SelectedItem = style;
            SaveSettings();
        }

        private bool cmdStyle_UserStyleSelected()
        {
            return App.Settings.ActiveStyle.Kind == StyleKind.User;
        }

        private void cmdStyle_Delete(object sender, ExecutedRoutedEventArgs e)
        {
            var style = App.Settings.ActiveStyle; // because it will have changed by the time we're ready to remove it from the list of styles
            if (DlgMessage.ShowQuestion(App.Translation.Prompt.DeleteStyle_Prompt.Fmt(style.Name), App.Translation.Prompt.DeleteStyle_Yes, App.Translation.Prompt.Cancel) == 1)
                return;
            if (ctStyleDropdown.SelectedIndex < ctStyleDropdown.Items.Count - 1)
                ctStyleDropdown.SelectedIndex++;
            else
                ctStyleDropdown.SelectedIndex--;
            App.Settings.Styles.Remove(style);
            SaveSettings();
        }

        private void cmdStyle_ChangeName(object sender, ExecutedRoutedEventArgs e)
        {
            var name = PromptWindow.ShowPrompt(this, App.Settings.ActiveStyle.Name, App.Translation.Prompt.RenameStyle_Title, App.Translation.Prompt.RenameStyle_Label);
            if (name == null)
                return;
            App.Settings.ActiveStyle.Name = name;
            SaveSettings();
        }

        private void cmdStyle_ChangeAuthor(object sender, ExecutedRoutedEventArgs e)
        {
            var author = PromptWindow.ShowPrompt(this, App.Settings.ActiveStyle.Author, App.Translation.Prompt.ChangeAuthor_Title, App.Translation.Prompt.ChangeAuthor_Label);
            if (author == null)
                return;
            App.Settings.ActiveStyle.Author = author;
            SaveSettings();
        }

        private void cmdStyle_Duplicate(object sender, ExecutedRoutedEventArgs e)
        {
            var name = PromptWindow.ShowPrompt(this, App.Translation.Misc.NameOfCopied.Fmt(App.Settings.ActiveStyle.Name),
                App.Translation.Prompt.DuplicateStyle_Title, App.Translation.Prompt.DuplicateStyle_Label);
            if (name == null)
                return;
            var style = App.Settings.ActiveStyle.Clone();
            style.Kind = StyleKind.User;
            style.Name = name;
            App.Settings.Styles.Add(style);
            ctStyleDropdown.SelectedItem = style;
            SaveSettings();
        }

        private void cmdStyle_Import(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new VistaOpenFileDialog();
            dlg.Filter = App.Translation.Misc.Filter_ImportExportStyle;
            dlg.FilterIndex = 0;
            dlg.Multiselect = false;
            dlg.CheckFileExists = true;
            if (dlg.ShowDialog() != true)
                return;

            Style style;
            try
            {
                style = XmlClassify.LoadObjectFromXmlFile<Style>(dlg.FileName);
                style.Kind = StyleKind.User;
            }
            catch
            {
                DlgMessage.ShowWarning(App.Translation.Prompt.StyleImport_Fail);
                return;
            }

            App.Settings.Styles.Add(style);
            ctStyleDropdown.SelectedItem = style;
            SaveSettings();
        }

        private void cmdStyle_Export(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new VistaSaveFileDialog();
            dlg.Filter = App.Translation.Misc.Filter_ImportExportStyle;
            dlg.FilterIndex = 0;
            dlg.CheckPathExists = true;
            if (dlg.ShowDialog() != true)
                return;

            var filename = dlg.FileName;
            if (!filename.ToLower().EndsWith(".xml"))
                filename += ".xml";
            XmlClassify.SaveObjectToXmlFile(App.Settings.ActiveStyle, filename);
            DlgMessage.ShowInfo(App.Translation.Prompt.StyleExport_Success);
        }

        private void cmdStyle_IconWidth(object sender, ExecutedRoutedEventArgs e)
        {
            // note: most of this code is duplicated below
            again: ;
            var widthStr = InputBox.GetLine(App.Translation.Prompt.IconDims_Width, App.Settings.ActiveStyle.IconWidth.ToString(), App.Translation.Prompt.IconDims_Title, App.Translation.DlgMessage.OK, App.Translation.Prompt.Cancel);
            if (widthStr == null)
                return;
            int width;
            if (!int.TryParse(widthStr, out width) || width <= 0)
            {
                DlgMessage.ShowError(App.Translation.Prompt.IconDims_NumberError);
                goto again;
            }
            if (App.Settings.ActiveStyle.IconWidth == width)
                return;
            var style = GetEditableStyle();
            style.IconWidth = width;
            SaveSettings();
            _renderResults.Clear();
            ctIconsPanel.Children.Clear(); // they need to be recreated with a new size
            UpdateIcons();
        }

        private void cmdStyle_IconHeight(object sender, ExecutedRoutedEventArgs e)
        {
            // note: most of this code is duplicated above
            again: ;
            var heightStr = InputBox.GetLine(App.Translation.Prompt.IconDims_Height, App.Settings.ActiveStyle.IconHeight.ToString(), App.Translation.Prompt.IconDims_Title, App.Translation.DlgMessage.OK, App.Translation.Prompt.Cancel);
            if (heightStr == null)
                return;
            int height;
            if (!int.TryParse(heightStr, out height) || height <= 0)
            {
                DlgMessage.ShowError(App.Translation.Prompt.IconDims_NumberError);
                goto again;
            }
            if (App.Settings.ActiveStyle.IconHeight == height)
                return;
            var style = GetEditableStyle();
            style.IconHeight = height;
            SaveSettings();
            _renderResults.Clear();
            ctIconsPanel.Children.Clear(); // they need to be recreated with a new size
            UpdateIcons();
        }

        private void cmdStyle_Centerable(object sender, ExecutedRoutedEventArgs e)
        {
            var choice = DlgMessage.ShowQuestion(App.Translation.Prompt.Centerable_Prompt.Fmt(App.Settings.ActiveStyle.Centerable ? App.Translation.Prompt.Centerable_Yes : App.Translation.Prompt.Centerable_No),
                App.Translation.Prompt.Centerable_Yes, App.Translation.Prompt.Centerable_No, App.Translation.Prompt.Cancel);
            if (choice == 2)
                return;
            if (App.Settings.ActiveStyle.Centerable == (choice == 0))
                return;
            var style = GetEditableStyle();
            style.Centerable = (choice == 0);
            SaveSettings();
            _renderResults.Clear();
            UpdateIcons();
        }

        private abstract class Warning
        {
            public string Text { get; protected set; }
            public override string ToString() { return Text; }
        }
        private sealed class Warning_LayerTest_MissingExtra : Warning { public Warning_LayerTest_MissingExtra(string text) { Text = text; } }
        private sealed class Warning_LayerTest_UnexpectedProperty : Warning { public Warning_LayerTest_UnexpectedProperty(string text) { Text = text; } }
        private sealed class Warning_LayerTest_MissingImage : Warning { public Warning_LayerTest_MissingImage(string text) { Text = text; } }
        private sealed class Warning_RenderedWithErrWarn : Warning { public Warning_RenderedWithErrWarn(string text) { Text = text; } }
        private sealed class Warning_DataMissing : Warning { public Warning_DataMissing(string text) { Text = text; } }
    }

    static class TankLayerCommands
    {
        public static RoutedCommand AddLayer = new RoutedCommand();
        public static RoutedCommand AddEffect = new RoutedCommand();
        public static RoutedCommand Rename = new RoutedCommand();
        public static RoutedCommand Delete = new RoutedCommand();
        public static RoutedCommand MoveUp = new RoutedCommand();
        public static RoutedCommand MoveDown = new RoutedCommand();
        public static RoutedCommand ToggleVisibility = new RoutedCommand();
    }

    static class TankStyleCommands
    {
        public static RoutedCommand Add = new RoutedCommand();
        public static RoutedCommand Delete = new RoutedCommand();
        public static RoutedCommand ChangeName = new RoutedCommand();
        public static RoutedCommand ChangeAuthor = new RoutedCommand();
        public static RoutedCommand Duplicate = new RoutedCommand();
        public static RoutedCommand Import = new RoutedCommand();
        public static RoutedCommand Export = new RoutedCommand();
        public static RoutedCommand IconWidth = new RoutedCommand();
        public static RoutedCommand IconHeight = new RoutedCommand();
        public static RoutedCommand Centerable = new RoutedCommand();
    }

    /// <summary>
    /// Holds all the information needed to render one tank image during the rendering stage, and the render results afterwards.
    /// </summary>
    sealed class RenderTask
    {
        /// <summary>System Id of the tank that this task is for.</summary>
        public string TankId;
        /// <summary>All the tank data pertaining to this render task. This is set to null immediately after a render.</summary>
        public Tank Tank;

        /// <summary>Image rendered by the maker for a tank.</summary>
        public BitmapSource Image;
        /// <summary>Any warnings generated by the maker while rendering this image.</summary>
        public List<string> Warnings;
        /// <summary>Exception that occurred while rendering this image, or null if none.</summary>
        public Exception Exception;

        public void AddWarning(string warning)
        {
            if (Warnings == null) Warnings = new List<string>();
            Warnings.Add(warning);
        }
        public int WarningsCount { get { return Warnings == null ? 0 : Warnings.Count; } }
    }

    /// <summary>
    /// Represents a tank's icon on screen.
    /// </summary>
    sealed class TankImageControl : Image
    {
        public RenderTask RenderTask;

        static Geometry _triangle;
        static TankImageControl()
        {
            _triangle = new PathGeometry(new[] {
                new PathFigure(new Point(0, -50), new[] {
                    new PolyLineSegment(new[] { new Point(58, 50), new Point(-58, 50) }, isStroked: true)
                }, closed: true)
            });
            _triangle.Freeze();
        }

        /// <summary>Renders the image, optionally with a warning triangle overlay (if there are warnings to be seen).</summary>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (RenderTask == null) return;
            if (RenderTask.Exception == null && RenderTask.WarningsCount == 0) return;

            double cy = ActualHeight / 2;
            double scale = 0.6 * ActualHeight / 100;

            dc.PushTransform(new TranslateTransform(50 * scale - 7 / App.DpiScaleX, cy));
            dc.PushTransform(new ScaleTransform(scale, scale));

            dc.DrawGeometry(Brushes.Black, null, _triangle);
            dc.PushTransform(new ScaleTransform(0.83, 0.83)); dc.PushTransform(new TranslateTransform(0, 3)); dc.DrawGeometry(Brushes.Red, null, _triangle); dc.Pop(); dc.Pop();
            dc.PushTransform(new ScaleTransform(0.5, 0.5)); dc.PushTransform(new TranslateTransform(0, 16)); dc.DrawGeometry(Brushes.White, null, _triangle); dc.Pop(); dc.Pop();

            var exclamation = new FormattedText("!", System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Black"), 55, Brushes.Black);
            dc.DrawText(exclamation, new Point(-exclamation.Width / 2, 11 - exclamation.Height / 2));

            dc.Pop(); dc.Pop();
        }
    }
}
