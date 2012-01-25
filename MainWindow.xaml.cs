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

/*
 * Load/save sets of properties to XML files (make sure distribution is well-supported)
 * 
 * Ensure all the graphics APIs have GDI and WPF variants
 * Good handling of exceptions in the maker: show a graphic for the failed tank; show what's wrong on click. Detect common errors like the shared resource usage exception
 * Good handling of exceptions due to bugs in the program (show detail and exit)
 * Test-render a tank with all null properties and tell the user if this fails (and deduce which property fails)
 * Test inheritance use-case: override a few properties from someone else's data, but for new version be able to import their new file with your overrides
 * 
 * ctGameVersions: use binding; use DisplayName
 * GameInstallationSettings should use the Version type for the game version.
 * In-game-like display of low/mid/high tier balance
 * Allow the maker to tell us which tanks to invalidate on a property change.
 * Drop-down for selecting backgrounds; Reload'able
 */

namespace TankIconMaker
{
    partial class MainWindow : ManagedWindow
    {
        private List<MakerBase> _makers = new List<MakerBase>();
        private WotData _data = new WotData();
        private DispatcherTimer _updateIconsTimer = new DispatcherTimer(DispatcherPriority.Background);
        private CancellationTokenSource _cancelRender = new CancellationTokenSource();
        private Dictionary<string, BitmapSource> _renderCache = new Dictionary<string, BitmapSource>();
        private ObservableCollection<string> _warnings = new ObservableCollection<string>();

        public MainWindow()
            : base(Program.Settings.MainWindow)
        {
            InitializeComponent();
            _updateIconsTimer.Tick += UpdateIcons;
            _updateIconsTimer.Interval = TimeSpan.FromMilliseconds(100);

            GlobalStatusShow("Loading...");

            BindingOperations.SetBinding(ctRemoveGamePath, Button.IsEnabledProperty, new Binding
            {
                Source = ctGamePath,
                Path = new PropertyPath(ComboBox.SelectedIndexProperty),
                Converter = LambdaConverter.New((int index) => index >= 0),
            });
            BindingOperations.SetBinding(ctGameVersion, ComboBox.IsEnabledProperty, new Binding
            {
                Source = ctGamePath,
                Path = new PropertyPath(ComboBox.SelectedIndexProperty),
                Converter = LambdaConverter.New((int index) => index >= 0),
            });
            BindingOperations.SetBinding(ctWarning, Image.VisibilityProperty, new Binding
            {
                Source = _warnings,
                Path = new PropertyPath("Count"),
                Converter = LambdaConverter.New((int count) => count > 0 ? Visibility.Visible : Visibility.Collapsed)
            });

            if (Program.Settings.LeftColumnWidth != null)
                ctLeftColumn.Width = new GridLength(Program.Settings.LeftColumnWidth.Value);
            if (Program.Settings.NameColumnWidth != null)
                ctMakerProperties.NameColumnWidth = Program.Settings.NameColumnWidth.Value;
            if (Program.Settings.DisplayMode >= 0 && Program.Settings.DisplayMode < ctDisplayMode.Items.Count)
                ctDisplayMode.SelectedIndex = Program.Settings.DisplayMode.Value;
            ctGamePath.ItemsSource = Program.Settings.GameInstalls;
            ctGamePath.DisplayMemberPath = "DisplayName";
            ctGamePath.SelectedItem = Program.Settings.GameInstalls.FirstOrDefault(gis => gis.Path.EqualsNoCase(Program.Settings.SelectedGamePath))
                ?? Program.Settings.GameInstalls.FirstOrDefault();

            ContentRendered += InitializeEverything;
        }

        private void GlobalStatusShow(string message)
        {
            (ctGlobalStatusBox.Child as TextBlock).Text = message;
            ctGlobalStatusBox.Visibility = Visibility.Visible;
            IsEnabled = false;
            ctIconsPanel.Opacity = 0.6;
        }

        private void GlobalStatusHide()
        {
            IsEnabled = true;
            ctGlobalStatusBox.Visibility = Visibility.Collapsed;
            ctIconsPanel.Opacity = 1;
        }

        void InitializeEverything(object _, EventArgs __)
        {
            ContentRendered -= InitializeEverything;

            if (File.Exists(Path.Combine(PathUtil.AppPath, "Data", "background.jpg")))
                ctOuterGrid.Background = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(Path.Combine(PathUtil.AppPath, "Data", "background.jpg"))),
                    Stretch = Stretch.UniformToFill,
                };

            // Find all the makers
            foreach (var makerType in Assembly.GetEntryAssembly().GetTypes().Where(t => typeof(MakerBase).IsAssignableFrom(t) && !t.IsAbstract))
            {
                var constructor = makerType.GetConstructor(new Type[0]);
                if (constructor == null)
                {
                    DlgMessage.ShowWarning("Ignored maker type \"{0}\" because it does not have a public parameterless constructor.".Fmt(makerType));
                    continue;
                }
                var maker = (MakerBase) constructor.Invoke(new object[0]);
                _makers.Add(maker);
            }

            _makers = _makers.OrderBy(m => m.Name).ThenBy(m => m.Author).ThenBy(m => m.Version).ToList();

            // Put the makers into the maker dropdown
            foreach (var maker in _makers)
                ctMakerDropdown.Items.Add(maker);

            // Locate the closest match for the maker that was selected last time the program was run
            ctMakerDropdown.SelectedItem = _makers
                .OrderBy(m => m.GetType().FullName == Program.Settings.SelectedMakerType ? 0 : 1)
                .ThenBy(m => m.Name == Program.Settings.SelectedMakerName ? 0 : 1)
                .ThenBy(m => _makers.IndexOf(m))
                .First();

            ReloadData();

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

        private void ReloadData()
        {
            _renderCache.Clear();

            _data.Reload(Path.Combine(PathUtil.AppPath, "Data"));

            // Update the list of warnings
            _warnings.Clear();
            foreach (var warning in _data.Warnings)
                _warnings.Add(warning);

            // Update UI to reflect whether the bare minimum data files are available
            var filesAvailable = _data.Versions.Any() && _data.BuiltIn.Any();
            ctSave.IsEnabled = filesAvailable;
            ctMakerDropdown.IsEnabled = filesAvailable;
            ctMakerProperties.IsEnabled = filesAvailable;
            ctGameVersion.IsEnabled = filesAvailable;
            if (!filesAvailable)
            {
                DlgMessage.ShowWarning("Found no version files and/or no built-in data files. Make sure the files are available under the following path:\n\n" + Path.Combine(PathUtil.AppPath, "Data"));
                return;
            }

            // Refresh game versions UI (TODO: just use binding)
            ctGameVersion.Items.Clear();
            foreach (var key in _data.Versions.Keys.OrderBy(v => v))
                ctGameVersion.Items.Add(key);
            GetInstallationSettings(); // fixes up the versions if necessary

            // Yes, this stuff is a bit WinForms'sy...
            var gis = GetInstallationSettings(addIfMissing: true);
            ctGameVersion.Text = gis.GameVersion;
            UpdateDataSources(Version.Parse(gis.GameVersion));
            ctMakerDropdown_SelectionChanged();
        }

        /// <summary>
        /// Updates the list of data sources currently available to be used in the icon maker. 
        /// </summary>
        private void UpdateDataSources(Version version)
        {
            foreach (var item in Program.DataSources.Where(ds => !(ds is DataSourceNone)).ToArray())
            {
                var extra = _data.Extra.Where(df => df.Name == item.Name && df.Language == item.Language && df.Author == item.Author && df.GameVersion <= version).MaxOrDefault(df => df.GameVersion);
                if (extra == null)
                    Program.DataSources.Remove(item);
                else
                    item.UpdateFrom(extra);
            }
            foreach (var group in _data.Extra.GroupBy(df => new { df.Name, df.Language, df.Author }))
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
            foreach (var image in ctIconsPanel.Children.OfType<Image>())
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
            foreach (var image in ctIconsPanel.Children.OfType<Image>())
                image.Opacity = 0.7;

            _updateIconsTimer.Stop();
            _cancelRender.Cancel();
            _cancelRender = new CancellationTokenSource();
            var cancelToken = _cancelRender.Token; // must be a local so that the task lambda captures it; _cancelRender could get reassigned before a task gets to check for cancellation of the old one

            var maker = (MakerBase) ctMakerDropdown.SelectedItem;
            maker.Initialize();

            var images = ctIconsPanel.Children.OfType<Image>().ToList();
            var tanks = EnumTanks().ToList();

            var tasks = new List<Action>();
            for (int i = 0; i < tanks.Count; i++)
            {
                if (i >= images.Count)
                {
                    var img = new Image
                    {
                        SnapsToDevicePixels = true,
                        Margin = new Thickness { Right = 15 },
                        Cursor = Cursors.Hand,
                        Opacity = 0.7,
                    };
                    BindingOperations.SetBinding(img, Image.WidthProperty, new Binding
                    {
                        Source = ctZoomCheckbox,
                        Path = new PropertyPath(CheckBox.IsCheckedProperty),
                        Converter = LambdaConverter.New((bool check) => 80 * (check ? 5 : 1)),
                    });
                    BindingOperations.SetBinding(img, Image.HeightProperty, new Binding
                    {
                        Source = ctZoomCheckbox,
                        Path = new PropertyPath(CheckBox.IsCheckedProperty),
                        Converter = LambdaConverter.New((bool check) => 24 * (check ? 5 : 1)),
                    });
                    ctIconsPanel.Children.Add(img);
                    images.Add(img);
                }
                var tank = tanks[i];
                var image = images[i];

                image.ToolTip = tanks[i].SystemId + (tanks[i]["OfficialName"] == null ? "" : (" (" + tanks[i]["OfficialName"] + ")"));
                if (_renderCache.ContainsKey(tank.SystemId))
                {
                    image.Source = _renderCache[tank.SystemId];
                    image.Opacity = 1;
                }
                else
                    tasks.Add(() =>
                    {
                        try
                        {
                            if (cancelToken.IsCancellationRequested) return;
                            var source = maker.DrawTankInternal(tank);
                            if (cancelToken.IsCancellationRequested) return;
                            Dispatcher.Invoke(new Action(() =>
                            {
                                if (cancelToken.IsCancellationRequested) return;
                                _renderCache[tank.SystemId] = source;
                                image.Source = source;
                                image.Opacity = 1;
                                if (ctIconsPanel.Children.OfType<Image>().All(c => c.Opacity == 1))
                                    UpdateIconsCompleted();
                            }));
                        }
                        catch { } // will do something more appropriate later
                    });
            }
            foreach (var task in tasks)
                Task.Factory.StartNew(task, cancelToken);

            // Remove unused images
            foreach (var image in images.Skip(tanks.Count))
                ctIconsPanel.Children.Remove(image);
        }

        /// <summary>
        /// Called on the GUI thread whenever all the icon renders are completed.
        /// </summary>
        private void UpdateIconsCompleted()
        {
            ctSave.IsEnabled = true;
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

        private void ctMakerDropdown_SelectionChanged(object _ = null, SelectionChangedEventArgs __ = null)
        {
            _renderCache.Clear();
            ScheduleUpdateIcons();
            var maker = (MakerBase) ctMakerDropdown.SelectedItem;
            maker.Initialize();
            ctMakerProperties.SelectedObject = maker;
            ctMakerDescription.Text = maker.Description ?? "";
            Program.Settings.SelectedMakerType = maker.GetType().FullName;
            Program.Settings.SelectedMakerName = maker.Name;
            SaveSettings();
        }

        private IEnumerable<Tank> EnumTanks(bool all = false)
        {
            var gis = GetInstallationSettings();
            if (gis == null)
                return new Tank[0]; // this happens if there are no data files at all; just do something sensible to avoid crashing

            var selectedVersion = Version.Parse(gis.GameVersion);

            var builtin = _data.BuiltIn.Where(b => b.GameVersion <= selectedVersion).MaxOrDefault(b => b.GameVersion);
            IEnumerable<TankData> selection = null;

            if (all || ctDisplayMode.SelectedIndex == 0) // all tanks
                selection = builtin.Data;
            else if (ctDisplayMode.SelectedIndex == 1) // one of each
                selection = builtin.Data.Select(t => new { t.Category, t.Class, t.Country }).Distinct()
                    .SelectMany(p => SelectTiers(builtin.Data.Where(t => t.Category == p.Category && t.Class == p.Class && t.Country == p.Country)));

            var extras = _data.Extra.GroupBy(df => new { df.Name, df.Language, df.Author })
                .Select(g => g.Where(df => df.GameVersion <= selectedVersion).MaxOrDefault(df => df.GameVersion))
                .Where(df => df != null).ToList();
            return selection.OrderBy(t => t.Country).ThenBy(t => t.Class).ThenBy(t => t.Tier).ThenBy(t => t.Category).ThenBy(t => t.SystemId)
                .Select(tank => new Tank(
                    tank,
                    extras.Select(df => new KeyValuePair<string, string>(
                        key: df.Name + "/" + df.Language + "/" + df.Author,
                        value: df.Data.Where(dp => dp.TankSystemId == tank.SystemId).Select(dp => dp.Value).FirstOrDefault()
                    )),
                    gameInstall: gis,
                    gameVersion: _data.Versions[selectedVersion]
                )).ToList();
        }

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
            _renderCache.Clear();
            ScheduleUpdateIcons();
        }

        private void ctGameVersion_SelectionChanged(object _, SelectionChangedEventArgs args)
        {
            var added = args.AddedItems.OfType<Version>().ToList();
            if (added.Count != 1)
                return;

            var gis = GetInstallationSettings();
            if (gis == null)
                return;
            gis.GameVersion = added.FirstOrDefault().ToString();
            ctGamePath.SelectedItem = gis;
            SaveSettings();
            UpdateDataSources(Version.Parse(gis.GameVersion));
            _renderCache.Clear();
            ScheduleUpdateIcons();
        }

        void ctGamePath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var gis = GetInstallationSettings();
            if (gis == null)
                return;
            ctGameVersion.Text = gis.GameVersion;
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
            DlgMessage.ShowWarning(string.Join("\n\n", _warnings.Select(s => "• " + s)));
        }

        private void ctReload_Click(object sender, RoutedEventArgs e)
        {
            ReloadData();
            UpdateIcons();
        }

        bool _overwriteAccepted = false;

        private void ctSave_Click(object _, RoutedEventArgs __)
        {
            var gis = GetInstallationSettings();
            if (gis == null)
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
            var tanks = EnumTanks(all: true).ToList();
            var renders = _renderCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var tank in tanks)
                        if (!renders.ContainsKey(tank.SystemId))
                            renders[tank.SystemId] = maker.DrawTankInternal(tank);
                    foreach (var kvp in renders)
                        Targa.Save(kvp.Value, Path.Combine(path, kvp.Key + ".tga"));
                }
                finally
                {
                    Dispatcher.Invoke((Action) GlobalStatusHide);
                }

                Dispatcher.Invoke((Action) (() =>
                {
                    foreach (var kvp in renders)
                        if (!_renderCache.ContainsKey(kvp.Key))
                            _renderCache[kvp.Key] = kvp.Value;
                    DlgMessage.ShowInfo("Saved!\nEnjoy.");
                }));
            });
        }

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

        private void BrowseForGameDirectory(object _, RoutedEventArgs __)
        {
            var dlg = new VistaFolderBrowserDialog();
            var gis = GetInstallationSettings();
            if (gis != null && Directory.Exists(gis.Path))
                dlg.SelectedPath = gis.Path;
            if (dlg.ShowDialog() != true)
                return;

            var best = _data.Versions.Where(v => File.Exists(Path.Combine(dlg.SelectedPath, v.Value.CheckFileName))).ToList();
            if (best.Count == 0)
            {
                if (DlgMessage.ShowWarning("This directory does not appear to contain a World Of Tanks installation. Are you sure you want to use it anyway?",
                    "&Use anyway", "Cancel") == 1)
                    return;
            }
            var version = best.Where(v => new FileInfo(Path.Combine(dlg.SelectedPath, v.Value.CheckFileName)).Length == v.Value.CheckFileSize)
                .Select(v => v.Key)
                .OrderByDescending(v => v)
                .FirstOrDefault();

            gis = new GameInstallationSettings { Path = dlg.SelectedPath, GameVersion = version == null ? _data.Versions.Keys.Max().ToString() : version.ToString() };
            Program.Settings.GameInstalls.Add(gis);
            ctGamePath.SelectedItem = gis;
            Program.Settings.SaveThreaded();
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

        private GameInstallationSettings GetInstallationSettings(bool addIfMissing = false)
        {
            if (!_data.Versions.Any())
                return null;

            var gis = ctGamePath.SelectedItem as GameInstallationSettings;
            if (gis == null)
            {
                if (!addIfMissing)
                    return null;
                gis = new GameInstallationSettings { Path = Ut.FindTanksDirectory(), GameVersion = _data.Versions.Keys.Max().ToString() };
                Program.Settings.GameInstalls.Add(gis);
                ctGamePath.SelectedItem = gis;
                ctGamePath.Items.Refresh();
                Program.Settings.SaveThreaded();
            }

            Version v;
            if (!Version.TryParse(gis.GameVersion, out v) || !_data.Versions.ContainsKey(v))
            {
                gis.GameVersion = ctGameVersion.Text = _data.Versions.Keys.Max().ToString();
                ctGamePath.Items.Refresh();
                Program.Settings.SaveThreaded();
            }

            return gis;
        }

        private string GetIconDestinationPath()
        {
            var gis = GetInstallationSettings();
            if (gis == null)
                return null;
            return Path.Combine(gis.Path, _data.Versions[Version.Parse(gis.GameVersion)].PathDestination);
        }

        private string GetIconSource3DPath()
        {
            var gis = GetInstallationSettings();
            if (gis == null)
                return null;
            return Path.Combine(gis.Path, _data.Versions[Version.Parse(gis.GameVersion)].PathSource3D);
        }
    }
}
