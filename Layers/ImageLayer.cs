using System.IO;
using RT.Util;
using RT.Util.Lingo;

namespace TankIconMaker.Layers
{
    sealed class TankImageLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.TankImageLayer.LayerName; } }
        public override string TypeDescription { get { return App.Translation.TankImageLayer.LayerDescription; } }

        public ImageBuiltInStyle Style { get; set; }
        public static MemberTr StyleTr(Translation tr) { return new MemberTr(tr.Category.Image, tr.TankImageLayer.Style); }

        public override BitmapBase Draw(Tank tank)
        {
            var image = tank.GetImageBuiltIn(Style);
            if (image == null)
            {
                tank.AddWarning(App.Translation.TankImageLayer.MissingImageWarning);
                return null;
            }
            return image;
        }
    }

    sealed class CurrentImageLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.CurrentImageLayer.LayerName; } }
        public override string TypeDescription { get { return App.Translation.CurrentImageLayer.LayerDescription; } }

        public override BitmapBase Draw(Tank tank)
        {
            var image = tank.GetImageCurrent();
            if (image == null)
            {
                tank.AddWarning(App.Translation.CurrentImageLayer.MissingImageWarning);
                return null;
            }
            return image;
        }
    }

    sealed class CustomImageLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.CustomImageLayer.LayerName; } }
        public override string TypeDescription { get { return App.Translation.CustomImageLayer.LayerDescription; } }

        public ValueSelector<Filename> ImageFile { get; set; }
        public static MemberTr ImageFileTr(Translation tr) { return new MemberTr(tr.Category.Image, tr.CustomImageLayer.ImageFile); }

        public CustomImageLayer()
        {
            ImageFile = new ValueSelector<Filename>("");
        }

        public override LayerBase Clone()
        {
            var result = (CustomImageLayer) base.Clone();
            result.ImageFile = ImageFile.Clone();
            return result;
        }

        public override BitmapBase Draw(Tank tank)
        {
            var filename = ImageFile.GetValue(tank);
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            var image = ImageCache.GetImage(new CompositePath(PathUtil.AppPath, filename));
            if (image == null && App.Settings.ActiveInstallation != null)
            {
                image = ImageCache.GetImage(new CompositePath(App.Settings.ActiveInstallation.Path, App.Settings.ActiveInstallation.GameVersionConfig.PathMods, filename));
                if (image == null)
                    image = ImageCache.GetImage(new CompositePath(App.Settings.ActiveInstallation.Path, filename));
            }
            if (image == null)
            {
                tank.AddWarning(App.Translation.CustomImageLayer.MissingImageWarning.Fmt(filename));
                return null;
            }
            return image;
        }
    }

    sealed class FilenamePatternImageLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.FilenamePatternImageLayer.LayerName; } }
        public override string TypeDescription { get { return App.Translation.FilenamePatternImageLayer.LayerDescription; } }

        public string Pattern { get; set; }
        public static MemberTr PatternTr(Translation tr) { return new MemberTr(tr.Category.Image, tr.FilenamePatternImageLayer.Pattern); }

        public FilenamePatternImageLayer()
        {
            Pattern = "Images/Example/tank-{country}-{class}-{category}.png";
        }

        public override BitmapBase Draw(Tank tank)
        {
            var filename = (Pattern ?? "")
                .Replace("{tier}", tank.Tier.ToString())
                .Replace("{country}", tank.Country.ToString().ToLower())
                .Replace("{class}", tank.Class.ToString().ToLower())
                .Replace("{category}", tank.Category.ToString().ToLower())
                .Replace("{id}", tank.SystemId);
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            var image = ImageCache.GetImage(new CompositePath(PathUtil.AppPath, filename));
            if (image == null && App.Settings.ActiveInstallation != null)
            {
                image = ImageCache.GetImage(new CompositePath(App.Settings.ActiveInstallation.Path, App.Settings.ActiveInstallation.GameVersionConfig.PathMods, filename));
                if (image == null)
                    image = ImageCache.GetImage(new CompositePath(App.Settings.ActiveInstallation.Path, filename));
            }
            if (image == null)
            {
                tank.AddWarning(App.Translation.FilenamePatternImageLayer.MissingImageWarning.Fmt(filename));
                return null;
            }
            return image;
        }
    }
}
