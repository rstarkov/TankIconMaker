using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using RT.Util;
using RT.Util.Forms;
using RT.Util.Lingo;
using RT.Util.Xml;
using WpfCrutches;

namespace TankIconMaker
{
    /// <summary>
    /// Stores all the settings for TankIconMaker. All the defaults set in the field initializer or the constructor are preserved
    /// whenever they are missing from the settings XML file. Background saves are supported efficiently via
    /// <see cref="SaveThreaded"/>, which is designed to be callable efficiently multiple times in a row.
    /// </summary>
    [Settings("TankIconMaker2", SettingsKind.UserSpecific)]
    sealed class Settings : SettingsThreadedBase, IXmlClassifyProcess
    {
        /// <summary>Stores the program version that was used to save this settings file.</summary>
        public int SavedByVersion = 17; // this feature was introduced in v018, so default to 17 for "everything before that".

        /// <summary>
        /// MainWindow settings, such as position, size and maximized state. Given that TankIconMaker is essentially a single-window
        /// application, all the other MainWindow-specific settings are stored directly in <see cref="Settings"/> for simplicity.
        /// </summary>
        public ManagedWindow.Settings MainWindow = new ManagedWindow.Settings();
        /// <summary>AddWindow-related settings.</summary>
        public ManagedWindow.Settings AddWindow = new ManagedWindow.Settings();
        /// <summary>RenameWindow-related settings.</summary>
        public ManagedWindow.Settings RenameWindow = new ManagedWindow.Settings();
        /// <summary>Translation window related settings.</summary>
        public TranslationForm<Translation>.Settings TranslationFormSettings = new TranslationForm<Translation>.Settings();
        /// <summary>The width of the sidebar in the main window; null to use the width set at design time.</summary>
        public double? LeftColumnWidth = null;
        /// <summary>The width of the name column in the maker property editor; null to use the width set at design time.</summary>
        public double? NameColumnWidth = null;
        /// <summary>The last used index of the display mode dropdown.</summary>
        public DisplayFilter DisplayFilter = DisplayFilter.All;
        /// <summary>The path last used with the "Save to folder" command.</summary>
        public string SaveToFolderPath = null;

        /// <summary>The name and author of the last used maker.</summary>
        public string SelectedStyleNameAndAuthor;

        /// <summary>A list of all the available user styles. Built-in styles are not stored in the settings file.</summary>
        public ObservableSortedList<Style> Styles = new ObservableSortedList<Style>();

        /// <summary>Specifies the name of the selected background file, or ":checkered" / ":solid" for these special backgrounds.</summary>
        public string Background = "Ruinberg (Руинберг).jpg";
        public Color BackgroundCheckeredColor1 = Color.FromRgb(0xC0, 0xC0, 0xC0);
        public Color BackgroundCheckeredColor2 = Color.FromRgb(0xA0, 0xA0, 0xA0);
        public Color BackgroundSolidColor = Color.FromRgb(0x80, 0xC0, 0xFF);
        public int[] CustomColors = new int[] { 0xC0C0C0, 0xFFC080, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xA0A0A0, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF };

        /// <summary>Program language, and also the default language for property values when not specified.</summary>
        public Language Lingo = Language.EnglishUK;
        /// <summary>The language of the operating system selected last time the program started. When this changes, <see cref="Lingo"/> is autoselected.</summary>
        public Language? OsLingo = null;

        /// <summary>When resolving property inheritance and the author is not specified, this author is given preference.</summary>
        public string DefaultPropertyAuthor = "Wargaming";

        /// <summary>Settings that are specific to a game installation path.</summary>
        public ObservableSortedList<GameInstallationSettings> GameInstallations = new ObservableSortedList<GameInstallationSettings>();

        /// <summary>The authoritative source on which of the game installations the user has activated in the UI. May be null if no game installations are listed.</summary>
        public GameInstallationSettings ActiveInstallation;

        protected override SettingsThreadedBase CloneForSaveThreaded()
        {
            var result = (Settings) MemberwiseClone();
            return result;
        }

        #region Upgrade-related

        void IXmlClassifyProcess.AfterXmlDeclassify()
        {
            // Added in v019
            if (SavedByVersion < 19 && GameInstalls != null)
                GameInstallations = GameInstalls;
            if (SavedByVersion < 19 && SelectedGamePath != null)
                ActiveInstallation = GameInstallations.Where(gi => gi.Path.EqualsNoCase(SelectedGamePath)).FirstOrDefault() ?? GameInstallations.FirstOrDefault();
            GameInstalls = null;
            SelectedGamePath = null;
        }

        void IXmlClassifyProcess.BeforeXmlClassify()
        {
        }

        // Fields obsoleted in v019
        [XmlIgnoreIfDefault]
        private ObservableSortedList<GameInstallationSettings> GameInstalls;
        [XmlIgnoreIfDefault]
        private string SelectedGamePath;

        #endregion
    }

    /// <summary>Specifies which tank icons are to be displayed in the preview area.</summary>
    /// <remarks>The numeric value corresponds to the index of the relevant item in the filter drop-down.</remarks>
    enum DisplayFilter
    {
        All = 0,
        OneOfEach = 1,
        USSR = 3, Germany, USA, France, UK, China,
        Light = 10, Medium, Heavy, Artillery, Destroyer,
        Normal = 16, Premium, Special,
    }

    /// <summary>
    /// Encapsulates the settings for a specific game install location.
    /// </summary>
    sealed class GameInstallationSettings : IComparable<GameInstallationSettings>, INotifyPropertyChanged
    {
        private GameInstallationSettings() { } // for XmlClassify

        public GameInstallationSettings(string path)
        {
            _path = path;
            ReloadGameVersion();
        }

        /// <summary>Absolute path to the root of this game installation.</summary>
        public string Path
        {
            get { return _path; }
        }
        private string _path;

        /// <summary>The version of the game that is located at this path. Null if game version could not be detected or no suitable configuration available.</summary>
        public GameVersion GameVersion
        {
            get { return GameVersionId == null ? null : App.Data.GetVersion(GameVersionId.Value); }
        }

        [XmlIgnore]
        public int? GameVersionId { get; private set; }
        [XmlIgnore]
        public string GameVersionName { get; private set; }

        public void ReloadGameVersion()
        {
            string gameVersionName; // sigh... :( This is so ugly.
            GameVersionId = Ut.ReadGameVersionId(Path, out gameVersionName);
            GameVersionName = gameVersionName;
            PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
        }

        /// <summary>The value displayed in the drop-down.</summary>
        public string DisplayName { get { return (GameVersionName ?? "?") + ":  " + Path; } }
        public override string ToString() { return DisplayName; }

        public int CompareTo(GameInstallationSettings other)
        {
            if (other == null) return 1;
            if (GameVersionId == null && other.GameVersionId == null)
                return 0;
            if (GameVersionId == null || other.GameVersionId == null)
                return other.GameVersionId == null ? -1 : 1;
            int result = -GameVersionId.Value.CompareTo(other.GameVersionId.Value);
            if (result != 0)
                return result;
            return string.Compare(Path, other.Path);
        }

        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };
    }
}
