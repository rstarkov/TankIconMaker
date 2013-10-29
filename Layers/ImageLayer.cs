using System;
using System.IO;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.Lingo;
using WotDataLib;

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
            BitmapBase image;
            if (tank is TestTank)
            {
                image = (tank as TestTank).LoadedImage;
            }
            else
            {
                var installation = tank.Context.Installation;
                var config = tank.Context.VersionConfig;
                switch (Style)
                {
                    case ImageBuiltInStyle.Contour:
                        image = ImageCache.GetImage(new CompositePath(tank.Context, installation.Path, config.PathSourceContour, tank.TankId + config.TankIconExtension));
                        break;
                    case ImageBuiltInStyle.ThreeD:
                        image = ImageCache.GetImage(new CompositePath(tank.Context, installation.Path, config.PathSource3D, tank.TankId + config.TankIconExtension));
                        break;
                    case ImageBuiltInStyle.ThreeDLarge:
                        image = ImageCache.GetImage(new CompositePath(tank.Context, installation.Path, config.PathSource3DLarge, tank.TankId + config.TankIconExtension));
                        break;
                    case ImageBuiltInStyle.Country:
                        image = ImageCache.GetImage(new CompositePath(tank.Context, installation.Path, config.PathSourceCountry[tank.Country]));
                        break;
                    case ImageBuiltInStyle.Class:
                        image = ImageCache.GetImage(new CompositePath(tank.Context, installation.Path, config.PathSourceClass[tank.Class]));
                        break;
                    default:
                        throw new Exception("9174876");
                }
            }

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
            BitmapBase image;
            if (tank is TestTank)
            {
                image = (tank as TestTank).LoadedImage;
            }
            else
            {
                var installation = tank.Context.Installation;
                var config = tank.Context.VersionConfig;
                image = ImageCache.GetImage(new CompositePath(tank.Context, installation.Path, config.PathDestination, tank.TankId + config.TankIconExtension))
                    ?? ImageCache.GetImage(new CompositePath(tank.Context, installation.Path, config.PathSourceContour, tank.TankId + config.TankIconExtension));
            }

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

            var image = ImageCache.GetImage(new CompositePath(tank.Context, PathUtil.AppPath, filename));
            if (image == null)
            {
                image = ImageCache.GetImage(new CompositePath(tank.Context, tank.Context.Installation.Path, tank.Context.VersionConfig.PathMods, filename));
                if (image == null)
                    image = ImageCache.GetImage(new CompositePath(tank.Context, tank.Context.Installation.Path, filename));
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
            Pattern = "Images/Example/tank-{country}-{class}-{category}-{NameShort}.png";
        }

        public override BitmapBase Draw(Tank tank)
        {
            var filename = (Pattern ?? "")
                .Replace("{tier}", tank.Tier.ToString())
                .Replace("{country}", tank.Country.ToString().ToLower())
                .Replace("{class}", tank.Class.ToString().ToLower())
                .Replace("{category}", tank.Category.ToString().ToLower())
                .Replace("{id}", tank.TankId);
            filename = Regex.Replace(filename, @"{([^}]+)}", match => tank[match.Groups[1].Value] ?? "");
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            var image = ImageCache.GetImage(new CompositePath(tank.Context, PathUtil.AppPath, filename));
            if (image == null)
            {
                image = ImageCache.GetImage(new CompositePath(tank.Context, tank.Context.Installation.Path, tank.Context.VersionConfig.PathMods, filename));
                if (image == null)
                    image = ImageCache.GetImage(new CompositePath(tank.Context, tank.Context.Installation.Path, filename));
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
