using System.IO;
using RT.Util;
using RT.Util.Lingo;

namespace TankIconMaker.Layers
{
    sealed class TankImageLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return Program.Translation.TankImageLayer.LayerName; } }
        public override string TypeDescription { get { return Program.Translation.TankImageLayer.LayerDescription; } }

        public ImageBuiltInStyle Style { get; set; }
        public static MemberTr StyleTr(Translation tr) { return new MemberTr(tr.CategoryImage, tr.TankImageLayer.Style); }

        public override BitmapBase Draw(Tank tank)
        {
            var image = tank.GetImageBuiltIn(Style);
            if (image == null)
            {
                tank.AddWarning(Program.Translation.TankImageLayer.MissingImageWarning);
                return null;
            }
            return image;
        }
    }

    sealed class CurrentImageLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return Program.Translation.CurrentImageLayer.LayerName; } }
        public override string TypeDescription { get { return Program.Translation.CurrentImageLayer.LayerDescription; } }

        public override BitmapBase Draw(Tank tank)
        {
            var image = tank.GetImageCurrent();
            if (image == null)
            {
                tank.AddWarning(Program.Translation.CurrentImageLayer.MissingImageWarning);
                return null;
            }
            return image;
        }
    }

    sealed class CustomImageLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return Program.Translation.CustomImageLayer.LayerName; } }
        public override string TypeDescription { get { return Program.Translation.CustomImageLayer.LayerDescription; } }

        public ValueSelector<Filename> ImageFile { get; set; }

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

            var image = ImageCache.GetImage(PathUtil.AppPathCombine(filename));
            if (image == null && Program.LastGameInstallSettings != null)
            {
                image = ImageCache.GetImage(Path.Combine(Program.LastGameInstallSettings.Path, Program.LastGameInstallSettings.GameVersion.PathMods, filename));
                if (image == null)
                    image = ImageCache.GetImage(Path.Combine(Program.LastGameInstallSettings.Path, filename));
            }
            if (image == null)
            {
                tank.AddWarning(Program.Translation.CustomImageLayer.MissingImageWarning.Fmt(filename));
                return null;
            }
            return image;
        }
    }

    sealed class FilenamePatternImageLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return Program.Translation.FilenamePatternImageLayer.LayerName; } }
        public override string TypeDescription { get { return Program.Translation.FilenamePatternImageLayer.LayerDescription; } }

        public string Pattern { get; set; }
        public static MemberTr PatternTr(Translation tr) { return new MemberTr(tr.CategoryImage, tr.FilenamePatternImageLayer.Pattern); }

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

            var image = ImageCache.GetImage(PathUtil.AppPathCombine(filename));
            if (image == null && Program.LastGameInstallSettings != null)
            {
                image = ImageCache.GetImage(Path.Combine(Program.LastGameInstallSettings.Path, Program.LastGameInstallSettings.GameVersion.PathMods, filename));
                if (image == null)
                    image = ImageCache.GetImage(Path.Combine(Program.LastGameInstallSettings.Path, filename));
            }
            if (image == null)
            {
                tank.AddWarning(Program.Translation.FilenamePatternImageLayer.MissingImageWarning.Fmt(filename));
                return null;
            }
            return image;
        }
    }
}
