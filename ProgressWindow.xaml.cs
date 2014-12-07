using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace TankIconMaker
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public int Stage
        {
            set
            {
                progressBar.Value = value;
                UpdateLayout();
                InvalidateVisual();
            }
            get { return (int)(progressBar.Value + 0.1); }
        }

        public string Description
        {
            set {
                label.Content = value;
                UpdateLayout();
                InvalidateVisual();
            }
            get { return (string)(label.Content); }
        }

        public static void ForceUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate(object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);
            Dispatcher.PushFrame(frame);
        }

        public ProgressWindow(int Maximum, string Description)
        {
            InitializeComponent();
            progressBar.Maximum = (double)Maximum;
            progressBar.Value = 0;
            label.Content = Description;
        }
    }
}
