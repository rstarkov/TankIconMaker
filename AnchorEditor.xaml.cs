using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using WpfCrutches;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace TankIconMaker
{
    public partial class AnchorEditor : UserControl, ITypeEditor
    {
        public AnchorEditor()
        {
            InitializeComponent();
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            bind(propertyItem, btnTopLeft, Anchor.TopLeft);
            bind(propertyItem, btnTopCenter, Anchor.TopCenter);
            bind(propertyItem, btnTopRight, Anchor.TopRight);
            bind(propertyItem, btnMidLeft, Anchor.MidLeft);
            bind(propertyItem, btnMidCenter, Anchor.MidCenter);
            bind(propertyItem, btnMidRight, Anchor.MidRight);
            bind(propertyItem, btnBottomLeft, Anchor.BottomLeft);
            bind(propertyItem, btnBottomCenter, Anchor.BottomCenter);
            bind(propertyItem, btnBottomRight, Anchor.BottomRight);
            return this;
        }

        private void bind(PropertyItem propertyItem, RadioButton btnTopLeft, Anchor anchor)
        {
            BindingOperations.SetBinding(btnTopLeft, ToggleButton.IsCheckedProperty, LambdaBinding.New(
                new Binding("Value") { Source = propertyItem, Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay },
                (object source) => { return (Anchor) source == anchor; },
                (bool source) => { return source ? anchor : Binding.DoNothing; }
            ));
        }

    }
}
