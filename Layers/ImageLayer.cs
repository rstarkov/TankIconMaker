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

        private static string[] GetAllSeparatorVariants(string baseName)
        {
            var safe = (baseName ?? "").ToLowerInvariant();
            var parts = Regex.Split(safe, "[-_]");
            if (parts.Length <= 1)
                return new[] { safe };

            var separators = new[] { '_', '-' };
            var variants = new System.Collections.Generic.List<string>();
            for (int i = 0; i < (1 << (parts.Length - 1)); i++)
            {
                var variant = parts[0];
                for (int j = 0; j < parts.Length - 1; j++)
                    variant += separators[(i >> j) & 1] + parts[j + 1];
                variants.Add(variant);
            }
            return variants.ToArray();
        }

        public override BitmapBase Draw(Tank tank)
        {
            if (tank == null || tank.Context == null || tank.Context.Installation == null || tank.Context.VersionConfig == null)
                return null;

            BitmapBase image;
            if (tank is TestTank)
            {
                image = (tank as TestTank).LoadedImage;
            }
            else
            {
                var installation = tank.Context.Installation;
                var config = tank.Context.VersionConfig;
                var guiPackage = (config.GuiPackageName ?? "")
                    .Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (guiPackage.Length == 0)
                    guiPackage = new[] { "" };

                var fullImageName = (tank.ImageName ?? tank.TankId) + config.TankIconExtension;

                switch (Style)
                {
                    case ImageBuiltInStyle.Contour:
                        image = null;
                        foreach (string items in guiPackage)
                        {
                            if (string.IsNullOrEmpty(config.PathSourceContour))
                                continue;

                            image = ImageCache.GetImage(new CompositePath(
                                tank.Context, installation.Path,
                                config.PathSourceContour.Replace("\"GuiPackage\"", items),
                                fullImageName));
                            if (image != null)
                                break;
                        }
                        break;

                    case ImageBuiltInStyle.ThreeD:
                        image = null;
                        foreach (string items in guiPackage)
                        {
                            if (string.IsNullOrEmpty(config.PathSource3D))
                                continue;

                            image = ImageCache.GetImage(new CompositePath(
                                tank.Context, installation.Path,
                                config.PathSource3D.Replace("\"GuiPackage\"", items),
                                fullImageName));
                            if (image != null)
                                break;
                        }
                        break;

                    case ImageBuiltInStyle.ThreeDLarge:
                        image = null;
                        foreach (string items in guiPackage)
                        {
                            if (string.IsNullOrEmpty(config.PathSource3DLarge))
                                continue;

                            image = ImageCache.GetImage(new CompositePath(
                                tank.Context, installation.Path,
                                config.PathSource3DLarge.Replace("\"GuiPackage\"", items),
                                fullImageName));
                            if (image != null)
                                break;
                        }
                        break;

                    case ImageBuiltInStyle.ThreeDv2:
                        image = null;
                        {
                            var shortName = tank.ImageName ?? tank.TankId ?? "";
                            var dash = shortName.IndexOf('-');
                            if (dash > 0)
                                shortName = shortName.Substring(dash + 1);
                            var variants = GetAllSeparatorVariants(shortName);

                            foreach (string items in guiPackage)
                            {
                                var folder3d = (config.PathSource3Dv2 ?? "").Replace("\"GuiPackage\"", items);
                                if (string.IsNullOrEmpty(folder3d))
                                    continue;

                                foreach (var variant in variants)
                                {
                                    image = ImageCache.GetImage(new CompositePath(
                                        tank.Context,
                                        installation.Path,
                                        folder3d,
                                        variant + config.TankIconExtension));
                                    if (image != null)
                                    {
                                        Ut.SetReal3DImageName(tank.TankId, variant);
                                        break;
                                    }
                                }
                                if (image != null)
                                    break;
                            }
                        }
                        break;

                    case ImageBuiltInStyle.ThreeDLargev2:
                        image = null;
                        {
                            var shortName = tank.ImageName ?? tank.TankId ?? "";
                            var dash = shortName.IndexOf('-');
                            if (dash > 0)
                                shortName = shortName.Substring(dash + 1);
                            var variants = GetAllSeparatorVariants(shortName);

                            foreach (string items in guiPackage)
                            {
                                var folder = (config.PathSource3DLargev2 ?? "").Replace("\"GuiPackage\"", items);
                                if (string.IsNullOrEmpty(folder))
                                    continue;

                                foreach (var variant in variants)
                                {
                                    image = ImageCache.GetImage(new CompositePath(
                                        tank.Context,
                                        installation.Path,
                                        folder,
                                        variant + config.TankIconExtension));
                                    if (image != null)
                                    {
                                        Ut.SetReal3DImageName(tank.TankId, variant);
                                        break;
                                    }
                                }
                                if (image != null)
                                    break;
                            }
                        }
                        break;

                    case ImageBuiltInStyle.Country:
                        if (tank.Country == Country.None)
                            return null;
                        image = null;
                        foreach (string items in guiPackage)
                        {
                            if (config.PathSourceCountry == null || !config.PathSourceCountry.ContainsKey(tank.Country))
                                continue;

                            image = ImageCache.GetImage(new CompositePath(
                                tank.Context, installation.Path,
                                config.PathSourceCountry[tank.Country].Replace("\"GuiPackage\"", items)));
                            if (image != null)
                                break;
                        }
                        break;

                    case ImageBuiltInStyle.Class:
                        if (tank.Class == Class.None)
                            return null;
                        image = null;
                        foreach (string items in guiPackage)
                        {
                            if (config.PathSourceClass == null || !config.PathSourceClass.ContainsKey(tank.Class))
                                continue;

                            image = ImageCache.GetImage(new CompositePath(
                                tank.Context, installation.Path,
                                config.PathSourceClass[tank.Class].Replace("\"GuiPackage\"", items)));
                            if (image != null)
                                break;
                        }
                        break;

                    default:
                        throw new Exception("9174876");
                }
            }

            if (image == null)
            {
                if (tank.TankId != "unknown")
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
            if (tank == null || tank.Context == null || tank.Context.Installation == null || tank.Context.VersionConfig == null)
                return null;

            BitmapBase image;
            if (tank is TestTank)
            {
                image = (tank as TestTank).LoadedImage;
            }
            else
            {
                var installation = tank.Context.Installation;
                var config = tank.Context.VersionConfig;
                var guiPackage = (config.GuiPackageName ?? "")
                    .Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (guiPackage.Length == 0)
                    guiPackage = new[] { "" };

                image = ImageCache.GetImage(new CompositePath(
                    tank.Context, installation.Path,
                    config.PathDestination,
                    tank.TankId + config.TankIconExtension));

                foreach (string items in guiPackage)
                {
                    if (string.IsNullOrEmpty(config.PathSourceContour))
                        continue;

                    image = image ?? ImageCache.GetImage(new CompositePath(
                        tank.Context, installation.Path,
                        config.PathSourceContour.Replace("\"GuiPackage\"", items),
                        tank.TankId + config.TankIconExtension));
                    if (image != null)
                        break;
                }
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
            if (tank == null || tank.Context == null || tank.Context.Installation == null || tank.Context.VersionConfig == null)
                return null;

            var filename = ImageFile.GetValue(tank);
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            var image = ImageCache.GetImage(new CompositePath(tank.Context, PathUtil.AppPath, filename));
            if (image == null)
                image = ImageCache.GetImage(new CompositePath(
                    tank.Context,
                    tank.Context.Installation.Path,
                    tank.Context.VersionConfig.PathMods,
                    filename));
            if (image == null)
                image = ImageCache.GetImage(new CompositePath(
                    tank.Context,
                    tank.Context.Installation.Path,
                    filename));
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
            if (tank == null || tank.Context == null || tank.Context.Installation == null || tank.Context.VersionConfig == null)
                return null;

            var filename = (Pattern ?? "")
                .Replace("{tier}", tank.Tier.ToString())
                .Replace("{country}", tank.Country.ToString().ToLower())
                .Replace("{class}", tank.Class.ToString().ToLower())
                .Replace("{category}", tank.Category.ToString().ToLower())
                .Replace("{id}", tank.TankId);

            filename = Regex.Replace(filename, @"{([^}]+)}",
                match => tank[match.Groups[1].Value] ?? "");

            if (string.IsNullOrWhiteSpace(filename))
                return null;

            var image = ImageCache.GetImage(new CompositePath(tank.Context, PathUtil.AppPath, filename));
            if (image == null)
                image = ImageCache.GetImage(new CompositePath(
                    tank.Context,
                    tank.Context.Installation.Path,
                    tank.Context.VersionConfig.PathMods,
                    filename));
            if (image == null)
                image = ImageCache.GetImage(new CompositePath(
                    tank.Context,
                    tank.Context.Installation.Path,
                    filename));
            if (image == null)
            {
                tank.AddWarning(App.Translation.FilenamePatternImageLayer.MissingImageWarning.Fmt(filename));
                return null;
            }

            return image;
        }
    }
}
