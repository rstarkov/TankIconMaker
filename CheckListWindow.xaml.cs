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

        private CheckListWindow(IEnumerable<CheckListItem> items, string prompt, string okButton, string columnTitle, string promptSure)
            : base(App.Settings.CheckListWindow)
        {
            InitializeComponent();
            MainWindow.ApplyUiZoom(this);

            ctPrompt.Text = prompt;
            ctOkBtn.Text = okButton;
            ctCancelBtn.Text = App.Translation.Prompt.Cancel;
            ctGrid.Columns[1].Header = columnTitle;
            _promptSure = promptSure;
            _promptSureYes = okButton.Replace('_', '&');

            _checkItems = new ObservableCollection<CheckListItem>(items);
            ctGrid.ItemsSource = _checkItems;

            BindingOperations.SetBinding(chkSelectAll, CheckBox.IsCheckedProperty, LambdaBinding.New(
                new Binding { Source = _checkAll, Path = new PropertyPath("Value") },
                (bool? checkAll) => checkAll,
                (bool? checkAll) => { setAllCheckboxes(checkAll); return checkAll; }
            ));
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

        public static IEnumerable<TItem> ShowCheckList<TItem>(Window owner, IEnumerable<CheckListItem<TItem>> items, string prompt, string okButton, string columnTitle, string promptSure = null)
        {
            var wnd = new CheckListWindow(items, prompt, okButton, columnTitle, promptSure) { Owner = owner };

            if (wnd.ShowDialog() != true)
                return Enumerable.Empty<TItem>();

            return wnd._checkItems.OfType<CheckListItem<TItem>>().Where(cli => cli.IsChecked).Select(cli => cli.Item);
        }
    }

    public class CheckListItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public bool IsChecked { get { return _isChecked; } set { _isChecked = value; PropertyChanged(this, new PropertyChangedEventArgs("IsChecked")); } }
        private bool _isChecked;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }

    public class CheckListItem<TItem> : CheckListItem
    {
        public TItem Item { set; get; }
    }
}
