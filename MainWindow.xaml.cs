using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Ookii.Dialogs.Wpf;
using RT.Util;
using RT.Util.Dialogs;
using RT.Util.Forms;
using TankIconMaker.Effects;
using TankIconMaker.Layers;
using WpfCrutches;
using Xceed.Wpf.Toolkit.PropertyGrid;

/*
 * Delete effects whose type is no longer in the assembly
 * Hide/show layer/effect toggle
 * Initial size of the dialogs is too large. Center in owner by default
 * Import/export
 * See if transparent ClearType works reasonably well (add ClearType background hint or something?)
 * Image scaling sharpness
 * Use a WPF MessageBox (avoid WinForms interop startup cost)
 * _otherWarnings: tag with warning type to enable reliable removal
 */

namespace TankIconMaker
{
    partial class MainWindow : ManagedWindow
    {
        private DispatcherTimer _updateIconsTimer = new DispatcherTimer(DispatcherPriority.Background);
        private CancellationTokenSource _cancelRender = new CancellationTokenSource();
        private Dictionary<string, RenderTask> _renderResults = new Dictionary<string, RenderTask>();
        private static BitmapImage _warningImage;
        private ObservableValue<bool> _rendering = new ObservableValue<bool>(false);

        private ObservableCollection<string> _dataWarnings = new ObservableCollection<string>();
        private ObservableCollection<string> _otherWarnings = new ObservableCollection<string>();

        public MainWindow()
            : base(Program.Settings.MainWindow)
        {
            InitializeComponent();
            GlobalStatusShow("Loading...");
            ContentRendered += InitializeEverything;
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
            Program.DpiScaleX = mat.M11;
            Program.DpiScaleY = mat.M22;

            CommandBindings.Add(new CommandBinding(TankLayerCommands.AddLayer, cmdLayer_AddLayer));
            CommandBindings.Add(new CommandBinding(TankLayerCommands.AddEffect, cmdLayer_AddEffect, (_, a) => { a.CanExecute = cmdLayer_AddEffect_IsAvailable(); }));
            CommandBindings.Add(new CommandBinding(TankLayerCommands.Rename, cmdLayer_Rename, (_, a) => { a.CanExecute = cmdLayer_Rename_IsAvailable(); }));
            CommandBindings.Add(new CommandBinding(TankLayerCommands.Delete, cmdLayer_Delete, (_, a) => { a.CanExecute = cmdLayer_Delete_IsAvailable(); }));
            CommandBindings.Add(new CommandBinding(TankLayerCommands.MoveUp, cmdLayer_MoveUp, (_, a) => { a.CanExecute = cmdLayer_MoveUp_IsAvailable(); }));
            CommandBindings.Add(new CommandBinding(TankLayerCommands.MoveDown, cmdLayer_MoveDown, (_, a) => { a.CanExecute = cmdLayer_MoveDown_IsAvailable(); }));

            CommandBindings.Add(new CommandBinding(TankStyleCommands.Add, cmdStyle_Add));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.Delete, cmdStyle_Delete, (_, a) => { a.CanExecute = cmdStyle_NonBuiltInStyleSelected(); }));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.ChangeName, cmdStyle_ChangeName, (_, a) => { a.CanExecute = cmdStyle_NonBuiltInStyleSelected(); }));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.ChangeAuthor, cmdStyle_ChangeAuthor, (_, a) => { a.CanExecute = cmdStyle_NonBuiltInStyleSelected(); }));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.Duplicate, cmdStyle_Duplicate, (_, a) => { a.CanExecute = ctStyleDropdown.SelectedItem is Style; }));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.Import, cmdStyle_Import));
            CommandBindings.Add(new CommandBinding(TankStyleCommands.Export, cmdStyle_Export, (_, a) => { a.CanExecute = ctStyleDropdown.SelectedItem is Style; }));

            _updateIconsTimer.Tick += UpdateIcons;
            _updateIconsTimer.Interval = TimeSpan.FromMilliseconds(100);

            if (Program.Settings.LeftColumnWidth != null)
                ctLeftColumn.Width = new GridLength(Program.Settings.LeftColumnWidth.Value);
            if (Program.Settings.NameColumnWidth != null)
                ctLayerProperties.NameColumnWidth = Program.Settings.NameColumnWidth.Value;
            if (Program.Settings.DisplayMode >= 0 && Program.Settings.DisplayMode < ctDisplayMode.Items.Count)
                ctDisplayMode.SelectedIndex = Program.Settings.DisplayMode.Value;

            if (File.Exists(Path.Combine(PathUtil.AppPath, "Data", "background.jpg")))
                ctOuterGrid.Background = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(Path.Combine(PathUtil.AppPath, "Data", "background.jpg"))),
                    Stretch = Stretch.UniformToFill,
                };

            _warningImage = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Graphics/warning.png"));

            // Compose the built-in styles and user styles and put them into the UI
            var styles = new CompositeCollection<Style>();
            RecreateBuiltInStyles();
            styles.AddCollection(_builtinStyles);
            styles.AddCollection(Program.Settings.Styles);
            ctStyleDropdown.ItemsSource = styles;
            ctStyleDropdown.DisplayMemberPath = "Display";

            // Locate the closest match for the maker that was selected last time the program was run
            ctStyleDropdown.SelectedItem = styles.OfType<Style>()
                .OrderBy(s => s.ToString() == Program.Settings.SelectedStyleNameAndAuthor ? 0 : 1)
                .ThenBy(s => styles.IndexOf(s))
                .FirstOrDefault();

            // Guess the location/version of the game and add to the list of paths if it’s empty
            if (Program.Settings.GameInstalls.Count == 0)
            {
                Program.Settings.GameInstalls.Add(GuessTanksLocationAndVersion());
                Program.Settings.SaveThreaded();
            }

            ctGamePath.ItemsSource = Program.Settings.GameInstalls;
            ctGamePath.DisplayMemberPath = "DisplayName";
            ctGameVersion.ItemsSource = Program.Data.Versions; // currently empty because we haven’t loaded it yet
            ctGameVersion.DisplayMemberPath = "DisplayName";

            ReloadData();

            // Set WPF bindings now that all the data we need is loaded
            BindingOperations.SetBinding(ctAddGamePath, Button.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = ctGamePath, Path = new PropertyPath(ComboBox.SelectedIndexProperty) },
                new Binding { Source = Program.Data, Path = new PropertyPath("FilesAvailable") },
                (int index, bool filesAvailable) => index >= 0 && filesAvailable
            ));
            BindingOperations.SetBinding(ctRemoveGamePath, Button.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = ctGamePath, Path = new PropertyPath(ComboBox.SelectedIndexProperty) },
                new Binding { Source = Program.Data, Path = new PropertyPath("FilesAvailable") },
                (int index, bool filesAvailable) => index >= 0 && filesAvailable
            ));
            BindingOperations.SetBinding(ctGamePath, ComboBox.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = ctGamePath, Path = new PropertyPath(ComboBox.SelectedIndexProperty) },
                new Binding { Source = Program.Data, Path = new PropertyPath("FilesAvailable") },
                (int index, bool filesAvailable) => index >= 0 && filesAvailable
            ));
            BindingOperations.SetBinding(ctGameVersion, ComboBox.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = ctGamePath, Path = new PropertyPath(ComboBox.SelectedIndexProperty) },
                new Binding { Source = Program.Data, Path = new PropertyPath("FilesAvailable") },
                (int index, bool filesAvailable) => index >= 0 && filesAvailable
            ));
            BindingOperations.SetBinding(ctStyleDropdown, ComboBox.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = ctGamePath, Path = new PropertyPath(ComboBox.SelectedIndexProperty) },
                new Binding { Source = Program.Data, Path = new PropertyPath("FilesAvailable") },
                (int index, bool filesAvailable) => index >= 0 && filesAvailable
            ));
            BindingOperations.SetBinding(ctLayerProperties, UIElement.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = ctGamePath, Path = new PropertyPath(ComboBox.SelectedIndexProperty) },
                new Binding { Source = Program.Data, Path = new PropertyPath("FilesAvailable") },
                (int index, bool filesAvailable) => index >= 0 && filesAvailable
            ));
            BindingOperations.SetBinding(ctWarning, Image.VisibilityProperty, LambdaBinding.New(
                new Binding { Source = _dataWarnings, Path = new PropertyPath("Count") },
                new Binding { Source = _otherWarnings, Path = new PropertyPath("Count") },
                (int dataCount, int otherCount) => dataCount + otherCount == 0 ? Visibility.Collapsed : Visibility.Visible
            ));
            BindingOperations.SetBinding(ctSave, Button.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = _rendering, Path = new PropertyPath("Value") },
                new Binding { Source = Program.Data, Path = new PropertyPath("FilesAvailable") },
                (bool rendering, bool filesAvailable) => !rendering && filesAvailable
            ));
            BindingOperations.SetBinding(ctLayersTree, TreeView.MaxHeightProperty, LambdaBinding.New(
                new Binding { Source = ctLeftBottomPane, Path = new PropertyPath(Grid.ActualHeightProperty) },
                (double paneHeight) => paneHeight * 0.4
            ));
            var selectedInstall = Program.Settings.GameInstalls.FirstOrDefault(gis => gis.Path.EqualsNoCase(Program.Settings.SelectedGamePath))
                ?? Program.Settings.GameInstalls.FirstOrDefault();
            ctGamePath.SelectedItem = selectedInstall;
            ctGameVersion.SelectedItem = selectedInstall.NullOr(gis => gis.GameVersion);

            // Another day, another WPF crutch... http://stackoverflow.com/questions/3921712
            ctLayersTree.PreviewMouseDown += (_, __) => { FocusManager.SetFocusedElement(this, ctLayersTree); };

            // Bind the events now that all the UI is set up as desired
            Closing += (_, __) => SaveSettings();
            this.SizeChanged += SaveSettings;
            this.LocationChanged += SaveSettings;
            ctStyleDropdown.SelectionChanged += ctStyleDropdown_SelectionChanged;
            ctLayerProperties.PropertyValueChanged += ctLayerProperties_PropertyValueChanged;
            ctDisplayMode.SelectionChanged += ctDisplayMode_SelectionChanged;
            ctGameVersion.SelectionChanged += ctGameVersion_SelectionChanged;
            ctGamePath.SelectionChanged += ctGamePath_SelectionChanged;
            ctGamePath.PreviewKeyDown += ctGamePath_PreviewKeyDown;
            ctLayersTree.SelectedItemChanged += (_, e) => { ctLayerProperties.SelectedObject = e.NewValue; };

            // Refresh all the commands because otherwise WPF doesn’t realise the states have changed.
            CommandManager.InvalidateRequerySuggested();

            // Done
            GlobalStatusHide();
            _updateIconsTimer.Start();
        }

        private ObservableSortedList<Style> _builtinStyles = new ObservableSortedList<Style>();
        private void RecreateBuiltInStyles()
        {
            _builtinStyles.Clear();
            Style style;

            // Original
            style = new Style { Name = "Original", Author = "Wargaming.net", BuiltIn = true };
            style.Layers.Add(new TankImageLayer { Name = "Image" });
            _builtinStyles.Add(style);

            // DarkAgent
            style = new Style { Name = "DarkAgent", Author = "Black_Spy", BuiltIn = true };
            style.Layers.Add(new BkgDarkAgentLayer { Name = "Back" });
            style.Layers.Add(new TankImageLayer { Name = "Image" });
            style.Layers[0].Effects.Add(new ShiftEffect { ShiftX = 30 });
            _builtinStyles.Add(style);
        }

        /// <summary>
        /// Does a bunch of stuff necessary to reload all the data off disk and refresh the UI (except for drawing the icons:
        /// this must be done as a separate step).
        /// </summary>
        private void ReloadData()
        {
            _renderResults.Clear();

            Program.Data.Reload(Path.Combine(PathUtil.AppPath, "Data"));

            // Update the list of warnings
            _dataWarnings.Clear();
            foreach (var warning in Program.Data.Warnings)
                _dataWarnings.Add(warning);

            // Yes, this stuff is a bit WinForms'sy...
            var gis = GetInstallationSettings();
            ctGamePath.Items.Refresh();
            ctGameVersion.Items.Refresh();
            if (gis != null)
            {
                ctGameVersion.SelectedItem = gis.GameVersion;
                UpdateDataSources(gis.GameVersion.Version);
                ctStyleDropdown_SelectionChanged();
            }
            else
            {
                ctLayerProperties.SelectedObject = null;
                ctIconsPanel.Children.Clear();
            }

            // Warn the user in a more obvious way
            if (!Program.Data.FilesAvailable)
                DlgMessage.ShowWarning("Found no version files and/or no built-in data files. Make sure the files are available under the following path:\n\n" + Path.Combine(PathUtil.AppPath, "Data"));

            UpdateIcons();
        }

        /// <summary>
        /// Updates the list of data sources currently available to be used in the icon maker. 
        /// </summary>
        private void UpdateDataSources(Version version)
        {
            foreach (var item in Program.DataSources.Where(ds => !(ds is DataSourceNone)).ToArray())
            {
                var extra = Program.Data.Extra.Where(df => df.Name == item.Name && df.Language == item.Language && df.Author == item.Author && df.GameVersion <= version).MaxOrDefault(df => df.GameVersion);
                if (extra == null)
                    Program.DataSources.Remove(item);
                else
                    item.UpdateFrom(extra);
            }
            foreach (var group in Program.Data.Extra.GroupBy(df => new { df.Name, df.Language, df.Author }))
            {
                var extra = group.Where(df => df.GameVersion <= version).MaxOrDefault(df => df.GameVersion);
                if (extra != null && !Program.DataSources.Any(item => extra.Name == item.Name && extra.Language == item.Language && extra.Author == item.Author))
                    Program.DataSources.Add(new DataSourceInfo(extra));
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

            _updateIconsTimer.Stop();
            _cancelRender.Cancel();
            _cancelRender = new CancellationTokenSource();
            var cancelToken = _cancelRender.Token; // must be a local so that the task lambda captures it; _cancelRender could get reassigned before a task gets to check for cancellation of the old one

            var gameInstall = GetInstallationSettings();
            if (gameInstall == null)
                return; // this happens if there are no data files at all; just do something sensible to avoid crashing

            var images = ctIconsPanel.Children.OfType<TankImageControl>().ToList();
            var renderTasks = ListRenderTasks(gameInstall);

            var style = (Style) ctStyleDropdown.SelectedItem;
            foreach (var layer in style.Layers)
                TestLayer(layer, gameInstall);

            var tasks = new List<Action>();
            for (int i = 0; i < renderTasks.Count; i++)
            {
                if (i >= images.Count)
                    images.Add(CreateTankImageControl());
                var renderTask = renderTasks[i];
                var image = images[i];

                image.ToolTip = renderTasks[i].TankSystemId;
                if (_renderResults.ContainsKey(renderTask.TankSystemId))
                {
                    image.Source = _renderResults[renderTask.TankSystemId].Image;
                    image.RenderTask = _renderResults[renderTask.TankSystemId];
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
                                _renderResults[renderTask.TankSystemId] = renderTask;
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
            string warning = "Some of the tank icons did not render correctly; make sure you view \"All tanks\" and click each broken image for details.";
            bool need = _renderResults.Values.Any(rr => rr.Exception != null);
            bool have = _otherWarnings.Contains(warning);
            if (need && !have)
                _otherWarnings.Add(warning);
            else if (have && !need)
                _otherWarnings.Remove(warning);

            if (!need)
            {
                warning = "Some of the tank icons rendered with warnings; make sure you view \"All tanks\" and click each image with a warning icon for details.";
                need = _renderResults.Values.Any(rr => rr.WarningsCount > 0);
                have = _otherWarnings.Contains(warning);
                if (need && !have)
                    _otherWarnings.Add(warning);
                else if (have && !need)
                    _otherWarnings.Remove(warning);
            }

            // Clean up all those temporary images we've just created and won't be doing again for a while.
            // (this keeps "private bytes" when idle 10-15 MB lower)
            GC.Collect();
        }

        /// <summary>
        /// Tests the specified layer instance for its handling of missing extra properties (and possibly other problems). Adds an
        /// appropriate warning message if a problem is detected.
        /// </summary>
        private void TestLayer(LayerBase layer, GameInstallationSettings gameInstall)
        {
            // Test missing extra properties
            string missingExtraProperties = "when presented with a tank that is missing some \"extra\" properties"; // A bit of a quick hack, but should do the job
            _otherWarnings.Remove(_otherWarnings.Where(w => w.Contains(missingExtraProperties)).FirstOrDefault());
            try
            {
                var tank = new TankTest("test", 5, Country.USSR, Class.Medium, Category.Normal);
                tank.LoadedImageGdi = Ut.NewBitmapGdi();
                tank.LoadedImageWpf = Ut.NewBitmapWpf();
                layer.DrawInternal(tank);
            }
            catch (Exception e)
            {
                if (!(e is StyleUserError))
                    _otherWarnings.Add(("The layer {0} is buggy: it throws a {1} " + missingExtraProperties + ". Please report this to the developer.").Fmt(layer.GetType().Name, e.GetType().Name));
                // The maker must not throw when properties are missing: firstly, for configurable properties the user could select "None"
                // from the drop-down, and secondly, hard-coded properties could simply be missing altogether.
                // (although this could, of course, be a bug in TankIconMaker itself)
            }

            // Test unexpected property values
            string unexpectedProperty = "possibly due to a property value it didn't expect"; // A bit of a quick hack, but should do the job
            _otherWarnings.Remove(_otherWarnings.Where(w => w.Contains(unexpectedProperty)).FirstOrDefault());
            try
            {
                var tank = new TankTest("test", 5, Country.USSR, Class.Medium, Category.Normal);
                tank.PropertyValue = "z"; // very short, so substring/indexing can fail, also not parseable as integer. Hopefully "unexpected enough".
                tank.LoadedImageGdi = Ut.NewBitmapGdi();
                tank.LoadedImageWpf = Ut.NewBitmapWpf();
                layer.DrawInternal(tank);
            }
            catch (Exception e)
            {
                if (!(e is StyleUserError))
                    _otherWarnings.Add(("The layer {0} is buggy: it throws a {1} " + unexpectedProperty + ". Please report this to the developer.").Fmt(layer.GetType().Name, e.GetType().Name));
                // The maker must not throw for unexpected property values: it could issue a warning using tank.AddWarning.
                // (although this could, of course, be a bug in TankIconMaker itself)
            }

            // Test missing images
            string missingImages = "when some of the standard images cannot be found"; // A bit of a quick hack, but should do the job
            _otherWarnings.Remove(_otherWarnings.Where(w => w.Contains(missingImages)).FirstOrDefault());
            try
            {
                var tank = new TankTest("test", 5, Country.USSR, Class.Medium, Category.Normal);
                tank.PropertyValue = "test";
                layer.DrawInternal(tank);
            }
            catch (Exception e)
            {
                if (!(e is StyleUserError))
                    _otherWarnings.Add(("The layer {0} is buggy: it throws a {1} " + missingImages + ". Please report this to the developer.").Fmt(layer.GetType().Name, e.GetType().Name));
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
                renderTask.Image = Ut.NewBitmapWpf(dc =>
                {
                    foreach (var layer in style.Layers)
                        dc.DrawImage(layer.DrawInternal(renderTask.Tank));
                });
            }
            catch (Exception e)
            {
                renderTask.Image = Ut.NewBitmapWpf(dc =>
                {
                    dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)), null, new Rect(0.5, 1.5, 79, 21));
                    var pen = new Pen(e is StyleUserError ? Brushes.Green : Brushes.Red, 2);
                    dc.DrawLine(pen, new Point(1, 2), new Point(79, 21));
                    dc.DrawLine(pen, new Point(79, 2), new Point(1, 21));
                    dc.DrawRectangle(null, new Pen(Brushes.Black, 1), new Rect(0.5, 1.5, 79, 21));
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
        private TankImageControl CreateTankImageControl()
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
                (bool check) => 80.0 * (check ? 5 : 1) / Program.DpiScaleX
            ));
            BindingOperations.SetBinding(img, TankImageControl.HeightProperty, LambdaBinding.New(
                new Binding { Source = ctZoomCheckbox, Path = new PropertyPath(CheckBox.IsCheckedProperty) },
                (bool check) => 24.0 * (check ? 5 : 1) / Program.DpiScaleY
            ));
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
                DlgMessage.ShowInfo("This image rendered without any problems.");

            else if (renderResult.Exception == null)
                DlgMessage.ShowWarning(warnings);

            else if (renderResult.Exception is StyleUserError)
                DlgMessage.ShowWarning(warnings + joiner + "Could not render this image: " + renderResult.Exception.Message);

            else
            {
                string hint = "";
                if (renderResult.Exception is InvalidOperationException && renderResult.Exception.Message.Contains("belongs to a different thread than its parent Freezable"))
                    hint = "Possible cause: the maker reuses a WPF drawing primitive (like Brush) for different tanks without calling Freeze() on it.\n";

                string message = hint
                    + "Exception details: {0}, {1}\n".Fmt(renderResult.Exception.GetType().Name, renderResult.Exception.Message)
                    + Ut.CollapseStackTrace(renderResult.Exception.StackTrace);

                bool copy = DlgMessage.ShowWarning(warnings + joiner + "The maker threw an exception while rendering this image. This is a bug in the maker; please report it.\n\n" + message,
                    "Copy report to &clipboard", "Close") == 0;

                if (copy)
                    try
                    {
                        Clipboard.SetText(message.ToString(), TextDataFormat.UnicodeText);
                        DlgMessage.ShowInfo("Information about the error is now in your clipboard.");
                    }
                    catch { DlgMessage.ShowInfo("Sorry, couldn't copy the error info to clipboard for some reason."); }
            }
        }

        private void SaveSettings()
        {
            Program.Settings.LeftColumnWidth = ctLeftColumn.Width.Value;
            Program.Settings.NameColumnWidth = ctLayerProperties.NameColumnWidth;
            Program.Settings.SaveThreaded();
        }

        private void SaveSettings(object _, SizeChangedEventArgs __)
        {
            SaveSettings();
        }

        private void SaveSettings(object _, EventArgs __)
        {
            SaveSettings();
        }

        private void ctStyleDropdown_SelectionChanged(object sender = null, SelectionChangedEventArgs __ = null)
        {
            _renderResults.Clear();
            ScheduleUpdateIcons();
            var style = (Style) ctStyleDropdown.SelectedItem;
            Program.Settings.SelectedStyleNameAndAuthor = style.ToString();
            SaveSettings();
            ctLayersTree.ItemsSource = style.Layers;
            if (style.Layers.Count > 0)
            {
                style.Layers[0].TreeViewItem.IsSelected = true;
                ctLayerProperties.SelectedObject = style.Layers[0];
            }
            else
                ctLayerProperties.SelectedObject = null;
        }

        /// <summary>
        /// Constructs a list of render tasks based on the current settings in the GUI. Will enumerate only some
        /// of the tanks if the user chose a smaller subset in the GUI.
        /// </summary>
        /// <param name="all">Forces the method to enumerate all tanks regardless of the GUI setting.</param>
        private List<RenderTask> ListRenderTasks(GameInstallationSettings gameInstall, bool all = false)
        {
            var builtin = Program.Data.BuiltIn.Where(b => b.GameVersion <= gameInstall.GameVersion.Version).MaxOrDefault(b => b.GameVersion);
            if (builtin == null)
                return new List<RenderTask>(); // happens when there are no built-in data files

            IEnumerable<TankData> selection = null;
            if (all || ctDisplayMode.SelectedIndex == 0) // all tanks
                selection = builtin.Data;
            else if (ctDisplayMode.SelectedIndex == 1) // one of each
                selection = builtin.Data.Select(t => new { t.Category, t.Class, t.Country }).Distinct()
                    .SelectMany(p => SelectTiers(builtin.Data.Where(t => t.Category == p.Category && t.Class == p.Class && t.Country == p.Country)));

            var extras = Program.Data.Extra.GroupBy(df => new { df.Name, df.Language, df.Author })
                .Select(g => g.Where(df => df.GameVersion <= gameInstall.GameVersion.Version).MaxOrDefault(df => df.GameVersion))
                .Where(df => df != null).ToList();
            return selection.OrderBy(t => t.Country).ThenBy(t => t.Class).ThenBy(t => t.Tier).ThenBy(t => t.Category).ThenBy(t => t.SystemId)
                .Select(tank =>
                {
                    var task = new RenderTask();
                    task.TankSystemId = tank.SystemId;
                    task.Tank = new Tank(
                        tank,
                        extras.Select(df => new KeyValuePair<ExtraPropertyId, string>(
                            key: new ExtraPropertyId(df.Name, df.Language, df.Author),
                            value: df.Data.Where(dp => dp.TankSystemId == tank.SystemId).Select(dp => dp.Value).FirstOrDefault()
                        )),
                        gameInstall: gameInstall,
                        gameVersion: gameInstall.GameVersion,
                        addWarning: task.AddWarning
                    );
                    return task;
                }).ToList();
        }

        /// <summary>
        /// Enumerates up to three tanks with tiers as different as possible. Ideally enumerates one tier 1, one tier 5 and one tier 10 tank.
        /// </summary>
        private IEnumerable<TankData> SelectTiers(IEnumerable<TankData> tanks)
        {
            TankData min = null;
            TankData mid = null;
            TankData max = null;
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

        private void ctGameVersion_SelectionChanged(object _, SelectionChangedEventArgs args)
        {
            var added = args.AddedItems.OfType<GameVersion>().ToList();
            if (added.Count != 1)
                return;

            var gis = GetInstallationSettings();
            if (gis == null)
                return;
            gis.GameVersion = added.First();
            ctGamePath.SelectedItem = gis;
            SaveSettings();
            UpdateDataSources(gis.GameVersion.Version);
            _renderResults.Clear();
            ScheduleUpdateIcons();
        }

        private void ctGamePath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var gis = GetInstallationSettings();
            ctGameVersion.SelectedItem = gis.NullOr(g => g.GameVersion);
            if (gis == null)
                return;
            Program.Settings.SelectedGamePath = gis.Path;
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
            var style = ctStyleDropdown.SelectedItem as Style;
            if (style.BuiltIn)
            {
                GetEditableStyle(); // duplicate the style
                RecreateBuiltInStyles();
            }
            _renderResults.Clear();
            ScheduleUpdateIcons();
            Program.Settings.SaveThreaded();
        }

        private void ctDisplayMode_SelectionChanged(object _, SelectionChangedEventArgs __)
        {
            Program.Settings.DisplayMode = ctDisplayMode.SelectedIndex;
            UpdateIcons();
            SaveSettings();
        }

        private void ctWarning_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DlgMessage.ShowWarning(string.Join("\n\n", _dataWarnings.Concat(_otherWarnings).Select(s => "• " + s)));
        }

        private void ctReload_Click(object sender, RoutedEventArgs e)
        {
            ReloadData();
        }

        bool _overwriteAccepted = false;

        private void ctSave_Click(object _, RoutedEventArgs __)
        {
            var gameInstall = GetInstallationSettings();
            if (gameInstall == null)
            {
                DlgMessage.ShowInfo("Please add a game path first (top left, green plus button) so that TankIconMaker knows where to save them.");
                return;
            }

            var path = Path.Combine(gameInstall.Path, gameInstall.GameVersion.PathDestination);

            if (!_overwriteAccepted && Directory.Exists(path))
                if (DlgMessage.ShowQuestion("Would you like to overwrite your current icons?\n\nPath: {0}\n\nWarning: ALL .tga files in this path will be overwritten, and there is NO UNDO for this!"
                    .Fmt(path), "&Yes, overwrite all files", "&Cancel") == 1)
                    return;
            _overwriteAccepted = true;
            Directory.CreateDirectory(path);

            GlobalStatusShow("Saving...");

            var style = (Style) ctStyleDropdown.SelectedItem;
            var renderTasks = ListRenderTasks(gameInstall, all: true);
            var renders = _renderResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var renderTask in renderTasks)
                        if (!renders.ContainsKey(renderTask.TankSystemId))
                        {
                            renders[renderTask.TankSystemId] = renderTask;
                            RenderTank(style, renderTask);
                        }
                    foreach (var kvp in renders.Where(kvp => kvp.Value.Exception == null))
                        Targa.Save(kvp.Value.Image, Path.Combine(path, kvp.Key + ".tga"));
                }
                finally
                {
                    Dispatcher.Invoke((Action) GlobalStatusHide);
                }

                Dispatcher.Invoke((Action) (() =>
                {
                    foreach (var kvp in renders)
                        if (!_renderResults.ContainsKey(kvp.Key))
                            _renderResults[kvp.Key] = kvp.Value;
                    int skipped = renders.Values.Count(rr => rr.Exception != null);
                    DlgMessage.ShowInfo("Saved!\nEnjoy." + (skipped == 0 ? "" : "\n\nNote that {0} images were skipped due to errors.").Fmt(skipped));
                }));
            });
        }

        private void ctAbout_Click(object sender, RoutedEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string version = assembly.GetName().Version.Major.ToString().PadLeft(3, '0');
            string copyright = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false).OfType<AssemblyCopyrightAttribute>().Select(c => c.Copyright).FirstOrDefault();
            var icon = Icon as BitmapSource;
            new DlgMessage()
            {
                Message = "Tank Icon Maker\nVersion " + version + "\nBy Romkyns\n\n" + copyright,
                Caption = "Tank Icon Maker",
                Image = icon == null ? null : icon.ToGdi().GetBitmapCopy()
            }.Show();
        }

        private void AddGamePath(object _, RoutedEventArgs __)
        {
            GameInstallationSettings gis;

            // Add the very first path differently: by guessing where the game is installed
            if (Program.Settings.GameInstalls.Count == 0)
            {
                gis = GuessTanksLocationAndVersion();
            }
            else
            {
                var dlg = new VistaFolderBrowserDialog();
                gis = GetInstallationSettings();
                if (gis != null && Directory.Exists(gis.Path))
                    dlg.SelectedPath = gis.Path;
                if (dlg.ShowDialog() != true)
                    return;

                var best = Program.Data.Versions.Where(v => File.Exists(Path.Combine(dlg.SelectedPath, v.CheckFileName))).ToList();
                if (best.Count == 0)
                {
                    if (DlgMessage.ShowWarning("This directory does not appear to contain a supported version of World Of Tanks. Are you sure you want to use it anyway?",
                        "&Use anyway", "Cancel") == 1)
                        return;
                }
                var version = best.Where(v => Ut.FileContains(Path.Combine(dlg.SelectedPath, v.CheckFileName), v.CheckFileContent))
                    .OrderByDescending(v => v.Version)
                    .FirstOrDefault();

                gis = new GameInstallationSettings { Path = dlg.SelectedPath, GameVersion = version ?? Program.Data.GetLatestVersion() };
            }

            Program.Settings.GameInstalls.Add(gis);
            Program.Settings.SaveThreaded();

            ctGamePath.SelectedItem = gis;
        }

        private void RemoveGamePath(object _ = null, RoutedEventArgs __ = null)
        {
            // Looks rather hacky but seems to do the job correctly even when called with the drop-down visible.
            var index = ctGamePath.SelectedIndex;
            Program.Settings.GameInstalls.RemoveAt(ctGamePath.SelectedIndex);
            ctGamePath.ItemsSource = null;
            ctGamePath.ItemsSource = Program.Settings.GameInstalls;
            ctGamePath.SelectedIndex = Math.Min(index, Program.Settings.GameInstalls.Count - 1);
            SaveSettings();
        }

        /// <summary>
        /// Returns the currently selected game installation settings, or null iff there are no paths in the list.
        /// </summary>
        private GameInstallationSettings GetInstallationSettings()
        {
            if (!Program.Data.Versions.Any())
                return null;

            if (ctGamePath.SelectedItem == null && ctGamePath.Items.Count > 0)
                ctGamePath.SelectedIndex = 0;

            return ctGamePath.SelectedItem as GameInstallationSettings;
        }

        /// <summary>
        /// Creates and returns a new instance of <see cref="GameInstallationSettings"/>, pointing to the most likely location of
        /// the World of Tanks installation, and either the exact matching game version or the latest of the versions we support.
        /// </summary>
        private GameInstallationSettings GuessTanksLocationAndVersion()
        {
            var path = Ut.FindTanksDirectory();
            var version = Program.Data.GetGuessedVersion(path);

            return new GameInstallationSettings { Path = path, GameVersion = version ?? Program.Data.GetLatestVersion() };
        }

        private void ctStyleMore_Click(object sender, RoutedEventArgs e)
        {
            ctStyleMore.ContextMenu.PlacementTarget = ctStyleMore;
            ctStyleMore.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            ctStyleMore.ContextMenu.IsOpen = true;
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
            var style = ctStyleDropdown.SelectedItem as Style;
            if (style.BuiltIn)
            {
                // Remember what was expanded and selected
                var layer = ctLayersTree.SelectedItem as LayerBase;
                var effect = ctLayersTree.SelectedItem as EffectBase;
                int selectedLayerIndex = layer == null && effect == null ? -1 : style.Layers.IndexOf(layer ?? effect.Layer);
                int selectedEffectIndex = effect == null ? -1 : effect.Layer.Effects.IndexOf(effect);
                var expandedIndexes = style.Layers.Select((l, i) => l.TreeViewItem.IsExpanded ? i : -1).Where(i => i >= 0).ToArray();
                // Duplicate
                style = style.Clone();
                style.BuiltIn = false;
                style.Name = style.Name + " (copy)";
                Program.Settings.Styles.Add(style);
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
            }
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

        private bool cmdLayer_AddEffect_IsAvailable()
        {
            return ctLayersTree.SelectedItem is LayerBase || ctLayersTree.SelectedItem is EffectBase;
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
            Dispatcher.BeginInvoke((Action) (() =>
            {
                newEffect.TreeViewItem.IsSelected = true;
                newEffect.TreeViewItem.BringIntoView();
                UpdateIcons();
            }), DispatcherPriority.Background);
        }

        private bool cmdLayer_Rename_IsAvailable()
        {
            return ctLayersTree.SelectedItem is LayerBase;
        }

        private void cmdLayer_Rename(object sender, ExecutedRoutedEventArgs e)
        {
            var layer = ctLayersTree.SelectedItem as LayerBase;
            var newName = PromptWindow.ShowPrompt(this, layer.Name, "Rename layer", "Layer _name:");
            if (newName == null)
                return;
            var style = GetEditableStyle();
            layer = ctLayersTree.SelectedItem as LayerBase;
            layer.Name = newName;
            SaveSettings();
        }

        private bool cmdLayer_Delete_IsAvailable()
        {
            return ctLayersTree.SelectedItem is LayerBase || ctLayersTree.SelectedItem is EffectBase;
        }

        private void cmdLayer_Delete(object sender, ExecutedRoutedEventArgs e)
        {
            if (DlgMessage.ShowQuestion("Delete the selected layer/effect?", "&Delete", "&Cancel") == 1)
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
            var style = ctStyleDropdown.SelectedItem as Style;
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

        private void cmdStyle_Add(object sender, ExecutedRoutedEventArgs e)
        {
            var name = PromptWindow.ShowPrompt(this, "New style", "Create style", "New style _name:");
            if (name == null)
                return;
            var style = new Style();
            style.Name = name;
            style.Author = "me";
            style.Layers.Add(new TankImageLayer { Name = "Tank image" });
            Program.Settings.Styles.Add(style);
            ctStyleDropdown.SelectedItem = style;
            SaveSettings();
        }

        private bool cmdStyle_NonBuiltInStyleSelected()
        {
            var style = ctStyleDropdown.SelectedItem as Style;
            if (style == null) return false;
            return !style.BuiltIn;
        }

        private void cmdStyle_Delete(object sender, ExecutedRoutedEventArgs e)
        {
            var style = ctStyleDropdown.SelectedItem as Style;
            if (DlgMessage.ShowQuestion("Delete this style?\r\n\r\n" + style.Name, "&Delete", "&Cancel") == 1)
                return;
            if (ctStyleDropdown.SelectedIndex < ctStyleDropdown.Items.Count - 1)
                ctStyleDropdown.SelectedIndex++;
            else
                ctStyleDropdown.SelectedIndex--;
            Program.Settings.Styles.Remove(style);
            SaveSettings();
        }

        private void cmdStyle_ChangeName(object sender, ExecutedRoutedEventArgs e)
        {
            var style = ctStyleDropdown.SelectedItem as Style;
            var name = PromptWindow.ShowPrompt(this, style.Name, "Change style name", "New style _name:");
            if (name == null)
                return;
            style.Name = name;
            SaveSettings();
        }

        private void cmdStyle_ChangeAuthor(object sender, ExecutedRoutedEventArgs e)
        {
            var style = ctStyleDropdown.SelectedItem as Style;
            var author = PromptWindow.ShowPrompt(this, style.Author, "Change style author", "New style _author:");
            if (author == null)
                return;
            style.Author = author;
            SaveSettings();
        }

        private void cmdStyle_Duplicate(object sender, ExecutedRoutedEventArgs e)
        {
            var style = ctStyleDropdown.SelectedItem as Style;
            var name = PromptWindow.ShowPrompt(this, style.Name + " (copy)", "Duplicate style", "New style _name:");
            if (name == null)
                return;
            style = style.Clone();
            style.BuiltIn = false;
            style.Name = name;
            Program.Settings.Styles.Add(style);
            ctStyleDropdown.SelectedItem = style;
            SaveSettings();
        }

        private void cmdStyle_Import(object sender, ExecutedRoutedEventArgs e)
        {
            DlgMessage.ShowWarning("Sorry, not done yet...");
            //var dlg = new VistaOpenFileDialog();
            //dlg.Filter = "Icon maker settings|*.xml|All files|*.*";
            //dlg.FilterIndex = 0;
            //dlg.Multiselect = false;
            //dlg.CheckFileExists = true;
            //if (dlg.ShowDialog() != true)
            //    return;

            //MakerBase newMaker;
            //try
            //{
            //    var oldMaker = ctStyleDropdown.SelectedItem as MakerBase;
            //    newMaker = XmlClassify.LoadObjectFromXmlFile<MakerBase>(dlg.FileName);
            //}
            //catch
            //{
            //    DlgMessage.ShowWarning("Could not load the file for some reason. It might be of the wrong format.");
            //    return;
            //}

            ////if (newMaker.GetType() != oldMaker.GetType())
            ////    if (DlgMessage.ShowQuestion("These settings are for maker \"{0}\". You currently have \"{1}\" selected. Load anyway?".Fmt(newMaker.ToString(), oldMaker.ToString()),
            ////        "&Load", "Cancel") == 1)
            ////        return;

            //int i = Program.Settings.Makers.Select((maker, index) => new { maker, index }).First(x => x.maker.GetType() == newMaker.GetType()).index;
            //ctStyleDropdown.SelectedItem = newMaker;
            //ctStyleDropdown.Items[i] = newMaker;
            //Program.Settings.Makers[i] = newMaker;
            //Program.Settings.SaveThreaded();
            //ctStyleDropdown_SelectionChanged();

            //_makerSettingsFilename = dlg.FileName;
            //_makerSettingsConfirmSave = true;
        }

        private void cmdStyle_Export(object sender, ExecutedRoutedEventArgs e)
        {
            DlgMessage.ShowWarning("Sorry, not done yet...");
            //private void ctMakerSave_Click(object _ = null, RoutedEventArgs __ = null)
            //{
            //    if (_makerSettingsFilename == null)
            //    {
            //        ctMakerSaveAs_Click();
            //        return;
            //    }
            //    else if (_makerSettingsConfirmSave)
            //        if (DlgMessage.ShowQuestion("Save maker settings?\n\nFile: {0}".Fmt(_makerSettingsFilename), "&Save", "Cancel") == 1)
            //            return;
            //    XmlClassify.SaveObjectToXmlFile(ctStyleDropdown.SelectedItem, typeof(MakerBase), _makerSettingsFilename);
            //    ctMakerSave.IsEnabled = false;
            //    _makerSettingsConfirmSave = false;
            //}

            //private void ctMakerSaveAs_Click(object _ = null, RoutedEventArgs __ = null)
            //{
            //    var dlg = new VistaSaveFileDialog();
            //    dlg.Filter = "Icon maker settings|*.xml|All files|*.*";
            //    dlg.FilterIndex = 0;
            //    dlg.CheckPathExists = true;
            //    if (dlg.ShowDialog() != true)
            //        return;
            //    _makerSettingsFilename = dlg.FileName;
            //    if (_makerSettingsFilename.ToLower().EndsWith(".xml"))
            //        _makerSettingsFilename = _makerSettingsFilename.Substring(0, _makerSettingsFilename.Length - 4);
            //    _makerSettingsFilename = "{0} ({1}).xml".Fmt(_makerSettingsFilename, ctStyleDropdown.SelectedItem.GetType().Name);
            //    _makerSettingsConfirmSave = false;
            //    ctMakerSave_Click();
            //}
        }
    }

    static class TankLayerCommands
    {
        public static RoutedCommand AddLayer = new RoutedCommand();
        public static RoutedCommand AddEffect = new RoutedCommand();
        public static RoutedCommand Rename = new RoutedCommand();
        public static RoutedCommand Delete = new RoutedCommand();
        public static RoutedCommand MoveUp = new RoutedCommand();
        public static RoutedCommand MoveDown = new RoutedCommand();
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
    }

    /// <summary>
    /// Holds all the information needed to render one tank image during the rendering stage, and the render results afterwards.
    /// </summary>
    sealed class RenderTask
    {
        /// <summary>System Id of the tank that this task is for.</summary>
        public string TankSystemId;
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

            dc.PushTransform(new TranslateTransform(50 * scale - 7 / Program.DpiScaleX, cy));
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
