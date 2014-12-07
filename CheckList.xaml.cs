using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace TankIconMaker
{
    /// <summary>
    /// Interaction logic for CheckList.xaml
    /// </summary>
    public partial class CheckList : Window
    {
        int checkAllState = 0;
        bool skipEvent = false;
        ObservableCollection<CheckData> checkList;

        public CheckList()
        {
            InitializeComponent();
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        
        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (skipEvent)
                return;
            checkAllState = 1;
            skipEvent = true;
            for (int i = 0; i < CheckGrid.Items.Count; i++)
            {
                checkList[i] = new CheckData { Id = checkList[i].Id, Name = checkList[i].Name, IsActiveBool = true };
            }
            skipEvent = false;
        }

        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (skipEvent)
                return;
            if (checkAllState == -1)
            {
                CheckBox chkAll = CheckGrid.Columns[1].Header as CheckBox;
                chkAll.IsChecked = true;
                checkAllState = 1;
                return;
            }
            checkAllState = 0;
            skipEvent = true;
            for (int i = 0; i < CheckGrid.Items.Count; i++)
            {
                checkList[i] = new CheckData { Id = checkList[i].Id, Name = checkList[i].Name, IsActiveBool = false };
            }
            skipEvent = false;
        }

        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            if (skipEvent)
                return;
            skipEvent = true;
            CheckBox chkAll = CheckGrid.Columns[1].Header as CheckBox;
            updateCheckListHeader();
            skipEvent = false;
        }

        private void chk_Unchecked(object sender, RoutedEventArgs e)
        {
            if (skipEvent)
                return;
            skipEvent = true;
            updateCheckListHeader();
            skipEvent = false;
        }

        private void updateCheckListHeader()
        {
            CheckBox chkAll = CheckGrid.Columns[1].Header as CheckBox;
            if (chkAll == null)
            {
                return;
            }
            int trues = 0;
            for (int i = 0; i < CheckGrid.Items.Count; i++)
            {
                if (checkList[i].IsActiveBool)
                {
                    ++trues;
                }
            }
            if (trues == 0)
            {
                chkAll.IsChecked = false;
                checkAllState = 0;
            }
            else if (trues < CheckGrid.Items.Count)
            {
                chkAll.IsChecked = null;
                checkAllState = -1;
            }
            else
            {
                chkAll.IsChecked = true;
                checkAllState = 1;
            }
        }

        public static List<string> ShowCheckList(Window owner, string title, List<CheckData> values)
        {
            var wnd = new CheckList { Owner = owner };
            wnd.Title = title;
            wnd.ctOkBtn.Text = App.Translation.Prompt.PromptWindowOK;
            wnd.ctCancelBtn.Text = App.Translation.Prompt.Cancel;
            
            wnd.checkList = new ObservableCollection<CheckData>(values);
            wnd.updateCheckListHeader();
            wnd.CheckGrid.Columns[0].Header = App.Translation.CheckList.Name;
            wnd.CheckGrid.ItemsSource = wnd.checkList;

            List<string> checkedIds = new List<string>();
            if (wnd.ShowDialog() != true)
                return checkedIds;

            foreach (CheckData chkd in wnd.checkList)
            {
                if (chkd.IsActiveBool)
                {
                    checkedIds.Add(chkd.Id);
                }
            }
            return checkedIds;
        }
    }

    public class CheckData
    {
        public string Id { set; get; }
        public string Name { set; get; }
        public bool IsActiveBool { set; get; }
    }
}
