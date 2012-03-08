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
using RT.Util.Xml;

/*
 * GetInstallationSettings: remove addIfMissing
 * Broken IsEnabled on ctGameVersion, ctSave
 * Remove icon backup stuff
 * GameInstallationSettings should use the Version type for the game version.
 * Allow the maker to tell us which tanks to invalidate on a property change.
 * _otherWarnings: tag with warning type to enable reliable removal
 */

namespace TankIconMaker
{
    partial class MainWindow : ManagedWindow
    {
        private DispatcherTimer _updateIconsTimer = new DispatcherTimer(DispatcherPriority.Background);
        private CancellationTokenSource _cancelRender = new CancellationTokenSource();
        private Dictionary<string, RenderTask> _renderResults = new Dictionary<string, RenderTask>();
        private string _makerSettingsFilename = null;
        private bool _makerSettingsConfirmSave = false;
        private static BitmapImage _warningImage;

        private ObservableCollection<string> _dataWarnings = new ObservableCollection<string>();
        private ObservableCollection<string> _otherWarnings = new ObservableCollection<string>();

        public MainWindow()
            : base(Program.Settings.MainWindow)
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

            GlobalStatusShow("Loading...");

            _updateIconsTimer.Tick += UpdateIcons;
            _updateIconsTimer.Interval = TimeSpan.FromMilliseconds(100);

            if (Program.Settings.LeftColumnWidth != null)
                ctLeftColumn.Width = new GridLength(Program.Settings.LeftColumnWidth.Value);
            if (Program.Settings.NameColumnWidth != null)
                ctMakerProperties.NameColumnWidth = Program.Settings.NameColumnWidth.Value;
            if (Program.Settings.DisplayMode >= 0 && Program.Settings.DisplayMode < ctDisplayMode.Items.Count)
                ctDisplayMode.SelectedIndex = Program.Settings.DisplayMode.Value;

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
        private void InitializeEverything(object _, EventArgs __)
        {
            ContentRendered -= InitializeEverything;

            var mat = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            Program.DpiScaleX = mat.M11;
            Program.DpiScaleY = mat.M22;

            if (File.Exists(Path.Combine(PathUtil.AppPath, "Data", "background.jpg")))
                ctOuterGrid.Background = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(Path.Combine(PathUtil.AppPath, "Data", "background.jpg"))),
                    Stretch = Stretch.UniformToFill,
                };

            _warningImage = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Graphics/warning.png"));

            foreach (var constructor in Program.MakerConstructors)
                if (!Program.Settings.Makers.Any(m => m.GetType() == constructor.DeclaringType))
                    Program.Settings.Makers.Add((MakerBase) constructor.Invoke(new object[0]));
            Program.Settings.Makers = Program.Settings.Makers.OrderBy(m => m.Name).ThenBy(m => m.Author).ToList();

            // Put the makers into the maker dropdown
            foreach (var maker in Program.Settings.Makers)
                ctMakerDropdown.Items.Add(maker);

            // Locate the closest match for the maker that was selected last time the program was run
            ctMakerDropdown.SelectedItem = Program.Settings.Makers
                .OrderBy(m => m.GetType().FullName == Program.Settings.SelectedMakerType ? 0 : 1)
                .ThenBy(m => m.Name == Program.Settings.SelectedMakerName ? 0 : 1)
                .ThenBy(m => Program.Settings.Makers.IndexOf(m))
                .First();

            ctGamePath.ItemsSource = Program.Settings.GameInstalls;
            ctGamePath.DisplayMemberPath = "DisplayName";
            ctGameVersion.ItemsSource = Program.Data.Versions; // currently empty because we haven’t loaded it yet
            ctGameVersion.DisplayMemberPath = "DisplayName";

            ReloadData();

            // Set WPF bindings now that all the data we need is loaded
            BindingOperations.SetBinding(ctRemoveGamePath, Button.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = ctGamePath, Path = new PropertyPath(ComboBox.SelectedIndexProperty) },
                (int index) => index >= 0
            ));
            BindingOperations.SetBinding(ctGamePath, ComboBox.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = ctGamePath, Path = new PropertyPath(ComboBox.SelectedIndexProperty) },
                (int index) => index >= 0
            ));
            BindingOperations.SetBinding(ctGameVersion, ComboBox.IsEnabledProperty, LambdaBinding.New(
                new Binding { Source = ctGamePath, Path = new PropertyPath(ComboBox.SelectedIndexProperty) },
                (int index) => index >= 0
            ));
            BindingOperations.SetBinding(ctWarning, Image.VisibilityProperty, LambdaBinding.New(
                new Binding { Source = _dataWarnings, Path = new PropertyPath("Count") },
                new Binding { Source = _otherWarnings, Path = new PropertyPath("Count") },
                (int dataCount, int otherCount) => dataCount + otherCount == 0 ? Visibility.Collapsed : Visibility.Visible
            ));
            var selectedInstall = Program.Settings.GameInstalls.FirstOrDefault(gis => gis.Path.EqualsNoCase(Program.Settings.SelectedGamePath))
                ?? Program.Settings.GameInstalls.FirstOrDefault();
            ctGamePath.SelectedItem = selectedInstall;
            ctGameVersion.SelectedItem = selectedInstall.NullOr(gis => gis.GameVersion);

            // Bind the events now that all the UI is set up as desired
            Closing += (___, ____) => SaveSettings();
            this.SizeChanged += SaveSettings;
            this.LocationChanged += SaveSettings;
            ctMakerDropdown.SelectionChanged += ctMakerDropdown_SelectionChanged;
            ctMakerProperties.PropertyChanged += ctMakerProperties_PropertyChanged;
            ctDisplayMode.SelectionChanged += ctDisplayMode_SelectionChanged;
            ctGameVersion.SelectionChanged += ctGameVersion_SelectionChanged;
            ctGamePath.SelectionChanged += ctGamePath_SelectionChanged;
            ctGamePath.PreviewKeyDown += ctGamePath_PreviewKeyDown;

            // Done
            GlobalStatusHide();
            _updateIconsTimer.Start();
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

            // Update UI to reflect whether the bare minimum data files are available
            var filesAvailable = Program.Data.Versions.Any() && Program.Data.BuiltIn.Any();
            ctSave.IsEnabled = filesAvailable;
            ctMakerDropdown.IsEnabled = filesAvailable;
            ctMakerProperties.IsEnabled = filesAvailable;
            ctGameVersion.IsEnabled = filesAvailable;
            if (!filesAvailable)
                DlgMessage.ShowWarning("Found no version files and/or no built-in data files. Make sure the files are available under the following path:\n\n" + Path.Combine(PathUtil.AppPath, "Data"));

            // Yes, this stuff is a bit WinForms'sy...
            var gis = GetInstallationSettings(addIfMissing: true);
            ctGamePath.Items.Refresh(); // it’s mostly notifiable, but 
            ctGameVersion.Items.Refresh();
            if (gis != null)
            {
                ctGameVersion.SelectedItem = gis.GameVersion;
                UpdateDataSources(gis.GameVersion.Version);
                ctMakerDropdown_SelectionChanged();
            }
            else
            {
                ctMakerProperties.SelectedObject = null;
                _renderResults.Clear();
                ctIconsPanel.Children.Clear();                
            }

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

            ctSave.IsEnabled = false;
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
            ctSave.IsEnabled = false;
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

            var maker = (MakerBase) ctMakerDropdown.SelectedItem;
            maker.Initialize();
            TestMaker(maker, gameInstall);

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
                            RenderTank(maker, renderTask);
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
        }

        /// <summary>
        /// Called on the GUI thread whenever all the icon renders are completed.
        /// </summary>
        private void UpdateIconsCompleted()
        {
            ctSave.IsEnabled = true;

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
        /// Tests the specified maker instance for its handling of missing extra properties (and possibly other problems). Adds an
        /// appropriate warning message if a problem is detected.
        /// </summary>
        private void TestMaker(MakerBase maker, GameInstallationSettings gameInstall)
        {
            // Test missing extra properties
            string missingExtraProperties = "when presented with a tank that is missing some \"extra\" properties"; // A bit of a quick hack, but should do the job
            _otherWarnings.Remove(_otherWarnings.Where(w => w.Contains(missingExtraProperties)).FirstOrDefault());
            try
            {
                var tank = new TankTest("test", 5, Country.USSR, Class.Medium, Category.Normal);
                tank.LoadedImageGdi = Ut.NewBitmapGdi();
                tank.LoadedImageWpf = Ut.NewBitmapWpf();
                maker.DrawTankInternal(tank);
            }
            catch (Exception e)
            {
                if (!(e is MakerUserError))
                    _otherWarnings.Add(("The maker {0} is buggy: it throws a {1} " + missingExtraProperties + ". Please report this to the developer.").Fmt(maker.GetType().Name, e.GetType().Name));
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
                maker.DrawTankInternal(tank);
            }
            catch (Exception e)
            {
                if (!(e is MakerUserError))
                    _otherWarnings.Add(("The maker {0} is buggy: it throws a {1} " + unexpectedProperty + ". Please report this to the developer.").Fmt(maker.GetType().Name, e.GetType().Name));
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
                maker.DrawTankInternal(tank);
            }
            catch (Exception e)
            {
                if (!(e is MakerUserError))
                    _otherWarnings.Add(("The maker {0} is buggy: it throws a {1} " + missingImages + ". Please report this to the developer.").Fmt(maker.GetType().Name, e.GetType().Name));
                // The maker must not throw if the images are missing: it could issue a warning using tank.AddWarning though.
                // (although this could, of course, be a bug in TankIconMaker itself)
            }
        }

        /// <summary>
        /// Executes a render task. Will handle any exceptions in the maker and draw an appropriate substitute image
        /// to draw the user's attention to the problem.
        /// </summary>
        private static void RenderTank(MakerBase maker, RenderTask renderTask)
        {
            try
            {
                renderTask.Image = maker.DrawTankInternal(renderTask.Tank);
            }
            catch (Exception e)
            {
                renderTask.Image = Ut.NewBitmapWpf(dc =>
                {
                    dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)), null, new Rect(0.5, 1.5, 79, 21));
                    var pen = new Pen(e is MakerUserError ? Brushes.Green : Brushes.Red, 2);
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

            else if (renderResult.Exception is MakerUserError)
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
            Program.Settings.NameColumnWidth = ctMakerProperties.NameColumnWidth;
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

        private void ctMakerDropdown_SelectionChanged(object sender = null, SelectionChangedEventArgs __ = null)
        {
            _renderResults.Clear();
            ScheduleUpdateIcons();
            var maker = (MakerBase) ctMakerDropdown.SelectedItem;
            maker.Initialize();
            ctMakerProperties.SelectedObject = maker;
            ctMakerDescription.Text = maker.Description ?? "";
            Program.Settings.SelectedMakerType = maker.GetType().FullName;
            Program.Settings.SelectedMakerName = maker.Name;
            SaveSettings();
            ctMakerSave.IsEnabled = true;
            if (sender != null)
            {
                _makerSettingsFilename = null;
                _makerSettingsConfirmSave = false;
            }
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

        private void ctMakerProperties_PropertyChanged(object _, RoutedEventArgs __)
        {
            _renderResults.Clear();
            ScheduleUpdateIcons();
            ctMakerSave.IsEnabled = true;
            Program.Settings.SaveThreaded();
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

        void ctGamePath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var gis = GetInstallationSettings();
            ctGameVersion.SelectedItem = gis.NullOr(g => g.GameVersion);
            if (gis == null)
                return;
            Program.Settings.SelectedGamePath = gis.Path;
            ReloadData();
            SaveSettings();
        }

        void ctGamePath_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ctGamePath.IsKeyboardFocusWithin && ctGamePath.IsDropDownOpen && e.Key == Key.Delete)
            {
                RemoveGameDirectory();
                e.Handled = true;
            }
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

            if (!EnsureBackup())
                return;

            var path = GetIconDestinationPath();
            if (!_overwriteAccepted)
                if (DlgMessage.ShowQuestion("Would you like to overwrite your current icons?\n\nPath: {0}\n\nWarning: ALL .tga files in this path will be overwritten, and there is NO UNDO for this!"
                    .Fmt(path), "&Yes, overwrite all files", "&Cancel") == 1)
                    return;
            _overwriteAccepted = true;

            GlobalStatusShow("Saving...");

            var maker = (MakerBase) ctMakerDropdown.SelectedItem;
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
                            RenderTank(maker, renderTask);
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

        /// <summary>
        /// Ensures that a backup of the original icons is available and up-to-date (even if the user has upgraded the game since
        /// the last backup). Might prompt the user about the backup. Returns true to indicate that a backup is fine, or false otherwise.
        /// </summary>
        private bool EnsureBackup()
        {
            try
            {
                IEnumerable<FileInfo> copy;
                var path = GetIconDestinationPath();
                var pathOriginal = Path.Combine(path, "original");
                var current = new DirectoryInfo(path).GetFiles("*.tga");
                if (Directory.Exists(pathOriginal))
                {
                    var original = new DirectoryInfo(pathOriginal).GetFiles("*.tga");
                    copy = current.Except(original, CustomEqualityComparer<FileInfo>.By(di => di.Name, ignoreCase: true));
                }
                else
                {
                    if (DlgMessage.ShowInfo("TankIconMaker needs to make a backup of your original icons, in case you want them back.\n\nPath: {0}\n\nProceed?"
                        .Fmt(pathOriginal), "&Make backup", "&Cancel") == 1)
                        return false;
                    copy = current;
                }

                Directory.CreateDirectory(pathOriginal);
                foreach (var file in copy)
                    file.CopyTo(Path.Combine(pathOriginal, file.Name));

                _overwriteAccepted = true;
                return true;
            }
            catch (Exception e)
            {
                DlgMessage.ShowError("Could not check / create backup of the original icons. Please tell the developer!\n\nError details: {0} ({1})."
                    .Fmt(e.Message, e.GetType().Name));
                return false;
            }
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
                var version = best.Where(v => FileContains(Path.Combine(dlg.SelectedPath, v.CheckFileName), v.CheckFileContent))
                    .OrderByDescending(v => v.Version)
                    .FirstOrDefault();

                gis = new GameInstallationSettings { Path = dlg.SelectedPath, GameVersion = version ?? Program.Data.GetLatestVersion() };
            }

            Program.Settings.GameInstalls.Add(gis);
            ctGamePath.SelectedItem = gis;
            Program.Settings.SaveThreaded();
        }

        private bool FileContains(string fileName, string content)
        {
            foreach (var line in File.ReadLines(fileName))
                if (line.Contains(content))
                    return true;
            return false;
        }

        private void RemoveGameDirectory(object _ = null, RoutedEventArgs __ = null)
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
        /// Returns the currently selected game installation settings. Can optionally add an auto-guessed path/version
        /// if the list is empty. Returns null if there are no paths in the list.
        /// </summary>
        private GameInstallationSettings GetInstallationSettings(bool addIfMissing = false)
        {
            if (!Program.Data.Versions.Any())
                return null;

            if (ctGamePath.SelectedItem == null && ctGamePath.Items.Count > 0)
                ctGamePath.SelectedIndex = 0;

            var gis = ctGamePath.SelectedItem as GameInstallationSettings;

            if (gis == null)
            {
                if (!addIfMissing)
                    return null;
                gis = GuessTanksLocationAndVersion();
                Program.Settings.GameInstalls.Add(gis);
                ctGamePath.SelectedItem = gis;
                ctGamePath.Items.Refresh();
                Program.Settings.SaveThreaded();
            }

            return gis;
        }

        /// <summary>
        /// Creates and returns a new instance of <see cref="GameInstallationSettings"/>, pointing to the most likely location of
        /// the World of Tanks installation, and either the exact matching game version or the latest of the versions we support.
        /// </summary>
        private GameInstallationSettings GuessTanksLocationAndVersion()
        {
            var path = Ut.FindTanksDirectory();
            var version = Program.Data.Versions
                .Where(v => File.Exists(Path.Combine(path, v.CheckFileName)) && FileContains(Path.Combine(path, v.CheckFileName), v.CheckFileContent))
                .OrderByDescending(v => v.Version)
                .FirstOrDefault();

            return new GameInstallationSettings { Path = path, GameVersion = version ?? Program.Data.GetLatestVersion() };
        }

        private string GetIconDestinationPath()
        {
            var gis = GetInstallationSettings();
            if (gis == null)
                return null;
            return Path.Combine(gis.Path, gis.GameVersion.PathDestination);
        }

        private void ctMakerDefaults_Click(object sender, RoutedEventArgs e)
        {
            var oldMaker = ctMakerDropdown.SelectedItem as MakerBase;
            if (oldMaker == null || ctMakerDropdown.SelectedIndex < 0)
                return; // shouldn't really happen though
            if (DlgMessage.ShowQuestion("Are you sure you want to reset the settings for this maker to defaults?", "&Reset", "Cancel") == 1)
                return;
            var newMaker = (MakerBase) oldMaker.GetType().GetConstructor(new Type[0]).Invoke(new object[0]);
            ctMakerDropdown.SelectedItem = newMaker;
            ctMakerDropdown.Items[ctMakerDropdown.SelectedIndex] = newMaker;
            Program.Settings.Makers[ctMakerDropdown.SelectedIndex] = newMaker;
            Program.Settings.SaveThreaded();
            ctMakerDropdown_SelectionChanged();
        }

        private void ctMakerLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new VistaOpenFileDialog();
            dlg.Filter = "Icon maker settings|*.xml|All files|*.*";
            dlg.FilterIndex = 0;
            dlg.Multiselect = false;
            dlg.CheckFileExists = true;
            if (dlg.ShowDialog() != true)
                return;

            MakerBase newMaker;
            try
            {
                var oldMaker = ctMakerDropdown.SelectedItem as MakerBase;
                newMaker = XmlClassify.LoadObjectFromXmlFile<MakerBase>(dlg.FileName);
            }
            catch
            {
                DlgMessage.ShowWarning("Could not load the file for some reason. It might be of the wrong format.");
                return;
            }

            //if (newMaker.GetType() != oldMaker.GetType())
            //    if (DlgMessage.ShowQuestion("These settings are for maker \"{0}\". You currently have \"{1}\" selected. Load anyway?".Fmt(newMaker.ToString(), oldMaker.ToString()),
            //        "&Load", "Cancel") == 1)
            //        return;

            int i = Program.Settings.Makers.Select((maker, index) => new { maker, index }).First(x => x.maker.GetType() == newMaker.GetType()).index;
            ctMakerDropdown.SelectedItem = newMaker;
            ctMakerDropdown.Items[i] = newMaker;
            Program.Settings.Makers[i] = newMaker;
            Program.Settings.SaveThreaded();
            ctMakerDropdown_SelectionChanged();

            _makerSettingsFilename = dlg.FileName;
            _makerSettingsConfirmSave = true;
        }

        private void ctMakerSave_Click(object _ = null, RoutedEventArgs __ = null)
        {
            if (_makerSettingsFilename == null)
            {
                ctMakerSaveAs_Click();
                return;
            }
            else if (_makerSettingsConfirmSave)
                if (DlgMessage.ShowQuestion("Save maker settings?\n\nFile: {0}".Fmt(_makerSettingsFilename), "&Save", "Cancel") == 1)
                    return;
            XmlClassify.SaveObjectToXmlFile(ctMakerDropdown.SelectedItem, typeof(MakerBase), _makerSettingsFilename);
            ctMakerSave.IsEnabled = false;
            _makerSettingsConfirmSave = false;
        }

        private void ctMakerSaveAs_Click(object _ = null, RoutedEventArgs __ = null)
        {
            var dlg = new VistaSaveFileDialog();
            dlg.Filter = "Icon maker settings|*.xml|All files|*.*";
            dlg.FilterIndex = 0;
            dlg.CheckPathExists = true;
            if (dlg.ShowDialog() != true)
                return;
            _makerSettingsFilename = dlg.FileName;
            if (_makerSettingsFilename.ToLower().EndsWith(".xml"))
                _makerSettingsFilename = _makerSettingsFilename.Substring(0, _makerSettingsFilename.Length - 4);
            _makerSettingsFilename = "{0} ({1}).xml".Fmt(_makerSettingsFilename, ctMakerDropdown.SelectedItem.GetType().Name);
            _makerSettingsConfirmSave = false;
            ctMakerSave_Click();
        }
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
