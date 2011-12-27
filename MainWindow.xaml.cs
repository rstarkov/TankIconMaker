using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RT.Util;
using RT.Util.Dialogs;

namespace TankIconMaker
{
    public partial class MainWindow : ManagedWindow
    {
        private string _exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private List<DataFileBuiltIn> _builtin = new List<DataFileBuiltIn>();
        private List<DataFileExtra> _extra = new List<DataFileExtra>();
        private List<IconMaker> _makers = new List<IconMaker>();

        public MainWindow()
            : base(Program.Settings.MainWindow)
        {
            InitializeComponent();
            IsEnabled = false;
            ContentRendered += InitializeEverything;
        }

        void InitializeEverything(object _, EventArgs __)
        {
            ContentRendered -= InitializeEverything;

            if (File.Exists(Path.Combine(_exePath, "background.jpg")))
                outerGrid.Background = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(Path.Combine(_exePath, "background.jpg"))),
                    Stretch = Stretch.UniformToFill,
                };

            ReloadData();

            foreach (var makerType in Assembly.GetEntryAssembly().GetTypes().Where(t => typeof(IconMaker).IsAssignableFrom(t) && !t.IsAbstract))
            {
                var constructor = makerType.GetConstructor(new Type[0]);
                if (constructor == null)
                {
                    DlgMessage.ShowWarning("Ignoring maker type \"{0}\" because it does not have a public parameterless constructor.".Fmt(makerType));
                    continue;
                }
                var maker = (IconMaker) constructor.Invoke(new object[0]);
                _makers.Add(maker);
                iconMaker.Items.Add(maker);
            }

            _makers = _makers.OrderBy(m => m.Name).ThenBy(m => m.Author).ThenBy(m => m.Version).ToList();

            iconMaker.SelectedItem = _makers
                .OrderBy(m => m.GetType().FullName == Program.Settings.SelectedMakerType ? 0 : 1)
                .ThenBy(m => m.Name == Program.Settings.SelectedMakerName ? 0 : 1)
                .ThenBy(m => _makers.IndexOf(m))
                .First();

            IsEnabled = true;
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

        private void RemakeIcons()
        {
            var maker = (IconMaker) iconMaker.SelectedItem;
            tankIcons.Children.Clear();
            foreach (var tank in DistinctTanks(EnumTanks()))
            {
                tankIcons.Children.Add(new Image
                {
                    Source = maker.DrawTankInternal(tank),
                    Width = 80, Height = 24,
                    SnapsToDevicePixels = true,
                    Margin = new Thickness { Right = 15 },
                    ToolTip = tank.SystemId + (tank["OfficialName"] == null ? "" : (" (" + tank["OfficialName"] + ")")),
                });
            }
        }

        private void Window_SizeChanged(object _, SizeChangedEventArgs __)
        {
            Program.Settings.SaveThreaded();
        }

        private void Window_LocationChanged(object _, EventArgs __)
        {
            Program.Settings.SaveThreaded();
        }

        private void iconMaker_SelectionChanged(object _, SelectionChangedEventArgs __)
        {
            RemakeIcons();
            var maker = (IconMaker) iconMaker.SelectedItem;
            makerProperties.SelectedObject = maker;
            Program.Settings.SelectedMakerType = maker.GetType().FullName;
            Program.Settings.SelectedMakerName = maker.Name;
            Program.Settings.SaveThreaded();
        }

        private IEnumerable<Tank> EnumTanks()
        {
            return _builtin.First().Data.Select(tank => new Tank(
                tank,
                new[] { new KeyValuePair<string, string>(_extra[1].Name, _extra[1].Data.Where(dp => tank.SystemId == dp.TankSystemId).Select(dp => dp.Value).FirstOrDefault()) }
            )).ToList();
        }

        private static IEnumerable<Tank> DistinctTanks(IEnumerable<Tank> tanks)
        {
            var tankList = tanks.ToList();
            return tankList.Select(t => new { t.Category, t.Class, t.Country }).Distinct()
                .Select(p => tankList.First(t => t.Category == p.Category && t.Class == p.Class && t.Country == p.Country));
        }

    }
}
