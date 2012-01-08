using System;
using System.Collections.Generic;
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
using RT.Util;
using RT.Util.Dialogs;

/*
 * Must also render anything not already in cache - if not "All"
 * Provide a means to load the in-game images
 * Provide a means to load user-supplied images
 * BytesBitmap to mimick BitmapSource in API (and name BitmapSourceGdi?)
 * Game versions & paths
 * Proper resolution of properties
 *    Description and inheritance source
 * Load/save sets of properties to XML files (make sure distribution is well-supported)
 * "Reload data" button
 * Bundled properties:
 *     Short names for all tanks
 *     Override example for russian colloquial names
 *     v0.7.1 as separate files
 * 
 * Good handling of exceptions in the maker: show a graphic for the failed tank; show what's wrong on click. Detect common errors like the shared resource usage exception
 * Good handling of exceptions due to bugs in the program (show detail and exit)
 * Report file loading errors properly
 * Test-render a tank with all null properties and tell the user if this fails (and deduce which property fails)
 * 
 * Use a drop-down listing all possible properties for NameDataSource
 * In-game-like display of low/mid/high tier balance
 * Allow the maker to tell us which tanks to invalidate on a property change.
 */

/*
 * Inheritance use-cases:
 *   Definitely: override a few properties from someone else's data, but for new version be able to import their new file with your overrides
 */

namespace TankIconMaker
{
    public partial class MainWindow : ManagedWindow
    {
        private string _exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private List<DataFileBuiltIn> _builtin = new List<DataFileBuiltIn>();
        private List<DataFileExtra> _extra = new List<DataFileExtra>();
        private List<MakerBase> _makers = new List<MakerBase>();
        private DispatcherTimer _updateIconsTimer = new DispatcherTimer(DispatcherPriority.Background);
        private CancellationTokenSource _cancelRender = new CancellationTokenSource();
        private Dictionary<string, BitmapSource> _renderCache = new Dictionary<string, BitmapSource>();

        private string _path = Path.Combine(@"I:\Games\WorldOfTanks", @"res\gui\maps\icons\vehicle\contour");
        private string _pathOriginal = Path.Combine(@"I:\Games\WorldOfTanks", @"res\gui\maps\icons\vehicle\contour", "original");

        public MainWindow()
            : base(Program.Settings.MainWindow)
        {
            InitializeComponent();
            _updateIconsTimer.Tick += UpdateIcons;
            _updateIconsTimer.Interval = TimeSpan.FromMilliseconds(100);

            IsEnabled = false;
            if (Program.Settings.LeftColumnWidth != null)
                ctLeftColumn.Width = new GridLength(Program.Settings.LeftColumnWidth.Value);
            if (Program.Settings.NameColumnWidth != null)
                ctMakerProperties.NameColumnWidth = Program.Settings.NameColumnWidth.Value;
            if (Program.Settings.DisplayMode >= 0 && Program.Settings.DisplayMode < ctDisplayMode.Items.Count)
                ctDisplayMode.SelectedIndex = Program.Settings.DisplayMode.Value;

            Closing += (_, __) => SaveSettings();
            ContentRendered += InitializeEverything;

            this.SizeChanged += Window_SizeChanged;
            this.LocationChanged += Window_LocationChanged;
            ctMakerDropdown.SelectionChanged += ctMakerDropdown_SelectionChanged;
            ctMakerProperties.PropertyChanged += ctMakerProperties_PropertyChanged;
            ctDisplayMode.SelectionChanged += ctDisplayMode_SelectionChanged;
        }

        void InitializeEverything(object _, EventArgs __)
        {
            ContentRendered -= InitializeEverything;

            if (File.Exists(Path.Combine(_exePath, "background.jpg")))
                ctOuterGrid.Background = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(Path.Combine(_exePath, "background.jpg"))),
                    Stretch = Stretch.UniformToFill,
                };

            ReloadData();

            // Find all the makers
            foreach (var makerType in Assembly.GetEntryAssembly().GetTypes().Where(t => typeof(MakerBase).IsAssignableFrom(t) && !t.IsAbstract))
            {
                var constructor = makerType.GetConstructor(new Type[0]);
                if (constructor == null)
                {
                    DlgMessage.ShowWarning("Ignoring maker type \"{0}\" because it does not have a public parameterless constructor.".Fmt(makerType));
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

            // Done
            IsEnabled = true;
            ctLoadingBox.Visibility = Visibility.Collapsed;
            _updateIconsTimer.Start();
        }

        private void ReloadData()
        {
            _builtin.Clear();
            _extra.Clear();
            _renderCache.Clear();

            foreach (var fi in new DirectoryInfo(_exePath).GetFiles("Data-*.csv"))
            {
                var parts = fi.Name.Substring(0, fi.Name.Length - 4).Split('-');
                var partsr = parts.Reverse().ToArray();

                if (parts.Length < 5 || parts.Length > 6)
                {
                    Console.WriteLine("Skipping \"{0}\" because it has the wrong number of filename parts.", fi.Name);
                    continue;
                }
                if (parts[1].EqualsNoCase("BuiltIn") && parts.Length != 5)
                {
                    Console.WriteLine("Skipping \"{0}\" because it has too many filename parts for a BuiltIn data file.", fi.Name);
                    continue;
                }
                if (parts.Length == 5 && !parts[1].EqualsNoCase("BuiltIn"))
                {
                    Console.WriteLine("Skipping \"{0}\" because it has too few filename parts for a non-BuiltIn data file.", fi.Name);
                    continue;
                }

                string author = partsr[2].Trim();
                if (author.Length == 0)
                {
                    Console.WriteLine("Skipping \"{0}\" because it has an empty author part in the filename.", fi.Name);
                    continue;
                }

                Version gameVersion;
                if (!Version.TryParse(partsr[1], out gameVersion))
                {
                    Console.WriteLine("Skipping \"{0}\" because it has an unparseable game version part in the filename: \"{1}\".", fi.Name, partsr[1]);
                    continue;
                }

                int fileVersion;
                if (!int.TryParse(partsr[0], out fileVersion))
                {
                    Console.WriteLine("Skipping \"{0}\" because it has an unparseable file version part in the filename: \"{1}\".", fi.Name, partsr[0]);
                    continue;
                }

                if (parts.Length == 5)
                    _builtin.Add(new DataFileBuiltIn(author, gameVersion, fileVersion, fi.FullName));
                else
                {
                    string extraName = parts[1].Trim();
                    if (extraName.Length == 0)
                    {
                        Console.WriteLine("Skipping \"{0}\" because it has an empty property name part in the filename.", fi.Name);
                        continue;
                    }

                    string languageName = parts[2].Trim();
                    if (languageName.Length != 2)
                    {
                        Console.WriteLine("Skipping \"{0}\" because its language name part in the filename is a 2 letter long language code.", fi.Name);
                        continue;
                    }

                    _extra.Add(new DataFileExtra(extraName, languageName, author, gameVersion, fileVersion, fi.FullName));
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
                            source.Freeze();
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

        private void Window_SizeChanged(object _, SizeChangedEventArgs __)
        {
            SaveSettings();
        }

        private void Window_LocationChanged(object _, EventArgs __)
        {
            SaveSettings();
        }

        private void ctMakerDropdown_SelectionChanged(object _, SelectionChangedEventArgs __)
        {
            ScheduleUpdateIcons();
            var maker = (MakerBase) ctMakerDropdown.SelectedItem;
            ctMakerProperties.SelectedObject = maker;
            Program.Settings.SelectedMakerType = maker.GetType().FullName;
            Program.Settings.SelectedMakerName = maker.Name;
            SaveSettings();
        }

        private IEnumerable<Tank> EnumTanks()
        {
#warning Implement property inheritance and languages

            IEnumerable<TankData> all = _builtin.First().Data;
            IEnumerable<TankData> selection = null;

            if (ctDisplayMode.SelectedIndex == 0) // all tanks
                selection = all;
            else if (ctDisplayMode.SelectedIndex == 1) // one of each
                selection = all.Select(t => new { t.Category, t.Class, t.Country }).Distinct()
                    .SelectMany(p => SelectTiers(all.Where(t => t.Category == p.Category && t.Class == p.Class && t.Country == p.Country)));

            return selection.OrderBy(t => t.Country).ThenBy(t => t.Class).ThenBy(t => t.Tier).ThenBy(t => t.Category).ThenBy(t => t.SystemId)
                .Select(tank => new Tank(
                    tank,
                    new[] {
                        new KeyValuePair<string, string>(_extra[1].Name, _extra[1].Data.Where(dp => tank.SystemId == dp.TankSystemId).Select(dp => dp.Value).FirstOrDefault()),
                        new KeyValuePair<string, string>(_extra[2].Name, _extra[2].Data.Where(dp => tank.SystemId == dp.TankSystemId).Select(dp => dp.Value).FirstOrDefault())
                    }
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

        private void ctDisplayMode_SelectionChanged(object _, SelectionChangedEventArgs __)
        {
            Program.Settings.DisplayMode = ctDisplayMode.SelectedIndex;
            UpdateIcons();
            SaveSettings();
        }

        bool _overwriteAccepted = false;

        private void ctSave_Click(object _, RoutedEventArgs __)
        {
            if (!EnsureBackup())
                return;

            if (!_overwriteAccepted)
                if (DlgMessage.ShowQuestion("Would you like to overwrite your current icons?\n\nPath: {0}\n\nWarning: ALL .tga files in this path will be overwritten, and there is NO UNDO for this!"
                    .Fmt(_path), "&Yes, overwrite all files", "&Cancel") == 1)
                    return;
            _overwriteAccepted = true;

            foreach (var kvp in _renderCache)
                Targa.Save(kvp.Value, Path.Combine(_path, kvp.Key + ".tga"));

            DlgMessage.ShowInfo("Saved!\nEnjoy.");
        }

        private bool EnsureBackup()
        {
            try
            {
                IEnumerable<FileInfo> copy;
                var current = new DirectoryInfo(_path).GetFiles("*.tga");
                if (Directory.Exists(_pathOriginal))
                {
                    var original = new DirectoryInfo(_pathOriginal).GetFiles("*.tga");
                    copy = current.Except(original, CustomEqualityComparer<FileInfo>.By(di => di.Name, ignoreCase: true));
                }
                else
                {
                    if (DlgMessage.ShowInfo("TankIconMaker needs to make a backup of your original icons, in case you want them back.\n\nPath: {0}\n\nProceed?"
                        .Fmt(_pathOriginal), "&Make backup", "&Cancel") == 1)
                        return false;
                    copy = current;
                }

                Directory.CreateDirectory(_pathOriginal);
                foreach (var file in copy)
                    file.CopyTo(Path.Combine(_pathOriginal, file.Name));

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
    }
}
