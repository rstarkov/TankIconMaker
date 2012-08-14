using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using RT.Util;
using RT.Util.Dialogs;
using RT.Util.Lingo;
using RT.Util.Xml;
using WpfCrutches;
using D = System.Drawing;
using W = System.Windows.Media;

namespace TankIconMaker
{
    partial class App : Application
    {
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
                        errorInfo.AppendLine(Ut.CollapseStackTrace(exception.StackTrace));
                        excp = exception.InnerException;
                    }
                }
                bool copy = DlgMessage.ShowError("An error has occurred. This is not your fault; the programmer has messed up!\n\nPlease send an error report to the programmer so that this can be fixed.",
                    "Copy report to &clipboard", "Close") == 0;
                if (copy)
                    try
                    {
                        Clipboard.SetText(errorInfo.ToString(), TextDataFormat.UnicodeText);
                        DlgMessage.ShowInfo("Information about the error is now in your clipboard.");
                    }
                    catch { DlgMessage.ShowInfo("Sorry, couldn't even copy the error info to clipboard. Something is broken pretty badly."); }
            };
#endif

            // Configure XmlClassify
            XmlClassify.DefaultOptions = new XmlClassifyOptions()
                .AddTypeOptions(typeof(W.Color), new colorTypeOptions())
                .AddTypeOptions(typeof(D.Color), new colorTypeOptions())
                .AddTypeOptions(typeof(Version), new versionTypeOptions())
                .AddTypeOptions(typeof(Filename), new filenameTypeOptions())
                .AddTypeOptions(typeof(ObservableCollection<LayerBase>), new listLayerBaseOptions())
                .AddTypeOptions(typeof(ObservableCollection<EffectBase>), new listEffectBaseOptions());

            // Find all the layer and effect types in the assembly (required before settings are loaded)
            Program.LayerTypes = findTypes<LayerBase>("layer");
            Program.EffectTypes = findTypes<EffectBase>("effect");

            base.OnStartup(e);

            // Load all settings and the UI translation
            SettingsUtil.LoadSettings(out Program.Settings);
            Program.Translation = Lingo.LoadTranslationOrDefault<Translation>("TankIconMaker", ref Program.Settings.Lingo);
        }

        private static IList<TypeInfo<T>> findTypes<T>(string name) where T : IHasTypeNameDescription
        {
            var infos = new List<TypeInfo<T>>();
            foreach (var type in Assembly.GetEntryAssembly().GetTypes().Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract))
            {
                var constructor = type.GetConstructor(new Type[0]);
                if (constructor == null)
                {
                    // (the error message will only be seen by maker developers, so it's ok that it's shown before any UI appears)
                    DlgMessage.ShowWarning("Ignored {1} type \"{0}\" because it does not have a public parameterless constructor.".Fmt(type, name));
                }
                else
                {
                    infos.Add(new TypeInfo<T>
                    {
                        Type = type,
                        Constructor = () => (T) constructor.Invoke(new object[0]),
                        Name = type.Name,
                        Description = type.FullName,
                    });
                }
            }
            infos.Sort(CustomComparer<TypeInfo<T>>.By(ti => ti.Name));
            return infos.AsReadOnly();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Program.Settings.SaveQuiet();
            base.OnExit(e);
        }
    }

    /// <summary>
    /// A few program-wide globals for those rare cases of truly program-global data items.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Various program settings. To ensure that an application crash or a power loss does not result in lost settings,
        /// one of the Save methods should be invoked every time changes are made; this is not automatic.
        /// </summary>
        public static Settings Settings;

        /// <summary>Contains the current UI translation.</summary>
        public static Translation Translation;

        /// <summary>Encapsulates all the tank/game data TankIconMaker requires.</summary>
        public static WotData Data = new WotData();

        /// <summary>Updated by <see cref="MainWindow"/> so that components not coupled to the window can access the current install settings.</summary>
        public static GameInstallationSettings LastGameInstallSettings;

        /// <summary>
        /// Lists all the possible sources of extra properties, sorted and in an observable fashion. This is kept up-to-date
        /// by the <see cref="MainWindow"/> and used in data binding by the <see cref="DataSourceEditor"/>.
        /// </summary>
        public static ObservableSortedList<DataSourceInfo> DataSources = new ObservableSortedList<DataSourceInfo>(
            items: new DataSourceInfo[] { new DataSourceTierArabic(), new DataSourceTierRoman() },
            comparer: CustomComparer<DataSourceInfo>.By(ds => ds is DataSourceTierArabic ? 0 : ds is DataSourceTierRoman ? 1 : 2)
                .ThenBy(ds => ds.Name).ThenBy(ds => ds.Language).ThenBy(ds => ds.Author).ThenBy(ds => ds.GameVersion));

        /// <summary>
        /// Screen resolution at program start time, relative to the WPF's 96.0 ppi. Windows 7 won't allow this to be changed
        /// without a log-off, so it's OK to read this once at start up and assume it doesn't change.
        /// </summary>
        public static double DpiScaleX, DpiScaleY;

        /// <summary>A list of info classes for each layer type defined in this assembly. Initialised once at startup.</summary>
        public static IList<TypeInfo<LayerBase>> LayerTypes;
        /// <summary>A list of info classes for each effect type defined in this assembly. Initialised once at startup.</summary>
        public static IList<TypeInfo<EffectBase>> EffectTypes;
    }
}
