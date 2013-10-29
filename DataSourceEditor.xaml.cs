using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WotDataLib;
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
                (ExtraPropertyId source) => { return App.DataSources.FirstOrDefault(d => d.PropertyId.Equals(source)); },
                (DataSourceInfo source) => { return source == null ? null : source.PropertyId; }
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
        public string Name { get { return PropertyId.FileId + (PropertyId.ColumnId == null ? "" : ("/" + PropertyId.ColumnId)); } }
        /// <summary>Name of the data file's author.</summary>
        public string Author { get { return PropertyId.Author; } }

        /// <summary>A short description of the property.</summary>
        public string Description { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected DataSourceInfo() { }

        private ExtraPropertyId _propertyId;

        public DataSourceInfo(ExtraPropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException();
            _propertyId = propertyInfo.PropertyId;
            Description = propertyInfo.Descriptions["Ru"];
        }

        /// <summary>
        /// Updates those properties that are allowed to change without treating the data source as a different source.
        /// There can be several versions of the same data source which may differ in the property description etc. This
        /// method is called to ensure such values are inherited from the latest version of the file.
        /// </summary>
        public void UpdateFrom(ExtraPropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException();
            if (Description != Ut.StringForCurrentLanguage(propertyInfo.Descriptions))
            {
                Description = Ut.StringForCurrentLanguage(propertyInfo.Descriptions);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Description"));
            }
        }

        /// <summary>Returns an "extra" property identifier matching this data file.</summary>
        public virtual ExtraPropertyId PropertyId
        {
            get { return _propertyId; }
        }
    }

    /// <summary>Represents a "tier" data source value using arabic numerals.</summary>
    sealed class DataSourceTierArabic : DataSourceInfo
    {
        public override ExtraPropertyId PropertyId
        {
            get { return ExtraPropertyId.TierArabic; }
        }
    }

    /// <summary>Represents a "tier" data source value using roman numerals.</summary>
    sealed class DataSourceTierRoman : DataSourceInfo
    {
        public override ExtraPropertyId PropertyId
        {
            get { return ExtraPropertyId.TierRoman; }
        }
    }
}
