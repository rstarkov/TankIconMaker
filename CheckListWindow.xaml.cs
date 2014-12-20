using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using RT.Util.Dialogs;
using RT.Util.Forms;
using WpfCrutches;

namespace TankIconMaker
{
    partial class CheckListWindow : ManagedWindow
    {
        private ObservableCollection<CheckListItem> _checkItems;
        private ObservableValue<bool?> _checkAll = new ObservableValue<bool?>(null);
        private string _promptSure;
        private string _promptSureYes;

        private CheckListWindow()
            : base(App.Settings.CheckListWindow)
        {
            InitializeComponent();
            MainWindow.ApplyUiZoom(this);
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            if (_promptSure != null && _checkItems.Count(item => item.IsChecked) > 0)
                if (DlgMessage.ShowWarning(_promptSure, _promptSureYes, App.Translation.Prompt.Cancel) != 0)
                    return;
            DialogResult = true;
        }

        private void setAllCheckboxes(bool? check)
        {
            if (check == null)
                return;
            foreach (var item in _checkItems)
                item.IsChecked = check.Value;
        }

        private void checkedChanged(object sender, RoutedEventArgs e)
        {
            int checkedCount = _checkItems.Count(item => item.IsChecked);
            _checkAll.Value = checkedCount == 0 ? false : checkedCount == _checkItems.Count ? true : (bool?) null;
        }

        public static IEnumerable<TItem> ShowCheckList<TItem>(Window owner, IEnumerable<CheckListItem<TItem>> items, string prompt, string okButton, string[] columnTitles, string promptSure = null)
        {
            var wnd = new CheckListWindow() { Owner = owner };
            wnd.ctPrompt.Text = prompt;
            wnd.ctOkBtn.Text = okButton;
            wnd.ctCancelBtn.Text = App.Translation.Prompt.Cancel;
            for (int i = 0; i < columnTitles.Length; i++)
            {
                wnd.ctGrid.Columns[i + 1].Header = columnTitles[i];
                wnd.ctGrid.Columns[i + 1].Visibility = Visibility.Visible;
                if (i != columnTitles.Length - 1) // mark all columns except for the last one as auto width
                    wnd.ctGrid.Columns[i + 1].Width = DataGridLength.Auto;
            }
            wnd._promptSure = promptSure;
            wnd._promptSureYes = okButton.Replace('_', '&');

            wnd._checkItems = new ObservableCollection<CheckListItem>(items);
            wnd.ctGrid.ItemsSource = wnd._checkItems;

            BindingOperations.SetBinding(wnd.chkSelectAll, CheckBox.IsCheckedProperty, LambdaBinding.New(
                new Binding { Source = wnd._checkAll, Path = new PropertyPath("Value") },
                (bool? checkAll) => checkAll,
                (bool? checkAll) => { wnd.setAllCheckboxes(checkAll); return checkAll; }
            ));

            if (wnd.ShowDialog() != true)
                return Enumerable.Empty<TItem>();

            return wnd._checkItems.OfType<CheckListItem<TItem>>().Where(cli => cli.IsChecked).Select(cli => cli.Item);
        }
    }

    public class CheckListItem : INotifyPropertyChanged
    {
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public bool IsChecked { get { return _isChecked; } set { _isChecked = value; PropertyChanged(this, new PropertyChangedEventArgs("IsChecked")); } }
        private bool _isChecked;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }

    public class CheckListItem<TItem> : CheckListItem
    {
        public TItem Item { set; get; }
    }
}
