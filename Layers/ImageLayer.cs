using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using RT.Util;

namespace TankIconMaker.Layers
{
    sealed class TankImageLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Image / Built-in"; } }
        public override string TypeDescription { get { return "Draws one of the several types of built-in images for this tank."; } }

        [Category("Image")]
        [Description("Chooses one of the standard tank image styles.")]
        public ImageBuiltInStyle Style { get; set; }

        public override BitmapBase Draw(Tank tank)
        {
            var image = tank.GetImageBuiltIn(Style);
            if (image == null)
            {
                tank.AddWarning("The image for this tank is missing.");
                return null;
            }
            return image;
        }
    }

    sealed class CurrentImageLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Image / Current"; } }
        public override string TypeDescription { get { return "Draws the icon that’s currently saved in the output directory."; } }

        public override BitmapBase Draw(Tank tank)
        {
            var image = tank.GetImageCurrent();
            if (image == null)
            {
                tank.AddWarning("There is no current image for this tank.");
                return null;
            }
            return image;
        }
    }

    sealed class CustomImageLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Image / Custom"; } }
        public override string TypeDescription { get { return "Draws an arbitrary, user-supplied image."; } }

        public ValueSelector<Filename> ImageFile { get; set; }

        public CustomImageLayer()
        {
            ImageFile = new ValueSelector<Filename>("");
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
                tank.AddWarning("The image {0} could not be found.".Fmt(filename));
                return null;
            }
            return image;
        }
    }
}
