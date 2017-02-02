using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RT.Util.Dialogs;
using RT.Util.Forms;
using RT.Util.Lingo;
using WotDataLib;

namespace TankIconMaker
{
    partial class PathTemplateWindow : ManagedWindow
    {
        private WotContext _context;
        private Style _style;
        private SaveType _saveType;
        private WotTank _exampleTank;

        internal PathTemplateWindow()
            : base(App.Settings.PathTemplateWindow)
        {
            InitializeComponent();
        }

        public PathTemplateWindow(string value, WotContext context, Style style, SaveType saveType)
            : this()
        {
            MainWindow.ApplyUiZoom(this);

            Title = App.Translation.PathTemplateWindow.Title;
            Lingo.TranslateWindow(this, App.Translation.PathTemplateWindow);

            _context = context;
            _style = style;
            _saveType = saveType;

            ctValue.Text = value;
            ctValue.Focus();
            ctValue_TextChanged(null, null);
            var isAtlas = saveType != SaveType.Icons;
            if (isAtlas)
            {
                ctIconsPathMacro.Text = "{AtlasPath}";
                for (int i = 4; i <= 9; ++i)
                {
                    var row = this.ctHelpGrid.RowDefinitions[i];
                    row.Height = new GridLength(0);
                }
            }
            else
            {
                _exampleTank = context.Tanks.FirstOrDefault(x => x.TankId.Contains("Object_260"));
                if (_exampleTank == null)
                {
                    _exampleTank = context.Tanks.FirstOrDefault();
                }
            }

            ctIconsPathHelp.Text = isAtlas
                ? ctIconsPathHelp.Text.Replace("{cur}",
                    Ut.ExpandIconPath("{AtlasPath}", _context, _style, "", "", fragment: true))
                : ctIconsPathHelp.Text.Replace("{cur}",
                    Ut.ExpandIconPath("{IconsPath}", _context, _style, "", "", fragment: true));
            ctTimPathHelp.Text = ctTimPathHelp.Text.Replace("{cur}",
                Ut.ExpandIconPath("{TimPath}", _context, _style, "", "", fragment: true));
            ctGamePathHelp.Text = ctGamePathHelp.Text.Replace("{cur}",
                Ut.ExpandIconPath("{GamePath}", _context, _style, "", "", fragment: true));
            ctGameVersionHelp.Text = ctGameVersionHelp.Text.Replace("{cur}",
                Ut.ExpandIconPath("{GameVersion}", _context, _style, "", "", fragment: true));
            ctStyleNameHelp.Text = ctStyleNameHelp.Text.Replace("{cur}",
                Ut.ExpandIconPath("{StyleName}", _context, _style, "", "", fragment: true));
            ctStyleAuthorHelp.Text = ctStyleAuthorHelp.Text.Replace("{cur}",
                Ut.ExpandIconPath("{StyleAuthor}", _context, _style, "", "", fragment: true));
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            if (!checkPath())
            {
                return;
            }
            DialogResult = true;
        }

        private bool checkPath()
        {
            var text = this.ctValue.Text;
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            if (this._saveType == SaveType.Icons)
            {
                int res;
                string append;
                if (!text.Contains("{TankId}") && !text.Contains("{TankFullName}") && !text.Contains("{TankShortName}"))
                {
                    res = DlgMessage.ShowQuestion(
                        App.Translation.PathTemplateWindow.WarnIconsPathIsFolder,
                        new string[]
                        {App.Translation.Prompt.Yes, App.Translation.Prompt.No, App.Translation.Prompt.Cancel});
                    append = @"\{TankId}{Ext}";
                }
                else if (!text.EndsWith("{Ext}"))
                {
                    res = DlgMessage.ShowQuestion(
                        App.Translation.PathTemplateWindow.WarnIconsPathNoExt,
                        new string[] { App.Translation.Prompt.Yes, App.Translation.Prompt.No, App.Translation.Prompt.Cancel });
                    append = @"{Ext}";
                }
                else
                {
                    return true;
                }
                switch (res)
                {
                    case 0:
                        this.ctValue.Text += append;
                        return true;
                    case 1:
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                if (!text.EndsWith(".png"))
                {
                    string atlasName = AtlasBuilder.GetAtlasFilename(this._saveType);
                    var res = DlgMessage.ShowQuestion(
                        string.Format(App.Translation.PathTemplateWindow.WarnAtlasPathIsFolder, atlasName),
                        new string[]
                        {App.Translation.Prompt.Yes, App.Translation.Prompt.No, App.Translation.Prompt.Cancel});
                    switch (res)
                    {
                        case 0:
                            this.ctValue.Text += @"\" + atlasName;
                            return true;
                        case 1:
                            return true;
                        default:
                            return false;
                    }
                }
            }
            return true;
        }

        public static string Show(Window owner, string value, WotContext context, Style style, SaveType saveType)
        {
            var wnd = new PathTemplateWindow(value, context, style, saveType) { Owner = owner };

            if (wnd.ShowDialog() != true)
                return null;

            return wnd.ctValue.Text;
        }

        private void ctValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            ctExpandsTo.Text = Ut.ExpandIconPath(ctValue.Text, _context, _style, _exampleTank, saveType: this._saveType);
        }

        private void SelectAll(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
    }
}
