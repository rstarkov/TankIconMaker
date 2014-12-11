using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using RT.Util.Forms;
using WpfCrutches;

namespace TankIconMaker
{
    partial class CheckListWindow : ManagedWindow
    {
        private ObservableCollection<CheckListItem> _checkItems;
        private ObservableValue<bool?> _checkAll = new ObservableValue<bool?>(null);

        private CheckListWindow(string title, IEnumerable<CheckListItem> items)
            : base(App.Settings.CheckListWindow)
        {
            InitializeComponent();
            MainWindow.ApplyUiZoom(this);

            Title = title;
            ctOkBtn.Text = App.Translation.Prompt.PromptWindowOK;
            ctCancelBtn.Text = App.Translation.Prompt.Cancel;
            CheckGrid.Columns[0].Header = App.Translation.CheckList.Name;

            _checkItems = new ObservableCollection<CheckListItem>(items);
            CheckGrid.ItemsSource = _checkItems;

            BindingOperations.SetBinding(chkSelectAll, CheckBox.IsCheckedProperty, LambdaBinding.New(
                new Binding { Source = _checkAll, Path = new PropertyPath("Value") },
                (bool? checkAll) => checkAll,
                (bool? checkAll) => { setAllCheckboxes(checkAll); return checkAll; }
            ));
        }

        private void ok(object sender, RoutedEventArgs e)
        {
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

        public static IEnumerable<TItem> ShowCheckList<TItem>(Window owner, string title, IEnumerable<CheckListItem<TItem>> items)
        {
            var wnd = new CheckListWindow(title, items) { Owner = owner };

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
