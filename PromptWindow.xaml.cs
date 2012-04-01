using System.Windows;
using RT.Util.Forms;

namespace TankIconMaker
{
    partial class PromptWindow : ManagedWindow
    {
        public PromptWindow()
            : base(Program.Settings.RenameWindow)
        {
            InitializeComponent();
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public static string ShowPrompt(Window owner, string name, string title, string label)
        {
            var wnd = new PromptWindow { Owner = owner };
            wnd.Title = title;
            wnd.lblName.Content = label;
            wnd.ctName.Text = name;
            wnd.ctName.Focus();

            if (wnd.ShowDialog() != true)
                return null;

            return wnd.ctName.Text;
        }
    }
}
