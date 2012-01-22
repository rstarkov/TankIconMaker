using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Windows.Controls.PropertyGrid;
using Microsoft.Windows.Controls.PropertyGrid.Editors;

namespace TankIconMaker
{
    public partial class DataSourceEditor : UserControl, ITypeEditor
    {
        public DataSourceEditor()
        {
            InitializeComponent();
            ctCombo.ItemsSource = Program.DataSources;
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            Binding binding = new Binding("Value")
            {
                Source = propertyItem,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                Converter = LambdaConverter.New(
                    (string source) =>
                    {
                        return Program.DataSources.FirstOrDefault(d => d.ToString() == source);
                    },
                    (DataSourceInfo source) =>
                    {
                        return source.ToString();
                    }
                )
            };
            BindingOperations.SetBinding(ctCombo, ComboBox.SelectedItemProperty, binding);
            return this;
        }
    }

    sealed class DataSourceInfo : INotifyPropertyChanged
    {
        public string Name { get; private set; }
        public string Language { get; private set; }
        public string Author { get; private set; }

        public string Description { get; private set; }
        public Version GameVersion { get; private set; }
        public int FileVersion { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public DataSourceInfo(DataFileExtra file)
        {
            Name = file.Name;
            Language = file.Language;
            Author = file.Author;

            Description = file.Description;
            GameVersion = file.GameVersion;
            FileVersion = file.FileVersion;
        }

        public void UpdateFrom(DataFileExtra file)
        {
            if (Description != file.Description)
            {
                Description = file.Description;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Description"));
            }
            if (GameVersion != file.GameVersion)
            {
                GameVersion = file.GameVersion;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("GameVersion"));
            }
            if (FileVersion != file.FileVersion)
            {
                FileVersion = file.FileVersion;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("FileVersion"));
            }
        }

        public override string ToString()
        {
            return Name + "/" + Language + "/" + Author;
        }
    }
}
