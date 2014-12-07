using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TankIconMaker
{
    class ProgressDialog
    {
        private int maximum = 100;
        private int stage;
        private string description;
        private CancellationTokenSource closeToken = null;
        private static AutoResetEvent updateEvent = new AutoResetEvent(false);

        public int Stage
        {
            set {
                stage = value;
                updateEvent.Set();
            }
            get { return stage; }
        }

        public string Description
        {
            set
            {
                description = value;
                updateEvent.Set();
            }
            get { return description; }
        }

        public void Next(string Description)
        {
            ++stage;
            description = Description + string.Format(" ({0}/{1})", stage, maximum);
            updateEvent.Set();
        }

        public void Close()
        {
            if (closeToken != null)
            {
                closeToken.Cancel();
                updateEvent.Set();
                closeToken = null;
            }
        }

        public void Show(int Maximum, string InitDescription = "Process started...")
        {
            stage = 0;
            description = InitDescription + string.Format(" ({0}/{1})", stage, Maximum);
            maximum = Maximum;
            closeToken = new CancellationTokenSource();
            CancellationToken cancelToken = closeToken.Token;
            Thread newThread = new Thread(()=>{
                try
                {
                    ProgressWindow window = new ProgressWindow(maximum, description);
                    window.Show();
                    window.UpdateLayout();
                    while (true)
                    {
                        updateEvent.WaitOne();
                        if (cancelToken.IsCancellationRequested)
                        {
                            window.Close();
                            return;
                        }
                        window.Stage = stage;
                        window.Description = description;
                        ProgressWindow.ForceUIToUpdate();
                    }
                }
                catch
                {
                }
            });
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.Start();
            //Task.Factory.StartNew(dialog, cancelToken, TaskCreationOptions.None, PriorityScheduler.Lowest);
        }

        public ProgressDialog()
        {
        }
    }
}
