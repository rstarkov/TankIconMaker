using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using RT.Util.Forms;
using RT.Util.Lingo;
using WotDataLib;

namespace TankIconMaker
{
    partial class BulkSaveSettingsWindow : ManagedWindow, INotifyPropertyChanged
    {
        private WotContext _context;
        private Style _style;

        /// <summary>A template for the path where the icons are to be saved.</summary>
        public string PathTemplate { get { return _PathTemplate; } set { _PathTemplate = value; NotifyPropertyChanged("PathTemplate"); } }
        private string _PathTemplate;

        /// <summary>A template for the path where the battleAtlas are to be saved.</summary>
        public string BattleAtlasPathTemplate { get { return _BattleAtlasPathTemplate; } set { _BattleAtlasPathTemplate = value; NotifyPropertyChanged("BattleAtlasPathTemplate"); } }
        private string _BattleAtlasPathTemplate;

        /// <summary>A template for the path where the vehicleMarkersAtlas are to be saved.</summary>
        public string VehicleMarkersAtlasPathTemplate { get { return _VehicleMarkersAtlasPathTemplate; } set { _VehicleMarkersAtlasPathTemplate = value; NotifyPropertyChanged("VehicleMarkersAtlasPathTemplate"); } }
        private string _VehicleMarkersAtlasPathTemplate;

        /// <summary>Enable/disable saving the icons.</summary>
        public bool IconsBulkSaveEnabled
        {
            get { return _IconsBulkSaveEnabled; }
            set
            {
                _IconsBulkSaveEnabled = value;
                NotifyPropertyChanged("IconsBulkSaveEnabled");
            }
        }
        private bool _IconsBulkSaveEnabled = true;

        /// <summary>Enable/disable saving the battleAtlas.</summary>
        public bool BattleAtlasBulkSaveEnabled
        {
            get { return _BattleAtlasBulkSaveEnabled; }
            set
            {
                _BattleAtlasBulkSaveEnabled = value;
                NotifyPropertyChanged("BattleAtlasBulkSaveEnabled");
            }
        }
        private bool _BattleAtlasBulkSaveEnabled = false;

        /// <summary>Enable/disable saving the vehicleMarkersAtlas.</summary>
        public bool VehicleMarkersAtlasBulkSaveEnabled
        {
            get { return _VehicleMarkersAtlasBulkSaveEnabled; }
            set
            {
                _VehicleMarkersAtlasBulkSaveEnabled = value;
                NotifyPropertyChanged("VehicleMarkersAtlasBulkSaveEnabled");
            }
        }
        private bool _VehicleMarkersAtlasBulkSaveEnabled = false;


        internal BulkSaveSettingsWindow()
            : base(App.Settings.BulkSaveSettingsWindow)
        {
            InitializeComponent();
        }

        public BulkSaveSettingsWindow(WotContext context, Style style)
            : this()
        {
            MainWindow.ApplyUiZoom(this);

            Lingo.TranslateWindow(this, App.Translation.BulkSaveSettingsWindow);

            ContentRendered += InitializeEverything;
            _context = context;
            _style = style;
            ctPrompt.Text = string.Format("{0} ({1})", style.Name, style.Author);
        }

        private void InitializeEverything(object _, EventArgs __)
        {
            BindingOperations.SetBinding(ctPathTemplate, TextBlock.TextProperty,
                new Binding
                {
                    Path = new PropertyPath("PathTemplate"),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay
                });
            BindingOperations.SetBinding(ctBattleAtlasPathTemplate, TextBlock.TextProperty,
                new Binding
                {
                    Path = new PropertyPath("BattleAtlasPathTemplate"),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay
                });
            BindingOperations.SetBinding(ctVehicleMarkersAtlasPathTemplate, TextBlock.TextProperty,
                new Binding
                {
                    Path = new PropertyPath("VehicleMarkersAtlasPathTemplate"),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay
                });
            BindingOperations.SetBinding(ctSaveIconsEnabled, CheckBox.IsCheckedProperty,
                new Binding
                {
                    Path = new PropertyPath("IconsBulkSaveEnabled"),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay
                });
            BindingOperations.SetBinding(ctSaveBattleAtlasEnabled, CheckBox.IsCheckedProperty,
                new Binding
                {
                    Path = new PropertyPath("BattleAtlasBulkSaveEnabled"),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay
                });
            BindingOperations.SetBinding(ctSaveVehicleMarkersAtlasEnabled, CheckBox.IsCheckedProperty,
                new Binding
                {
                    Path = new PropertyPath("VehicleMarkersAtlasBulkSaveEnabled"),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay
                });

            this.ctPathTemplate.DataContext = this;
            this.ctBattleAtlasPathTemplate.DataContext = this;
            this.ctVehicleMarkersAtlasPathTemplate.DataContext = this;
            this.ctSaveIconsEnabled.DataContext = this;
            this.ctSaveBattleAtlasEnabled.DataContext = this;
            this.ctSaveVehicleMarkersAtlasEnabled.DataContext = this;
            DataContext = this;

            PathTemplate = _style.PathTemplate;
            BattleAtlasPathTemplate = _style.BattleAtlasPathTemplate;
            VehicleMarkersAtlasPathTemplate = _style.VehicleMarkersAtlasPathTemplate;
            IconsBulkSaveEnabled = _style.IconsBulkSaveEnabled;
            BattleAtlasBulkSaveEnabled = _style.BattleAtlasBulkSaveEnabled;
            VehicleMarkersAtlasBulkSaveEnabled = _style.VehicleMarkersAtlasBulkSaveEnabled;
        }

        private void NotifyPropertyChanged(string name) { PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };

        private void ok(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public static BulkSaveSettingsWindow Show(Window owner, string value, WotContext context, Style style)
        {
            var wnd = new BulkSaveSettingsWindow(context, style) { Owner = owner };
            wnd.ShowDialog();
            return wnd;
        }

        private void ctEditPathTemplate_Click(object _, RoutedEventArgs __)
        {
            var value = PathTemplateWindow.Show(this, PathTemplate, _context, _style);
            if (value == null)
                return;
            PathTemplate = value;
            IconsBulkSaveEnabled = true;
        }

        private void ctEditBattleAtlasPathTemplate_Click(object _, RoutedEventArgs __)
        {
            var value = PathTemplateWindow.Show(this, BattleAtlasPathTemplate, _context, _style);
            if (value == null)
                return;
            BattleAtlasPathTemplate = value;
            BattleAtlasBulkSaveEnabled = true;
        }

        private void ctEditVehicleMarkersAtlasPathTemplate_Click(object _, RoutedEventArgs __)
        {
            var value = PathTemplateWindow.Show(this, VehicleMarkersAtlasPathTemplate, _context, _style);
            if (value == null)
                return;
            VehicleMarkersAtlasPathTemplate = value;
            VehicleMarkersAtlasBulkSaveEnabled = true;
        }
    }
}
