using System.Windows;
using RT.Util.Forms;

namespace TankIconMaker
{
    partial class RenameWindow : ManagedWindow
    {
        public RenameWindow()
            : base(Program.Settings.RenameWindow)
        {
            InitializeComponent();
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public static string ShowRename(Window owner, string name)
        {
            var wnd = new RenameWindow { Owner = owner };
            wnd.ctName.Text = name;
            wnd.ctName.Focus();

            if (wnd.ShowDialog() != true)
                return null;

            return wnd.ctName.Text;
        }
    }
}
