using System;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;
using RT.Util;
using RT.Util.Forms;
using RT.Util.Lingo;
using RT.Util.Serialization;
using WotDataLib;
using WpfCrutches;

namespace TankIconMaker
{
    /// <summary>
    /// Stores all the settings for TankIconMaker. All the defaults set in the field initializer or the constructor are preserved
    /// whenever they are missing from the settings XML file. Background saves are supported efficiently via
    /// <see cref="SaveThreaded"/>, which is designed to be callable efficiently multiple times in a row.
    /// </summary>
    [Settings("TankIconMaker2", SettingsKind.UserSpecific)]
    sealed class Settings : SettingsThreadedBase, IClassifyXmlObjectProcessor
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
        /// <summary>Settings for all the short prompts shown using the PromptWindow.</summary>
        public ManagedWindow.Settings RenameWindow = new ManagedWindow.Settings();
        /// <summary>Settings for the path template edit prompts.</summary>
        public ManagedWindow.Settings PathTemplateWindow = new ManagedWindow.Settings();
        /// <summary>CheckListWindow-related settings.</summary>
        public ManagedWindow.Settings CheckListWindow = new ManagedWindow.Settings();
        /// <summary>CheckListWindow-related settings.</summary>
        public ManagedWindow.Settings BulkSaveSettingsWindow = new ManagedWindow.Settings();
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
        public string SaveToAtlas = null;
        /// <summary>The path last used with the "Bulk save to folder" command.</summary>
        public string BulkSaveToFolderPath = null;

        /// <summary>Specifies the name of the selected background file, or ":checkered" / ":solid" for these special backgrounds.</summary>
        public string Background = "Ruinberg (Руинберг).jpg";
        public Color BackgroundCheckeredColor1 = Color.FromRgb(0xC0, 0xC0, 0xC0);
        public Color BackgroundCheckeredColor2 = Color.FromRgb(0xA0, 0xA0, 0xA0);
        public Color BackgroundSolidColor = Color.FromRgb(0x80, 0xC0, 0xFF);
        public int[] CustomColors = new int[] { 0xC0C0C0, 0xFFC080, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xA0A0A0, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF, 0xFFFFFF };

        /// <summary>Specifies the scale at which icons are displayed with the "Zoom" checkbox checked or unchecked. The scale is relative to screen pixels, so a scale of 1 shows 1:1 pixels.</summary>
        public double IconScaleNormal = 1;
        public double IconScaleZoomed = 5;

        /// <summary>Specifies a zoom factor for the entire UI, except for the icons.</summary>
        public double UiZoom = 1;

        /// <summary>Program language, and also the default language for property values when not specified.</summary>
        public Language Lingo = Language.EnglishUK;
        /// <summary>The language of the operating system selected last time the program started. When this changes, <see cref="Lingo"/> is autoselected.</summary>
        public Language? OsLingo = null;

        /// <summary>When a property name is specified without the author, and multiple authors are available, this author is given preference.</summary>
        public string DefaultPropertyAuthor = "Wargaming";

        /// <summary>Holds every game installation the user added to the program, as well as any installation-specific settings (currently none).</summary>
        public ObservableSortedList<TimGameInstallation> GameInstallations = new ObservableSortedList<TimGameInstallation>();

        /// <summary>Remembers which game installation the user selected.</summary>
        [Obsolete("Use this only for settings!")] // this field should only be used on startup to load the value and on change to save it. At all other times this should be retrieved either directly from the GUI or from the relevant WotContext.
        public TimGameInstallation ActiveInstallation;

        /// <summary>A list of all the available user styles. Built-in styles are not stored in the settings file.</summary>
        public ObservableSortedList<Style> Styles = new ObservableSortedList<Style>();

        /// <summary>
        /// The authoritative source on which of the game styles the user has activated in the UI. This is only null on first run, until the initialization
        /// has finished. Note that this may also be set to a built-in style, even though those aren't stored in <see cref="Styles"/>; in this case,
        /// the correct built-in style instance is substituted on startup by matching the style name.
        /// </summary>
        public Style ActiveStyle;

        protected override SettingsThreadedBase CloneForSaveThreaded()
        {
            // Note: this currently makes only a partial clone. This means that the user could potentially change some important setting
            // while a background save is happening, resulting in a save that's inconsistent. The risk is not non-existent, but fairly low.
            // Still, doing a deep clone here would be nice.
            var result = (Settings) MemberwiseClone();
            return result;
        }

        #region Upgrade-related

        void IClassifyObjectProcessor<XElement>.AfterSerialize(XElement element) { }
        void IClassifyObjectProcessor<XElement>.BeforeDeserialize(XElement element) { }
        void IClassifyObjectProcessor<XElement>.BeforeSerialize() { }
        void IClassifyObjectProcessor<XElement>.AfterDeserialize(XElement element)
        {
            // Some people have strange broken styles in their settings, probably added due to some bug in an earlier version of the program.
            Styles.RemoveWhere(style => style.Layers == null || style.Layers.Count == 0);
            foreach (var style in Styles)
            {
                if (style.Name == null)
                    style.Name = "<unknown>";
                if (style.Author == null)
                    style.Author = "<unknown>";
                if (style.PathTemplate == null)
                    style.PathTemplate = "";
            }

            // Added in v019
            if (SavedByVersion < 19 && GameInstalls != null)
                GameInstallations = GameInstalls;
#pragma warning disable 0618 // ActiveInstallation should only be used for loading/saving the setting, which is what the code below does.
            if (SavedByVersion < 19 && SelectedGamePath != null)
                ActiveInstallation = GameInstallations.Where(gi => gi.Path.EqualsNoCase(SelectedGamePath)).FirstOrDefault() ?? GameInstallations.FirstOrDefault();
#pragma warning restore 0618
            if (SavedByVersion < 19 && SelectedStyleNameAndAuthor != null)
                // This is a fairly approximate match but this way at least some users will see the right style still selected. The old property was too lossy to allow for reliable matching.
                ActiveStyle = Styles.FirstOrDefault(s => SelectedStyleNameAndAuthor.Contains(s.Name) && SelectedStyleNameAndAuthor.Contains(s.Author));
            GameInstalls = null;
            SelectedGamePath = null;
            SelectedStyleNameAndAuthor = null;
        }

        // Fields obsoleted in v019
        [ClassifyIgnoreIfDefault]
        private ObservableSortedList<TimGameInstallation> GameInstalls;
        [ClassifyIgnoreIfDefault]
        private string SelectedGamePath;
        [ClassifyIgnoreIfDefault]
        private string SelectedStyleNameAndAuthor;

        #endregion
    }

    /// <summary>Specifies which tank icons are to be displayed in the preview area.</summary>
    /// <remarks>The numeric value corresponds to the index of the relevant item in the filter drop-down.</remarks>
    enum DisplayFilter
    {
        All = 0,
        OneOfEach = 1,
        USSR = 3, Germany, USA, France, UK, China, Japan, Czech, Sweden, Poland, Italy, Intunion,
        Light = 16, Medium, Heavy, Artillery, Destroyer,
        Normal = 22, Premium, Special, Collector,
        TierLow = 27, TierMedHigh, TierHigh,
    }
}
