using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.Xml;

namespace TankIconMaker
{
    class Tank
    {
        public string SystemId { get; protected set; }
        public Country Country { get; protected set; }
        public int Tier { get; protected set; }
        public Class Class { get; protected set; }
        public Category Category { get; protected set; }

        private Dictionary<string, string> _extras;

        protected Tank() { }

        public Tank(TankData tank, IEnumerable<KeyValuePair<string, string>> extras)
        {
            SystemId = tank.SystemId;
            Country = tank.Country;
            Tier = tank.Tier;
            Class = tank.Class;
            Category = tank.Category;
            _extras = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var extra in extras)
                _extras.Add(extra.Key, extra.Value);
        }

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
    }

    class TankData : Tank
    {
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

    class DataFileBuiltIn
    {
        public string Author { get; private set; }
        public Version GameVersion { get; private set; }
        public int FileVersion { get; private set; }

        public IList<TankData> Data { get; private set; }

        public DataFileBuiltIn(string author, Version gameVersion, int fileVersion, string filename)
        {
            Author = author;
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

        public DataFileBuiltIn(string author, Version gameVersion, int fileVersion, IEnumerable<TankData> data)
        {
            Author = author;
            GameVersion = gameVersion;
            FileVersion = fileVersion;
            Data = data.ToList().AsReadOnly();
        }
    }

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

        public DataFileExtra(string name, string language, string author, Version gameVersion, int fileVersion, IEnumerable<ExtraData> data)
        {
            Name = name;
            Language = language;
            Author = author;
            GameVersion = gameVersion;
            FileVersion = fileVersion;
            Data = data.ToList().AsReadOnly();
        }
    }

    class ExtraData
    {
        public string TankSystemId { get; private set; }
        public string Value { get; private set; }

        public ExtraData(string[] fields)
        {
            if (fields.Length < 2)
                throw new Exception(string.Format("Expected at least 2 fields"));
            TankSystemId = fields[0];
            Value = fields[1];
        }
    }

    class WotData
    {
        public List<DataFileBuiltIn> BuiltIn = new List<DataFileBuiltIn>();
        public List<DataFileExtra> Extra = new List<DataFileExtra>();
        public Dictionary<Version, GameVersion> Versions = new Dictionary<Version, GameVersion>();

        private class DataFileExtraWithInherit : DataFileExtra
        {
            public List<DataFileExtraWithInherit> ImmediateParents = new List<DataFileExtraWithInherit>();
            public HashSet<DataFileExtraWithInherit> TransitiveChildren = new HashSet<DataFileExtraWithInherit>();
            public DataFileExtra Result;

            public DataFileExtraWithInherit(string name, string language, string author, Version gameVersion, int fileVersion, string filename)
                : base(name, language, author, gameVersion, fileVersion, filename) { }
        }

        public void Reload(string path)
        {
            // Read data files off disk
            var builtin = new List<DataFileBuiltIn>();
            var extra = new List<DataFileExtraWithInherit>();
            var origFilenames = new Dictionary<object, string>();
            foreach (var fi in new DirectoryInfo(path).GetFiles("Data-*.csv"))
            {
                var parts = fi.Name.Substring(0, fi.Name.Length - 4).Split('-');
                var partsr = parts.Reverse().ToArray();

                if (parts.Length < 5 || parts.Length > 6)
                {
                    Console.WriteLine("Skipping \"{0}\" because it has the wrong number of filename parts.", fi.Name);
                    continue;
                }
                if (parts[1].EqualsNoCase("BuiltIn") && parts.Length != 5)
                {
                    Console.WriteLine("Skipping \"{0}\" because it has too many filename parts for a BuiltIn data file.", fi.Name);
                    continue;
                }
                if (parts.Length == 5 && !parts[1].EqualsNoCase("BuiltIn"))
                {
                    Console.WriteLine("Skipping \"{0}\" because it has too few filename parts for a non-BuiltIn data file.", fi.Name);
                    continue;
                }

                string author = partsr[2].Trim();
                if (author.Length == 0)
                {
                    Console.WriteLine("Skipping \"{0}\" because it has an empty author part in the filename.", fi.Name);
                    continue;
                }

                Version gameVersion;
                if (!Version.TryParse(partsr[1], out gameVersion))
                {
                    Console.WriteLine("Skipping \"{0}\" because it has an unparseable game version part in the filename: \"{1}\".", fi.Name, partsr[1]);
                    continue;
                }

                int fileVersion;
                if (!int.TryParse(partsr[0], out fileVersion))
                {
                    Console.WriteLine("Skipping \"{0}\" because it has an unparseable file version part in the filename: \"{1}\".", fi.Name, partsr[0]);
                    continue;
                }

                if (parts.Length == 5)
                {
                    var df = new DataFileBuiltIn(author, gameVersion, fileVersion, fi.FullName);
                    builtin.Add(df);
                    origFilenames[df] = fi.Name;
                }
                else
                {
                    string extraName = parts[1].Trim();
                    if (extraName.Length == 0)
                    {
                        Console.WriteLine("Skipping \"{0}\" because it has an empty property name part in the filename.", fi.Name);
                        continue;
                    }

                    string languageName = parts[2].Trim();
                    if (languageName.Length != 2)
                    {
                        Console.WriteLine("Skipping \"{0}\" because its language name part in the filename is not a 2 letter long language code.", fi.Name);
                        continue;
                    }

                    var df = new DataFileExtraWithInherit(extraName, languageName, author, gameVersion, fileVersion, fi.FullName);
                    extra.Add(df);
                    origFilenames[df] = fi.Name;
                }
            }

            // Resolve built-in data files
            BuiltIn.Clear();
            foreach (var group in builtin.GroupBy(df => new { author = df.Author, gamever = df.GameVersion }).OrderBy(g => g.Key.gamever))
            {
                var tanks = new Dictionary<string, TankData>();
                // Inherit from the earlier game versions by same author
                var earlierVer = BuiltIn.Where(df => df.Author == group.Key.author).OrderByDescending(df => df.GameVersion).FirstOrDefault();
                if (earlierVer != null)
                    foreach (var row in earlierVer.Data)
                        tanks[row.SystemId] = row;
                // Inherit from all the data files by this author/game version
                foreach (var row in group.OrderBy(df => df.FileVersion).SelectMany(df => df.Data))
                    tanks[row.SystemId] = row;
                // Create a new data file with all the tanks
                BuiltIn.Add(new DataFileBuiltIn(group.Key.author, group.Key.gamever, group.Max(df => df.FileVersion), tanks.Values));
            }

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
                        Console.WriteLine("Skipping \"{0}\" because there are no data files for the property \"{1}\" (from which it inherits values).".Fmt(origFilenames[e], e.InheritsFromName));
                        ignore.Add(e);
                        continue;
                    }
                    if (e.InheritsFromLanguage != null)
                    {
                        p = p.Where(df => df.Language == e.InheritsFromLanguage).ToList();
                        if (p.Count == 0)
                        {
                            Console.WriteLine("Skipping \"{0}\" because no data files for the property \"{1}\" (from which it inherits values) are in language \"{2}\"".Fmt(origFilenames[e], e.InheritsFromName, e.InheritsFromLanguage));
                            ignore.Add(e);
                            continue;
                        }
                    }
                    p = p.Where(df => df.GameVersion <= e.GameVersion).ToList();
                    if (p.Count == 0)
                    {
                        Console.WriteLine("Skipping \"{0}\" because no data files for the property \"{1}\"/\"{2}\" (from which it inherits values) have game version \"{3}\" or below.".Fmt(origFilenames[e], e.InheritsFromName, e.InheritsFromLanguage, e.GameVersion));
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

                // Inherit from an earlier version of this same file
                var earlierVersionOfSameFile = sameNEL.Where(df => df.GameVersion == e.GameVersion && df.FileVersion < e.FileVersion)
                    .MaxOrDefault(df => df.FileVersion);
                if (earlierVersionOfSameFile != null)
                    e.ImmediateParents.Add(earlierVersionOfSameFile);

                // Inherit from the latest version of the same file for an earlier game version
                var earlierGameVersion = sameNEL.Where(df => df.GameVersion < e.GameVersion).MaxAll(df => df.GameVersion).MaxOrDefault(df => df.FileVersion);
                if (earlierGameVersion != null)
                    e.ImmediateParents.Add(earlierGameVersion);

                // Inherit from the explicitly specified file
                if (e.InheritsFromName != null)
                {
                    var p = extra.Where(df => df.GameVersion <= e.GameVersion && df.Name == e.InheritsFromName).ToList();
                    if (e.InheritsFromLanguage != null)
                        p = p.Where(df => df.Language == e.InheritsFromLanguage).ToList();
                    e.ImmediateParents.Add(p.Where(df => df.Author == e.InheritsFromAuthor).FirstOrDefault() ?? p[0]);
                }
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
                    foreach (var c1 in e.TransitiveChildren)
                        foreach (var c2 in c1.TransitiveChildren)
                            if (!c1.TransitiveChildren.Contains(c2))
                            {
                                c1.TransitiveChildren.Add(c2);
                                added = true;
                            }
            } while (added);

            // Detect dependency loops and remove them
            var looped = extra.Where(e => e.TransitiveChildren.Contains(e)).ToArray();
            foreach (var item in looped.ToArray())
            {
                Console.WriteLine("Skipping \"{0}\" due to a circular dependency.".Fmt(origFilenames[item]));
                extra.Remove(item);
            }

            // Get the full list of properties for every data file
            foreach (var e in extra.OrderBy(df => df, new CustomComparer<DataFileExtraWithInherit>((df1, df2) => df1.TransitiveChildren.Contains(df2) ? -1 : df2.TransitiveChildren.Contains(df1) ? 1 : 0)))
            {
                var tanks = new Dictionary<string, ExtraData>();

                // Inherit the properties (all the hard work is already done and the files to inherit from are in the correct order)
                foreach (var p in e.ImmediateParents)
                    foreach (var d in p.Result.Data)
                        tanks[d.TankSystemId] = d;
                foreach (var d in e.Data)
                    tanks[d.TankSystemId] = d;

                // Create a new data file with all the tanks
                e.Result = new DataFileExtra(e.Name, e.Language, e.Author, e.GameVersion, e.FileVersion, tanks.Values);
            }

            // Keep only the latest file version of each file
            Extra.Clear();
            foreach (var e in extra.GroupBy(df => new { name = df.Name, language = df.Language, author = df.Author, gamever = df.GameVersion }))
                Extra.Add(e.Single(k => k.FileVersion == e.Max(m => m.FileVersion)).Result);

            // Read game versions off disk
            Versions.Clear();
            foreach (var fi in new DirectoryInfo(path).GetFiles("GameVersion-*.xml"))
            {
                var parts = fi.Name.Substring(0, fi.Name.Length - 4).Split('-');

                if (parts.Length != 2)
                {
                    Console.WriteLine("Skipping \"{0}\" because it has the wrong number of filename parts.", fi.Name);
                    continue;
                }

                Version gameVersion;
                if (!Version.TryParse(parts[1], out gameVersion))
                {
                    Console.WriteLine("Skipping \"{0}\" because it has an unparseable game version part in the filename: \"{1}\".", fi.Name, parts[1]);
                    continue;
                }

                try
                {
                    Versions.Add(gameVersion, XmlClassify.LoadObjectFromXmlFile<GameVersion>(fi.FullName));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Skipping \"{0}\" because the file could not be parsed: {1}", fi.Name, e.Message);
                    continue;
                }
            }

        }

    }

    enum Country
    {
        USSR,
        Germany,
        USA,
        France,
        China,
    }

    enum Class
    {
        Light,
        Medium,
        Heavy,
        Destroyer,
        Artillery,
    }

    enum Category
    {
        Normal,
        Premium,
        Special,
    }

#pragma warning disable 649

    class GameVersion
    {
        public string DisplayName;
        public string PathDestination;
        public string PathSource3D;

        public string CheckFileName;
        public long CheckFileSize = -1;
    }

#pragma warning restore 649
}
