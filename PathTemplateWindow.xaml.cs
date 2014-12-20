using System.Windows;
using System.Windows.Controls;
using RT.Util.Forms;
using RT.Util.Lingo;
using WotDataLib;

namespace TankIconMaker
{
    partial class PathTemplateWindow : ManagedWindow
    {
        private WotContext _context;
        private Style _style;

        internal PathTemplateWindow()
            : base(App.Settings.PathTemplateWindow)
        {
            InitializeComponent();
        }

        public PathTemplateWindow(string value, WotContext context, Style style)
            : this()
        {
            MainWindow.ApplyUiZoom(this);

            Title = App.Translation.PathTemplateWindow.Title;
            Lingo.TranslateWindow(this, App.Translation.PathTemplateWindow);

            _context = context;
            _style = style;

            ctValue.Text = value;
            ctValue.Focus();
            ctValue_TextChanged(null, null);

            ctModsPathHelp.Text = ctModsPathHelp.Text.Replace("{cur}", Ut.ExpandIconPath("{ModsPath}", _context, _style, Country.USSR, Class.Heavy));
            ctTimPathHelp.Text = ctTimPathHelp.Text.Replace("{cur}", Ut.ExpandIconPath("{TimPath}", _context, _style, Country.USSR, Class.Heavy));
            ctGamePathHelp.Text = ctGamePathHelp.Text.Replace("{cur}", Ut.ExpandIconPath("{GamePath}", _context, _style, Country.USSR, Class.Heavy));
            ctGameVersionHelp.Text = ctGameVersionHelp.Text.Replace("{cur}", Ut.ExpandIconPath("{GameVersion}", _context, _style, Country.USSR, Class.Heavy));
            ctStyleNameHelp.Text = ctStyleNameHelp.Text.Replace("{cur}", Ut.ExpandIconPath("{StyleName}", _context, _style, Country.USSR, Class.Heavy));
            ctStyleAuthorHelp.Text = ctStyleAuthorHelp.Text.Replace("{cur}", Ut.ExpandIconPath("{StyleAuthor}", _context, _style, Country.USSR, Class.Heavy));
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public static string Show(Window owner, string value, WotContext context, Style style)
        {
            var wnd = new PathTemplateWindow(value, context, style) { Owner = owner };

            if (wnd.ShowDialog() != true)
                return null;

            return wnd.ctValue.Text;
        }

        private void ctValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            ctExpandsTo.Text = Ut.ExpandIconPath(ctValue.Text, _context, _style, Country.USSR, Class.Heavy);
        }
    }
}
