using System;
using System.Text;
using System.Threading;
using System.Windows;
using RT.Util;
using RT.Util.Dialogs;

namespace TankIconMaker
{
    partial class App : Application
    {
        public App() { Program.App = this; }

        protected override void OnStartup(StartupEventArgs e)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

#if !DEBUG
            Thread.CurrentThread.Name = "Main";
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var errorInfo = new StringBuilder("Thread: " + Thread.CurrentThread.Name);
                var excp = args.ExceptionObject;
                while (excp != null)
                {
                    errorInfo.AppendFormat("\n\nException: {0}", excp.GetType());
                    var exception = excp as Exception;
                    if (exception != null)
                    {
                        errorInfo.AppendFormat("\nMessage: {0}\n", exception.Message);
                        errorInfo.AppendLine(collapseStackTrace(exception.StackTrace));
                        excp = exception.InnerException;
                    }
                }
                var copy = DlgMessage.ShowError("An error has occurred. This is not your fault; the programmer has messed up!\n\nPlease send an error report to the programmer so that this can be fixed.",
                    "Copy report to &clipboard", "Close") == 0;
                if (copy)
                {
                    try
                    {
                        Clipboard.SetText(errorInfo.ToString(), TextDataFormat.UnicodeText);
                        DlgMessage.ShowInfo("Information about the error is now in your clipboard.");
                    }
                    catch { DlgMessage.ShowInfo("Sorry, couldn't even copy the error info to clipboard. Something is broken pretty badly."); }
                }
            };
#endif

            base.OnStartup(e);
            SettingsUtil.LoadSettings(out Program.Settings);
        }

        private string collapseStackTrace(string stackTrace)
        {
            var lines = stackTrace.Split('\n');
            var result = new StringBuilder();
            bool needEllipsis = false;
            foreach (var line in lines)
            {
                if (line.Contains(GetType().Namespace))
                {
                    result.AppendLine("  " + line.Trim());
                    needEllipsis = true;
                }
                else if (needEllipsis)
                {
                    result.AppendLine("  ...");
                    needEllipsis = false;
                }
            }
            return result.ToString();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Program.Settings.SaveQuiet();
            base.OnExit(e);
        }
    }

    static class Program
    {
        public static App App;
        public static Settings Settings;
        public static ObservableSortedList<DataSourceInfo> DataSources = new ObservableSortedList<DataSourceInfo>(
            items: new[] { new DataSourceNone() },
            comparer: CustomComparer<DataSourceInfo>.By(ds => ds is DataSourceNone ? 0 : 1)
                .ThenBy(ds => ds.Name).ThenBy(ds => ds.Language).ThenBy(ds => ds.Author).ThenBy(ds => ds.GameVersion));
    }
}
