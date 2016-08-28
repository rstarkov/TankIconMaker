using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using RT.Util.Dialogs;
using RT.Util.Forms;
using RT.Util.Lingo;
using WotDataLib;
using WpfCrutches;

namespace TankIconMaker
{
    partial class BulkSaveSettingsWindow : ManagedWindow, INotifyPropertyChanged
    {
        private string _promptSure;
        private string _promptSureYes;
        private WotContext context;
        private Style style;

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
        

        private BulkSaveSettingsWindow()
            : base(App.Settings.BulkSaveSettingsWindow)
        {
            InitializeComponent();
            ContentRendered += InitializeEverything;
            MainWindow.ApplyUiZoom(this);
            Lingo.TranslateWindow(this, App.Translation.BulkSaveSettingsWindow);
        }

        public BulkSaveSettingsWindow(WotContext context, Style style)
            : this()
        {
            this.context = context;
            this.style = style;
        }

        private void InitializeEverything(object ___, EventArgs ____)
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
            this.DataContext = this;

            this.PathTemplate = style.PathTemplate;
            this.BattleAtlasPathTemplate = style.BattleAtlasPathTemplate;
            this.VehicleMarkersAtlasPathTemplate = style.VehicleMarkersAtlasPathTemplate;
            this.IconsBulkSaveEnabled = style.IconsBulkSaveEnabled;
            this.BattleAtlasBulkSaveEnabled = style.BattleAtlasBulkSaveEnabled;
            this.VehicleMarkersAtlasBulkSaveEnabled = style.VehicleMarkersAtlasBulkSaveEnabled;
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
            var value = PathTemplateWindow.Show(this, this.PathTemplate, context, style);
            if (value == null)
                return;
            this.PathTemplate = value;
            this.IconsBulkSaveEnabled = true;
        }

        private void ctEditBattleAtlasPathTemplate_Click(object _, RoutedEventArgs __)
        {
            var value = PathTemplateWindow.Show(this, this.BattleAtlasPathTemplate, context, style);
            if (value == null)
                return;
            this.BattleAtlasPathTemplate = value;
            this.BattleAtlasBulkSaveEnabled = true;
        }

        private void ctEditVehicleMarkersAtlasPathTemplate_Click(object _, RoutedEventArgs __)
        {
            var value = PathTemplateWindow.Show(this, this.VehicleMarkersAtlasPathTemplate, context, style);
            if (value == null)
                return;
            this.VehicleMarkersAtlasPathTemplate = value;
            this.VehicleMarkersAtlasBulkSaveEnabled = true;
        }
    }
}
