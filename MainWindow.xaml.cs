using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using RT.Util;
using RT.Util.Dialogs;

/*
 * Provide a means to load the in-game images
 * Provide a means to load user-supplied images
 * Proper resolution of properties
 * Short names for all tanks
 * "Save icons" button
 * Load/save sets of properties to XML files (make sure distribution is well-supported)
 * "Reload data" button
 * Good handling of exceptions in the maker: show a graphic for the failed tank; show what's wrong on click. Detect common errors like the shared resource usage exception
 * Test-render a tank with all null properties and tell the user if this fails (and deduce which property fails)
 * In-game-like display of low/mid/high tier balance
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
        private CancellationTokenSource _cancelRender = null;

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
            ctZoomCheckbox.Checked += ctZoomCheckbox_Changed;
            ctZoomCheckbox.Unchecked += ctZoomCheckbox_Changed;
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

        private void ScheduleUpdateIcons()
        {
            if (_cancelRender != null)
                _cancelRender.Cancel();
            _cancelRender = new CancellationTokenSource();

            _updateIconsTimer.Stop();
            _updateIconsTimer.Start();
        }

        private void UpdateIcons(object _ = null, EventArgs __ = null)
        {
            _updateIconsTimer.Stop();
            var maker = (MakerBase) ctMakerDropdown.SelectedItem;
            maker.Initialize();

            var images = ctIconsPanel.Children.OfType<Image>().ToList();
            var tanks = EnumTanks().ToList();

            for (int i = 0; i < tanks.Count; i++)
            {
                if (i >= images.Count)
                {
                    var img = new Image
                    {
                        Width = 80 * (ctZoomCheckbox.IsChecked == true ? 3 : 1), Height = 24 * (ctZoomCheckbox.IsChecked == true ? 3 : 1),
                        SnapsToDevicePixels = true,
                        Margin = new Thickness { Right = 15 },
                        Cursor = Cursors.Hand,
                    };
                    ctIconsPanel.Children.Add(img);
                    images.Add(img);
                }
                var tank = tanks[i];
                var image = images[i];

                image.Opacity = 160;
                image.ToolTip = tanks[i].SystemId + (tanks[i]["OfficialName"] == null ? "" : (" (" + tanks[i]["OfficialName"] + ")"));

                var cancel = _cancelRender.Token;
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (cancel.IsCancellationRequested) return;
                        var source = maker.DrawTankInternal(tank);
                        if (cancel.IsCancellationRequested) return;
                        source.Freeze();
                        Dispatcher.Invoke(new Action(() =>
                        {
                            image.Source = source;
                            image.Opacity = 255;
                        }));
                    }
                    catch { image.Source = null; } // will do something more appropriate later
                }, cancel);
            }

            // Remove unused images
            foreach (var image in images.Skip(tanks.Count))
                ctIconsPanel.Children.Remove(image);
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
                    new[] { new KeyValuePair<string, string>(_extra[1].Name, _extra[1].Data.Where(dp => tank.SystemId == dp.TankSystemId).Select(dp => dp.Value).FirstOrDefault()) }
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

        private void ctZoomCheckbox_Changed(object _, RoutedEventArgs __)
        {
            foreach (var child in ctIconsPanel.Children.OfType<Image>())
            {
                child.Width = 80 * (ctZoomCheckbox.IsChecked == true ? 3 : 1);
                child.Height = 24 * (ctZoomCheckbox.IsChecked == true ? 3 : 1);
            }
        }

        private void ctMakerProperties_PropertyChanged(object _, RoutedEventArgs __)
        {
            ScheduleUpdateIcons();
        }

        private void ctDisplayMode_SelectionChanged(object _, SelectionChangedEventArgs __)
        {
            Program.Settings.DisplayMode = ctDisplayMode.SelectedIndex;
            ScheduleUpdateIcons();
            SaveSettings();
        }
    }
}
