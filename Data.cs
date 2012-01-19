using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using D = System.Drawing;
using DI = System.Drawing.Imaging;

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

    class DataFileExtraWithInherit : DataFileExtra
    {
        public List<DataFileExtraWithInherit> ImmediateParents = new List<DataFileExtraWithInherit>();
        public HashSet<DataFileExtraWithInherit> TransitiveChildren = new HashSet<DataFileExtraWithInherit>();
        public DataFileExtra Result;

        public DataFileExtraWithInherit(string name, string language, string author, Version gameVersion, int fileVersion, string filename)
            : base(name, language, author, gameVersion, fileVersion, filename) { }
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
