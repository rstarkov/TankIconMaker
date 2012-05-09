using System;
using System.Collections.Generic;
using System.ComponentModel;
using RT.Util;
using RT.Util.Forms;
using WpfCrutches;

namespace TankIconMaker
{
    /// <summary>
    /// Stores all the settings for TankIconMaker. All the defaults set in the field initializer or the constructor are preserved
    /// whenever they are missing from the settings XML file. Background saves are supported efficiently via
    /// <see cref="SaveThreaded"/>, which is designed to be callable efficiently multiple times in a row.
    /// </summary>
    [Settings("TankIconMaker2", SettingsKind.UserSpecific)]
    sealed class Settings : SettingsThreadedBase
    {
        /// <summary>
        /// MainWindow settings, such as position, size and maximized state. Given that TankIconMaker is essentially a single-window
        /// application, all the other MainWindow-specific settings are stored directly in <see cref="Settings"/> for simplicity.
        /// </summary>
        public ManagedWindow.Settings MainWindow = new ManagedWindow.Settings();
        /// <summary>AddWindow-related settings</summary>
        public ManagedWindow.Settings AddWindow = new ManagedWindow.Settings();
        /// <summary>RenameWindow-related settings</summary>
        public ManagedWindow.Settings RenameWindow = new ManagedWindow.Settings();
        /// <summary>The width of the sidebar in the main window; null to use the width set at design time.</summary>
        public double? LeftColumnWidth = null;
        /// <summary>The width of the name column in the maker property editor; null to use the width set at design time.</summary>
        public double? NameColumnWidth = null;
        /// <summary>The last used index of the display mode dropdown.</summary>
        public int? DisplayMode = null;

        /// <summary>The name and author of the last used maker.</summary>
        public string SelectedStyleNameAndAuthor;

        /// <summary>A list of all the available user styles. Built-in styles are not stored in the settings file.</summary>
        public ObservableSortedList<Style> Styles = new ObservableSortedList<Style>();

        /// <summary>The last selected game install location.</summary>
        public string SelectedGamePath = Ut.FindTanksDirectory();

        /// <summary>Program language (in the future), and also the default language for property values when not specified.</summary>
        public string Language = "Ru";
        /// <summary>If a maker requests a property by name only, this author is given preference.</summary>
        public string DefaultPropertyAuthor = "Romkyns";

        /// <summary>Settings that are specific to a game installation path.</summary>
        public ObservableSortedList<GameInstallationSettings> GameInstalls = new ObservableSortedList<GameInstallationSettings>();

        protected override SettingsThreadedBase CloneForSaveThreaded()
        {
            var result = (Settings) MemberwiseClone();
            return result;
        }
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
        }

        /// <summary>Absolute path to the root of this game installation.</summary>
        public string Path
        {
            get { return _path; }
        }
        private string _path;

        /// <summary>The version of the game that is located at this path. Null iff there are no game versions defined at all.</summary>
        public GameVersion GameVersion
        {
            get { return Program.Data.GetVersion(_version) ?? Program.Data.GetGuessedVersion(_path); }
            set
            {
                if (value == null)
                    return;
                _version = value.Version;
                PropertyChanged(this, new PropertyChangedEventArgs("GameVersion"));
                PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
            }
        }
        private VersionId _version;

        /// <summary>The value displayed in the drop-down.</summary>
        public string DisplayName { get { return (GameVersion == null ? "" : (GameVersion.DisplayName + ":  ")) + _path; } }
        public override string ToString() { return DisplayName; }

        public int CompareTo(GameInstallationSettings other)
        {
            if (other == null) return 1;
            if (_version == null && other._version == null)
                return 0;
            if (_version == null || other._version == null)
                return other._version == null ? 1 : -1;
            int result = -_version.CompareTo(other._version);
            if (result != 0)
                return result;
            return string.Compare(Path, other.Path);
        }

        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };
    }
}
