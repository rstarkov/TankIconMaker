using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ookii.Dialogs.Wpf;
using RT.Util.Xml;
using WotDataLib;
using WpfCrutches;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace TankIconMaker
{
    public partial class FilenameEditor : UserControl, ITypeEditor
    {
        private BindingExpression _expression;

        /// <summary>
        ///     A hack to give the filename editor access to the relevant context. A non-hacky approach would require the
        ///     MainWindow to pass to each instance some way of retrieving the current context. However, since these instances
        ///     are created by a third-party component, this is highly non-trivial, so this hack is used instead.</summary>
        public static WotContext LastContext;

        public FilenameEditor()
        {
            InitializeComponent();
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            BindingOperations.SetBinding(textbox, TextBox.TextProperty, LambdaBinding.New(
                new Binding("Value") { Source = propertyItem, Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay },
                (Filename source) => { return (string) source; },
                (string source) => { return (Filename) source; }
            ));
            _expression = textbox.GetBindingExpression(TextBox.TextProperty);
            return this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new VistaOpenFileDialog();
            dlg.Filter = App.Translation.Misc.Filter_FilenameEditor;
            dlg.FilterIndex = 0;
            dlg.Multiselect = false;
            dlg.CheckFileExists = false;
            if (dlg.ShowDialog() != true)
                return;
            textbox.Text = Ut.MakeRelativePath(LastContext, dlg.FileName);
            _expression.UpdateSource();
        }
    }

    struct Filename
    {
        private string _name;
        public static implicit operator Filename(string value) { return new Filename { _name = value }; }
        public static implicit operator string(Filename value) { return value._name; }
        public override string ToString() { return this; }
    }

    class filenameTypeOptions : XmlClassifyTypeOptions, IXmlClassifySubstitute<Filename, string>
    {
        public Filename FromSubstitute(string instance) { return instance; }
        public string ToSubstitute(Filename instance) { return instance; }
    }
}
