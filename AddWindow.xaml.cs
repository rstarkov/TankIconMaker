using System.Windows;
using System.Windows.Controls;
using RT.Util.Forms;

namespace TankIconMaker
{
    partial class AddWindow : ManagedWindow
    {
        public AddWindow()
            : base(Program.Settings.AddWindow)
        {
            InitializeComponent();
            ContentRendered += delegate
            {
                ((ListBoxItem) ctList.ItemContainerGenerator.ContainerFromIndex(0)).Focus();
            };
        }

        private void add(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public static LayerBase ShowAddLayer(Window owner)
        {
            var wnd = new AddWindow { Owner = owner };
            wnd.Title = Program.Translation.AddWindow.AddLayerTitle;
            wnd.lblName.Content = new AccessText { Text = Program.Translation.AddWindow.LayerName };
            wnd.lblList.Content = new AccessText { Text = Program.Translation.AddWindow.LayerType };
            wnd.ctAddLabel.Text = Program.Translation.AddWindow.BtnAdd;
            wnd.ctCancelLabel.Text = Program.Translation.AddWindow.BtnCancel;
            wnd.ctList.ItemsSource = Program.LayerTypes;
            wnd.ctList.SelectedIndex = 0;

            if (wnd.ShowDialog() != true)
                return null;

            var item = wnd.ctList.SelectedItem as TypeInfo<LayerBase>;
            if (item == null)
                return null;
            var result = item.Constructor();
            result.Name = wnd.ctName.Text;
            return result;
        }

        public static EffectBase ShowAddEffect(Window owner)
        {
            var wnd = new AddWindow { Owner = owner };
            wnd.Title = Program.Translation.AddWindow.AddEffectTitle;
            wnd.lblName.Content = new AccessText { Text = Program.Translation.AddWindow.EffectName };
            wnd.lblList.Content = new AccessText { Text = Program.Translation.AddWindow.EffectType };
            wnd.ctAddLabel.Text = Program.Translation.AddWindow.BtnAdd;
            wnd.ctCancelLabel.Text = Program.Translation.AddWindow.BtnCancel;
            wnd.ctList.ItemsSource = Program.EffectTypes;
            wnd.ctList.SelectedIndex = 0;

            if (wnd.ShowDialog() != true)
                return null;

            var item = wnd.ctList.SelectedItem as TypeInfo<EffectBase>;
            if (item == null)
                return null;
            var result = item.Constructor();
            result.Name = wnd.ctName.Text;
            return result;
        }
    }
}
