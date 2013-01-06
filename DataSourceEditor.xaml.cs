using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WpfCrutches;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace TankIconMaker
{
    /// <summary>
    /// Implements a drop-down editor for selecting one of the extra property files in the maker property editor.
    /// Apply to your maker's property as follows: <c>[Editor(typeof(DataSourceEditor), typeof(DataSourceEditor))]</c>.
    /// </summary>
    public partial class DataSourceEditor : UserControl, ITypeEditor
    {
        public DataSourceEditor()
        {
            InitializeComponent();
            ctCombo.ItemsSource = App.DataSources;
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            BindingOperations.SetBinding(ctCombo, ComboBox.SelectedItemProperty, LambdaBinding.New(
                new Binding("Value") { Source = propertyItem, Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay },
                (ExtraPropertyId source) => { return App.DataSources.FirstOrDefault(d => d.ToExtraPropertyId().Equals(source)); },
                (DataSourceInfo source) => { return source == null ? null : source.ToExtraPropertyId(); }
            ));
            return this;
        }
    }

    /// <summary>
    /// Selects one of the two data templates: one for "no data source", and the other for all the real data sources.
    /// </summary>
    class DataSourceTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;

            if (item is DataSourceTierArabic)
                return element.FindResource("tierArabicTemplate") as DataTemplate;
            else if (item is DataSourceTierRoman)
                return element.FindResource("tierRomanTemplate") as DataTemplate;
            else
                return element.FindResource("sourceTemplate") as DataTemplate;
        }
    }

    /// <summary>
    /// Holds information about a data source (that is, an "extra" property data file).
    /// </summary>
    class DataSourceInfo : INotifyPropertyChanged
    {
        /// <summary>The name of the property.</summary>
        public string Name { get; private set; }
        /// <summary>The 2-letter language code of the property; "xx" for language-less properties.</summary>
        public string Language { get; private set; }
        /// <summary>Name of the data file's author.</summary>
        public string Author { get; private set; }

        /// <summary>A short description of the property.</summary>
        public string Description { get; private set; }
        /// <summary>Which game version was this data file made for.</summary>
        public int GameVersion { get; private set; }
        /// <summary>The last data file version used to construct this source.</summary>
        public int FileVersion { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected DataSourceInfo() { }

        public DataSourceInfo(DataFileExtra file)
        {
            Name = file.Name;
            Language = file.Language;
            Author = file.Author;

            Description = file.Description;
            GameVersion = file.GameVersion;
            FileVersion = file.FileVersion;
        }

        /// <summary>
        /// Updates those properties that are allowed to change without treating the data source as a different source.
        /// There can be several versions of the same data source which may differ in the property description etc. This
        /// method is called to ensure such values are inherited from the latest version of the file.
        /// </summary>
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

        /// <summary>Returns an "extra" property identifier matching this data file.</summary>
        public virtual ExtraPropertyId ToExtraPropertyId()
        {
            return new ExtraPropertyId(Name, Language, Author);
        }
    }

    /// <summary>Represents a "tier" data source value using arabic numerals.</summary>
    sealed class DataSourceTierArabic : DataSourceInfo
    {
        public override ExtraPropertyId ToExtraPropertyId()
        {
            return ExtraPropertyId.TierArabic;
        }
    }

    /// <summary>Represents a "tier" data source value using roman numerals.</summary>
    sealed class DataSourceTierRoman : DataSourceInfo
    {
        public override ExtraPropertyId ToExtraPropertyId()
        {
            return ExtraPropertyId.TierRoman;
        }
    }
}
