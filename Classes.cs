using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing.Imaging;

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

            Data = Ut.ReadCsvLines(filename).Select(lp =>
            {
                try { return new TankData(lp.Item2); }
                catch (Exception e) { throw new Exception(e.Message + " at line " + lp.Item1); }
            }).ToList().AsReadOnly();
        }
    }

    class DataFileExtra
    {
        public string Name { get; private set; }
        public string Language { get; private set; }
        public string Author { get; private set; }
        public Version GameVersion { get; private set; }
        public int FileVersion { get; private set; }

        public IList<ExtraData> Data { get; private set; }

        public DataFileExtra(string name, string language, string author, Version gameVersion, int fileVersion, string filename)
        {
            Name = name;
            Language = language;
            Author = author;
            GameVersion = gameVersion;
            FileVersion = fileVersion;

            Data = Ut.ReadCsvLines(filename).Select(lp =>
            {
                try { return new ExtraData(lp.Item2); }
                catch (Exception e) { throw new Exception(e.Message + " at line " + lp.Item1); }
            }).ToList().AsReadOnly();
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

    abstract class IconMaker
    {
        public abstract string Name { get; }
        public abstract string Author { get; }
        public abstract int Version { get; }
        public abstract BytesBitmap DrawTank(Tank tank);

        protected static BytesBitmap NewBitmap()
        {
            return new BytesBitmap(80, 24, PixelFormat.Format32bppArgb);
        }
    }
}
