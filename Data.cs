using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using RT.Util.Collections;
using RT.Util.Xml;

namespace TankIconMaker
{
    /// <summary>
    /// Encapsulates all the available information about a single tank. Exposes methods to retrieve standard images for this tank.
    /// </summary>
    class Tank
    {
        /// <summary>WoT seems to consistently identify tanks by this string, which is unique for each tank and is also used in many filenames.</summary>
        public string SystemId { get; protected set; }
        /// <summary>Tank's country.</summary>
        public Country Country { get; protected set; }
        /// <summary>Tank's tier.</summary>
        public int Tier { get; protected set; }
        /// <summary>Tank's class: light/medium/heavy tank, artillery or tank destroyer.</summary>
        public Class Class { get; protected set; }
        /// <summary>Tank's category: normal (buyable for silver), premium (buyable for gold), or special (not for sale).</summary>
        public Category Category { get; protected set; }

        private Dictionary<string, string> _extras;

        private GameInstallationSettings _gameInstall;
        private GameVersion _gameVersion;

        protected Tank() { }

        /// <summary>Creates an instance ready to pass on to a Maker.</summary>
        /// <param name="tank">Tank data to copy into this instance.</param>
        /// <param name="extras">All the extra properties available for this tank and this game version.</param>
        /// <param name="gameInstall">Game install settings (to allow loading standard tank images).</param>
        /// <param name="gameVersion">Game version info (to allow loading standard tank images).</param>
        public Tank(TankData tank, IEnumerable<KeyValuePair<string, string>> extras, GameInstallationSettings gameInstall, GameVersion gameVersion)
        {
            if (gameInstall == null || gameVersion == null) throw new ArgumentNullException();
            SystemId = tank.SystemId;
            Country = tank.Country;
            Tier = tank.Tier;
            Class = tank.Class;
            Category = tank.Category;
            _extras = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var extra in extras)
                _extras.Add(extra.Key, extra.Value);
            _gameInstall = gameInstall;
            _gameVersion = gameVersion;
        }

        /// <summary>Constructs a clone of the specified tank but without any extra properties, in order to test a Maker for exceptions.</summary>
        public Tank(Tank tank)
        {
            SystemId = tank.SystemId;
            Country = tank.Country;
            Tier = tank.Tier;
            Class = tank.Class;
            Category = tank.Category;
            _extras = new Dictionary<string, string>();
            _gameInstall = tank._gameInstall;
            _gameVersion = tank._gameVersion;
        }

        /// <summary>
        /// Gets the value of an extra property. The extra property can be specified by full name ("Name/Language/Author"), which is
        /// also what the <see cref="DataSourceEditor"/> drop-down uses. If the property with such a name doesn't exist, a null value
        /// is returned. The maker must not crash just because some data files are missing, and hence must handle these nulls properly.
        /// </summary>
        public string this[string name]
        {
            get
            {
                string result;
                if (!_extras.TryGetValue(name, out result))
                    return null;
                return result;
            }
        }

        /// <summary>For debugging.</summary>
        public override string ToString()
        {
            return "Tank: " + SystemId;
        }

        /// <summary>
        /// Loads the standard 3D image for this tank and returns as a WPF image. Note that it's larger than 80x24.
        /// Returns null if the image file does not exist.
        /// </summary>
        public WriteableBitmap LoadImage3DWpf()
        {
            try { return Targa.LoadWpf(Path.Combine(_gameInstall.Path, _gameVersion.PathSource3D, SystemId + ".tga")); }
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
        }

        /// <summary>
        /// Loads the standard 3D image for this tank and returns as a GDI image. Note that it's larger than 80x24.
        /// Returns null if the image file does not exist.
        /// </summary>
        public BitmapGdi LoadImage3DGdi()
        {
            try { return Targa.LoadGdi(Path.Combine(_gameInstall.Path, _gameVersion.PathSource3D, SystemId + ".tga")); }
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
        }

        /// <summary>
        /// Loads the standard contour image for this tank and returns it as a WPF image.
        /// Returns null if the image file does not exist.
        /// </summary>
        public WriteableBitmap LoadImageContourWpf()
        {
            try { return Targa.LoadWpf(Path.Combine(_gameInstall.Path, _gameVersion.PathDestination, "original", SystemId + ".tga")); }
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
        }

        /// <summary>
        /// Loads the standard contour image for this tank and returns it as a GDI image.
        /// Returns null if the image file does not exist.
        /// </summary>
        public BitmapGdi LoadImageContourGdi()
        {
            try { return Targa.LoadGdi(Path.Combine(_gameInstall.Path, _gameVersion.PathDestination, "original", SystemId + ".tga")); }
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
        }
    }

    /// <summary>
    /// Encapsulates information about a tank read from the built-in data file. This is a separate type from <see cref="Tank"/> to ensure
    /// that it is not accidentally passed on to a maker without filling out the extra properties or linking to a game version/install.
    /// </summary>
    class TankData : Tank
    {
        /// <summary>Parses a CSV row from a built-in data file.</summary>
        public TankData(string[] fields)
        {
            if (fields.Length < 5)
                throw new Exception(string.Format("Expected at least 5 fields"));

            SystemId = fields[0];

            switch (fields[1])
            {
                case "ussr": Country = Country.USSR; break;
                case "germany": Country = Country.Germany; break;
                case "usa": Country = Country.USA; break;
                case "china": Country = Country.China; break;
                case "france": Country = Country.France; break;
                default: throw new Exception(string.Format("Unrecognized country: \"{0}\"", fields[1]));
            }

            int tier;
            if (!int.TryParse(fields[2], out tier))
                throw new Exception(string.Format("The tier field was not a whole number: \"{0}\"", fields[2]));
            if (tier < 1 || tier > 10)
                throw new Exception("Tank tier is not in the 1..10 range");
            Tier = tier;

            switch (fields[3])
            {
                case "light": Class = Class.Light; break;
                case "medium": Class = Class.Medium; break;
                case "heavy": Class = Class.Heavy; break;
                case "destroyer": Class = Class.Destroyer; break;
                case "artillery": Class = Class.Artillery; break;
                default: throw new Exception(string.Format("Unrecognized class: \"{0}\"", fields[3]));
            }

            switch (fields[4])
            {
                case "normal": Category = Category.Normal; break;
                case "premium": Category = Category.Premium; break;
                case "special": Category = Category.Special; break;
                default: throw new Exception(string.Format("Unrecognized category: \"{0}\"", fields[4]));
            }
        }
    }

    /// <summary>
    /// Represents a built-in data file, including its properties and the actual data it holds.
    /// </summary>
    class DataFileBuiltIn
    {
        /// <summary>Which game version this data file was made for.</summary>
        public Version GameVersion { get; private set; }
        /// <summary>The file version of this file.</summary>
        public int FileVersion { get; private set; }

        /// <summary>All the data held in this data file. Note that this list is read-only.</summary>
        public IList<TankData> Data { get; private set; }

        /// <summary>For debugging.</summary>
        public override string ToString()
        {
            return "BuiltIn-{0}-{1}".Fmt(GameVersion, FileVersion);
        }

        /// <summary>Constructs an instance and parses the data from the specified file.</summary>
        public DataFileBuiltIn(Version gameVersion, int fileVersion, string filename)
        {
            GameVersion = gameVersion;
            FileVersion = fileVersion;

            var lines = Ut.ReadCsvLines(filename).ToArray();
            if (lines.Length == 0)
                throw new Exception("Expected at least one line");
            var header = lines[0].Item2;
            if (header.Length < 2)
                throw new Exception("Expected at least two columns in the first row");
            if (header[0] != "WOT-BUILTIN-DATA")
                throw new Exception("Expected WOT-BUILTIN-DATA on first row");
            if (header[1] != "1")
                throw new Exception("The second column of the first row must be \"1\" (format version)");

            Data = lines.Skip(1).Select(lp =>
            {
                try { return new TankData(lp.Item2); }
                catch (Exception e) { throw new Exception(e.Message + " at line " + lp.Item1); }
            }).ToList().AsReadOnly();
        }

        /// <summary>Constructs an instance using the specified parameters and data.</summary>
        public DataFileBuiltIn(Version gameVersion, int fileVersion, IEnumerable<TankData> data)
        {
            GameVersion = gameVersion;
            FileVersion = fileVersion;
            Data = data.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Represents an "extra" property data file, including its properties and the actual data it holds.
    /// Note: extra data files come with complex inheritance rules, documented on the project website.
    /// </summary>
    class DataFileExtra
    {
        public string Name { get; private set; }
        public string Language { get; private set; }
        public string Author { get; private set; }
        public Version GameVersion { get; private set; }
        public int FileVersion { get; private set; }
        public string Description { get; private set; }
        public string InheritsFromName { get; private set; }
        public string InheritsFromAuthor { get; private set; }
        public string InheritsFromLanguage { get; private set; }

        public IList<ExtraData> Data { get; private set; }

        public override string ToString()
        {
            return "{0}-{1}-{2}-{3}-{4}".Fmt(Name, Language, Author, GameVersion, FileVersion);
        }

        public DataFileExtra(string name, string language, string author, Version gameVersion, int fileVersion, string filename)
        {
            Name = name;
            Language = language;
            Author = author;
            GameVersion = gameVersion;
            FileVersion = fileVersion;

            var lines = Ut.ReadCsvLines(filename).ToArray();
            if (lines.Length == 0)
                throw new Exception("Expected at least one line");
            var header = lines[0].Item2;
            if (header.Length < 2)
                throw new Exception("Expected at least two columns in the first row");
            if (header[0] != "WOT-DATA")
                throw new Exception("Expected WOT-DATA on first row");
            if (header[1] != "1")
                throw new Exception("The second column of the first row must be \"1\" (format version)");
            if (header.Length >= 3)
                Description = header[2];
            if (header.Length >= 4)
                InheritsFromName = header[3];
            if (header.Length >= 5)
                InheritsFromAuthor = header[4];
            if (header.Length >= 6)
                InheritsFromLanguage = header[5];

            if (InheritsFromName == "")
                InheritsFromName = null;
            if (InheritsFromAuthor == "")
                InheritsFromAuthor = null;
            if (InheritsFromLanguage == "")
                InheritsFromLanguage = null;

            Data = lines.Skip(1).Select(lp =>
            {
                try { return new ExtraData(lp.Item2); }
                catch (Exception e) { throw new Exception(e.Message + " at line " + lp.Item1); }
            }).ToList().AsReadOnly();
        }

        public DataFileExtra(DataFileExtra properties, IEnumerable<ExtraData> data)
        {
            Name = properties.Name;
            Language = properties.Language;
            Author = properties.Author;
            GameVersion = properties.GameVersion;
            FileVersion = properties.FileVersion;
            Description = properties.Description;
            InheritsFromName = properties.InheritsFromName;
            InheritsFromAuthor = properties.InheritsFromAuthor;
            InheritsFromLanguage = properties.InheritsFromLanguage;
            Data = data.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Represents a single value of an "extra" property.
    /// </summary>
    class ExtraData
    {
        /// <summary>System Id of the tank that this property is for.</summary>
        public string TankSystemId { get; private set; }
        /// <summary>Property value; can be anything at all.</summary>
        public string Value { get; private set; }

        /// <summary>Parses an "extra" property from an extra data file CSV row.</summary>
        public ExtraData(string[] fields)
        {
            if (fields.Length < 2)
                throw new Exception(string.Format("Expected at least 2 fields"));
            TankSystemId = fields[0];
            Value = fields[1];
        }

        /// <summary>For debugging.</summary>
        public override string ToString()
        {
            return "{0} = {1}".Fmt(TankSystemId, Value);
        }
    }

    /// <summary>
    /// Encapsulates all of the World of Tanks data available to the program, and implements methods
    /// to load the data off disk and retrieve a list of warnings indicating problems with the data.
    /// </summary>
    sealed class WotData
    {
        /// <summary>
        /// Gets a list of all the built-in data files available. Each file contains all the tanks, including those
        /// inherited from earlier game version files. This list is read-only.
        /// </summary>
        public IList<DataFileBuiltIn> BuiltIn { get { return _builtIn.AsReadOnly(); } }
        private readonly List<DataFileBuiltIn> _builtIn = new List<DataFileBuiltIn>();

        /// <summary>
        /// Gets a list of all the extra property data files available. Each file contains all the properties,
        /// including those it inherited from other files. This list is read-only.
        /// </summary>
        public IList<DataFileExtra> Extra { get { return _extra.AsReadOnly(); } }
        private readonly List<DataFileExtra> _extra = new List<DataFileExtra>();

        /// <summary>
        /// Gets a dictionary of all the game version information available. This dictionary is read-only.
        /// </summary>
        public IDictionary<Version, GameVersion> Versions { get { if (_versionsReadonly == null) _versionsReadonly = new ReadOnlyDictionary<Version, GameVersion>(_versions); return _versionsReadonly; } }
        private readonly Dictionary<Version, GameVersion> _versions = new Dictionary<Version, GameVersion>();
        private IDictionary<Version, GameVersion> _versionsReadonly;

        /// <summary>
        /// Gets a list of warnings issued while loading the data. These can be serious and help understand
        /// why data might be missing. This list is read-only.
        /// </summary>
        public IList<string> Warnings { get { return _warnings.AsReadOnly(); } }
        private List<string> _warnings = new List<string>();

        /// <summary>Represents an "extra" data file during loading, including all the extra info required to properly resolve inheritance.</summary>
        private class DataFileExtra2 : DataFileExtra
        {
            public List<DataFileExtra2> ImmediateParents = new List<DataFileExtra2>();
            public HashSet<DataFileExtra2> TransitiveChildren = new HashSet<DataFileExtra2>();
            public int NearestRoot;
            public DataFileExtra Result;

            public DataFileExtra2(string name, string language, string author, Version gameVersion, int fileVersion, string filename)
                : base(name, language, author, gameVersion, fileVersion, filename) { }
        }

        /// <summary>Clears all the data and performs a fresh load off disk.</summary>
        public void Reload(string path)
        {
            _builtIn.Clear();
            _extra.Clear();
            _versions.Clear();
            _warnings.Clear();

            if (Directory.Exists(path))
            {
                readGameVersions(path);
                readDataFiles(path);
            }

            // Remove versions that have no built-in data files available
            if (_builtIn.Any() && _versions.Any())
            {
                var minBuiltin = _builtIn.Min(b => b.GameVersion);
                foreach (var version in _versions.Keys.Where(k => k < minBuiltin).ToArray())
                {
                    _versions.Remove(version);
                    _warnings.Add("Skipped version {0} because there are no built-in data files for it to use. Delete the \"Data\\GameVersion-{0}.xml\" file to get rid of this warning.".Fmt(version));
                }
            }

            if (!_builtIn.Any() || !_versions.Any())
                _warnings.Add("Could not load any version data files and/or any built-in data files.");
        }

        private void readGameVersions(string path)
        {
            foreach (var fi in new DirectoryInfo(path).GetFiles("GameVersion-*.xml"))
            {
                var parts = fi.Name.Substring(0, fi.Name.Length - 4).Split('-');

                if (parts.Length != 2)
                {
                    _warnings.Add("Skipped \"{0}\" because it has the wrong number of filename parts.".Fmt(fi.Name));
                    continue;
                }

                Version gameVersion;
                if (!Version.TryParse(parts[1], out gameVersion))
                {
                    _warnings.Add("Skipped \"{0}\" because it has an unparseable game version part in the filename: \"{1}\".".Fmt(fi.Name, parts[1]));
                    continue;
                }

                try
                {
                    _versions.Add(gameVersion, XmlClassify.LoadObjectFromXmlFile<GameVersion>(fi.FullName));
                }
                catch (Exception e)
                {
                    _warnings.Add("Skipped \"{0}\" because the file could not be parsed: {1}".Fmt(fi.Name, e.Message));
                    continue;
                }
            }
        }

        private void readDataFiles(string path)
        {
            var builtin = new List<DataFileBuiltIn>();
            var extra = new List<DataFileExtra2>();
            var origFilenames = new Dictionary<object, string>();
            foreach (var fi in new DirectoryInfo(path).GetFiles("Data-*.csv"))
            {
                var parts = fi.Name.Substring(0, fi.Name.Length - 4).Split('-');
                var partsr = parts.Reverse().ToArray();

                if (parts.Length != 4 && parts.Length != 6)
                {
                    _warnings.Add("Skipped \"{0}\" because it has the wrong number of filename parts.".Fmt(fi.Name));
                    continue;
                }
                if (parts[1].EqualsNoCase("BuiltIn") && parts.Length != 4)
                {
                    _warnings.Add("Skipped \"{0}\" because it has too many filename parts for a BuiltIn data file.".Fmt(fi.Name));
                    continue;
                }
                if (parts.Length == 4 && !parts[1].EqualsNoCase("BuiltIn"))
                {
                    _warnings.Add("Skipped \"{0}\" because it has too few filename parts for a non-BuiltIn data file.".Fmt(fi.Name));
                    continue;
                }

                Version gameVersion;
                if (!Version.TryParse(partsr[1], out gameVersion))
                {
                    _warnings.Add("Skipped \"{0}\" because it has an unparseable game version part in the filename: \"{1}\".".Fmt(fi.Name, partsr[1]));
                    continue;
                }

                int fileVersion;
                if (!int.TryParse(partsr[0], out fileVersion))
                {
                    _warnings.Add("Skipped \"{0}\" because it has an unparseable file version part in the filename: \"{1}\".".Fmt(fi.Name, partsr[0]));
                    continue;
                }

                if (parts.Length == 4)
                {
                    var df = new DataFileBuiltIn(gameVersion, fileVersion, fi.FullName);
                    builtin.Add(df);
                    origFilenames[df] = fi.Name;
                }
                else
                {
                    string author = partsr[2].Trim();
                    if (author.Length == 0)
                    {
                        _warnings.Add("Skipped \"{0}\" because it has an empty author part in the filename.".Fmt(fi.Name));
                        continue;
                    }

                    string extraName = parts[1].Trim();
                    if (extraName.Length == 0)
                    {
                        _warnings.Add("Skipped \"{0}\" because it has an empty property name part in the filename.".Fmt(fi.Name));
                        continue;
                    }

                    string languageName = parts[2].Trim();
                    if (languageName.Length != 2)
                    {
                        _warnings.Add("Skipped \"{0}\" because its language name part in the filename is not a 2 letter long language code.".Fmt(fi.Name));
                        continue;
                    }

                    var df = new DataFileExtra2(extraName, languageName, author, gameVersion, fileVersion, fi.FullName);
                    extra.Add(df);
                    origFilenames[df] = fi.Name;
                }
            }

            resolveBuiltIn(builtin);
            resolveExtras(extra, origFilenames);
        }

        private void resolveBuiltIn(List<DataFileBuiltIn> builtin)
        {
            foreach (var group in builtin.GroupBy(df => new { gamever = df.GameVersion }).OrderBy(g => g.Key.gamever))
            {
                var tanks = new Dictionary<string, TankData>();

                // Inherit from the earlier game versions
                var earlierVer = _builtIn.OrderByDescending(df => df.GameVersion).FirstOrDefault();
                if (earlierVer != null)
                    foreach (var row in earlierVer.Data)
                        tanks[row.SystemId] = row;

                // Inherit from all the file versions for this game version
                foreach (var row in group.OrderBy(df => df.FileVersion).SelectMany(df => df.Data))
                    tanks[row.SystemId] = row;

                // Create a new data file with all the tanks
                _builtIn.Add(new DataFileBuiltIn(group.Key.gamever, group.Max(df => df.FileVersion), tanks.Values));
            }
        }

        private void resolveExtras(List<DataFileExtra2> extra, Dictionary<object, string> origFilenames)
        {
            while (true) // circular dependency removal requires us to iterate again if something got removed
            {
                // Make sure the explicit inheritance is resolvable, and complain if not
                var ignore = new List<DataFileExtra>();
                do
                {
                    ignore.Clear();
                    foreach (var e in extra.Where(e => e.InheritsFromName != null))
                    {
                        var p = extra.Where(df => df.Name == e.InheritsFromName).ToList();
                        if (p.Count == 0)
                        {
                            _warnings.Add("Skipped \"{0}\" because there are no data files for the property \"{1}\" (from which it inherits values).".Fmt(origFilenames[e], e.InheritsFromName));
                            ignore.Add(e);
                            continue;
                        }
                        if (e.InheritsFromLanguage != null)
                        {
                            p = p.Where(df => df.Language == e.InheritsFromLanguage).ToList();
                            if (p.Count == 0)
                            {
                                _warnings.Add("Skipped \"{0}\" because no data files for the property \"{1}\" (from which it inherits values) are in language \"{2}\"".Fmt(origFilenames[e], e.InheritsFromName, e.InheritsFromLanguage));
                                ignore.Add(e);
                                continue;
                            }
                        }
                        if (e.InheritsFromAuthor != null)
                        {
                            p = p.Where(df => df.Author == e.InheritsFromAuthor).ToList();
                            if (p.Count == 0)
                            {
                                _warnings.Add("Skipped \"{0}\" because no data files for the property \"{1}\" (from which it inherits values) are by author \"{2}\"".Fmt(origFilenames[e], e.InheritsFromName, e.InheritsFromAuthor));
                                ignore.Add(e);
                                continue;
                            }
                        }
                        p = p.Where(df => df.GameVersion <= e.GameVersion).ToList();
                        if (p.Count == 0)
                        {
                            _warnings.Add("Skipped \"{0}\" because no data files for the property \"{1}\"/\"{2}\" (from which it inherits values) have game version \"{3}\" or below.".Fmt(origFilenames[e], e.InheritsFromName, e.InheritsFromLanguage, e.GameVersion));
                            ignore.Add(e);
                            continue;
                        }
                    }
                    extra.RemoveAll(f => ignore.Contains(f));
                } while (ignore.Count > 0);

                // Determine all the immediate parents
                foreach (var e in extra)
                {
                    var sameNEL = extra.Where(df => df.Name == e.Name && df.Author == e.Author && df.Language == e.Language).ToList();

                    // Inherit from the explicitly specified file
                    if (e.InheritsFromName != null)
                    {
                        var p = extra.Where(df => df.GameVersion <= e.GameVersion && df.Name == e.InheritsFromName)
                            .OrderByDescending(df => df.GameVersion).AsEnumerable();
                        if (e.InheritsFromLanguage != null)
                            p = p.Where(df => df.Language == e.InheritsFromLanguage);
                        e.ImmediateParents.Add(p.Where(df => df.Author == e.InheritsFromAuthor).OrderByDescending(df => df.FileVersion).First());
                    }

                    // Inherit from the latest version of the same file for an earlier game version
                    var earlierGameVersion = sameNEL.Where(df => df.GameVersion < e.GameVersion).MaxAll(df => df.GameVersion).MaxOrDefault(df => df.FileVersion);
                    if (earlierGameVersion != null)
                        e.ImmediateParents.Add(earlierGameVersion);

                    // Inherit from an earlier version of this same file
                    var earlierVersionOfSameFile = sameNEL.Where(df => df.GameVersion == e.GameVersion && df.FileVersion < e.FileVersion)
                        .MaxOrDefault(df => df.FileVersion);
                    if (earlierVersionOfSameFile != null)
                        e.ImmediateParents.Add(earlierVersionOfSameFile);
                }

                // Compute the transitive closure
                bool added;
                foreach (var e in extra)
                    foreach (var p in e.ImmediateParents)
                        p.TransitiveChildren.Add(e);
                // Keep adding children's children until no further changes (quite a brute-force algorithm... potential bottleneck)
                do
                {
                    added = false;
                    foreach (var e in extra)
                        foreach (var c1 in e.TransitiveChildren.ToArray())
                            foreach (var c2 in c1.TransitiveChildren)
                                if (!e.TransitiveChildren.Contains(c2))
                                {
                                    e.TransitiveChildren.Add(c2);
                                    added = true;
                                }
                } while (added);

                // Detect dependency loops and remove them
                var looped = extra.Where(e => e.TransitiveChildren.Contains(e)).ToArray();
                foreach (var item in looped.ToArray())
                {
                    _warnings.Add("Skipped \"{0}\" due to a circular dependency.".Fmt(origFilenames[item]));
                    extra.Remove(item);
                }
                if (looped.Length == 0)
                    break;

                // Removed some data files. Other files could depend on them, so redo the whole resolution process with the reduced set.
            }

            // Compute the distance to nearest "root"
            foreach (var e in extra)
                e.NearestRoot = -1;
            while (extra.Any(e => e.NearestRoot == -1))
            {
                foreach (var e in extra.Where(e => e.NearestRoot == -1 && e.ImmediateParents.All(p => p.NearestRoot != -1)))
                    e.NearestRoot = e.ImmediateParents.Any() ? e.ImmediateParents.Min(p => p.NearestRoot) + 1 : 0;
            }

            // Get the full list of properties for every data file
            foreach (var e in extra.OrderBy(df => df.NearestRoot))
            {
                var tanks = new Dictionary<string, ExtraData>();

                // Inherit the properties (all the hard work is already done and the files to inherit from are in the correct order)
                foreach (var p in e.ImmediateParents)
                    foreach (var d in p.Result.Data)
                        tanks[d.TankSystemId] = d;
                foreach (var d in e.Data)
                    tanks[d.TankSystemId] = d;

                // Create a new data file with all the tanks
                e.Result = new DataFileExtra(e, tanks.Values);
            }

            // Keep only the latest file version of each file
            foreach (var e in extra.GroupBy(df => new { name = df.Name, language = df.Language, author = df.Author, gamever = df.GameVersion }))
                _extra.Add(e.Single(k => k.FileVersion == e.Max(m => m.FileVersion)).Result);
        }

    }

    /// <summary>Represents one of the WoT countries.</summary>
    enum Country
    {
        USSR,
        Germany,
        USA,
        France,
        China,
    }

    /// <summary>Represents one of the possible tank classes: light/medium/heavy, tank destroyer, and artillery.</summary>
    enum Class
    {
        Light,
        Medium,
        Heavy,
        Destroyer,
        Artillery,
    }

    /// <summary>Represents one of the possible tank categories based on how (and whether) they can be bought.</summary>
    enum Category
    {
        /// <summary>This tank can be bought for silver.</summary>
        Normal,
        /// <summary>This tank can be bought for gold only.</summary>
        Premium,
        /// <summary>This tank cannot be bought at all.</summary>
        Special,
    }

    /// <summary>
    /// Represents some settings for a particular game version.
    /// </summary>
    class GameVersion
    {
        /// <summary>How this version should be displayed in the UI. Note: not currently used.</summary>
        public string DisplayName { get; private set; }
        /// <summary>Relative path to the directory containing the tank icons we're creating.</summary>
        public string PathDestination { get; private set; }
        /// <summary>Relative path to the directory containing 3D tank images.</summary>
        public string PathSource3D { get; private set; }

        /// <summary>Relative path to a file whose size can be checked to auto-guess the game version.</summary>
        public string CheckFileName { get; private set; }
        /// <summary>Expected size of the <see cref="CheckFileName"/> file. A match means that this is probably the right game version.</summary>
        public long CheckFileSize { get; private set; }

        private GameVersion()
        {
            CheckFileSize = -1;
        }
    }
}
