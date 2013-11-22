using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using RT.Util.ExtensionMethods;
using RT.Util.Lingo;
using RT.Util.Xml;
using WotDataLib;

namespace TankIconMaker
{
    class Tank : WotTank
    {
        private Action<string> _addWarning;

        /// <param name="addWarning">
        ///     The method to be used to add warnings about this tank's rendering.</param>
        public Tank(WotTank tank, Action<string> addWarning)
            : base(tank)
        {
            _addWarning = addWarning;
        }

        public Tank(string tankId, int tier, Country country, Class class_, Category category)
            : base(tankId, country, tier, class_, category, Enumerable.Empty<KeyValuePair<ExtraPropertyId, string>>(), null)
        {
        }

        /// <summary>
        ///     Adds a warning about this tank's rendering. The user will see a big warning icon telling them to look for
        ///     warnings on specific tanks, and each image with warnings will have a little warning icon shown in it.</summary>
        public virtual void AddWarning(string warning)
        {
            _addWarning(warning);
        }

    }

    /// <summary>Used to test makers for bugs in handling missing data.</summary>
    class TestTank : Tank
    {
        /// <summary>Constructor.</summary>
        public TestTank(string tankId, int tier, Country country, Class class_, Category category, WotContext context)
            : base(tankId, tier, country, class_, category)
        {
            TankId = tankId;
            Tier = tier;
            Country = country;
            Class = class_;
            Category = category;
            Context = context;
        }

        public string PropertyValue;
        public BitmapBase LoadedImage;

        public override string this[string name] { get { return PropertyValue; } }
        public override string this[ExtraPropertyId property] { get { return PropertyValue; } }
        public override void AddWarning(string warning) { }
    }


    /// <summary>Represents one of the built-in tank image styles.</summary>
    [TypeConverter(typeof(ImageBuiltInStyleTranslation.Conv))]
    enum ImageBuiltInStyle
    {
        Contour,
        ThreeD,
        ThreeDLarge,
        Country,
        Class
    }

    class TimGameInstallation : GameInstallation, INotifyPropertyChanged
    {
        private TimGameInstallation() { } // for XmlClassify

        public TimGameInstallation(string path)
            : base(path)
        {
        }

        public override void Reload()
        {
            base.Reload();
            PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
        }

        /// <summary>The value displayed in the drop-down.</summary>
        public string DisplayName { get { return (GameVersionName ?? "?") + ":  " + Path; } }

        public event PropertyChangedEventHandler PropertyChanged = (_, __) => { };
    }
}
