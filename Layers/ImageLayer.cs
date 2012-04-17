using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace TankIconMaker.Layers
{
    public enum ImageStyle { Contour, [Description("3D")] ThreeD }

    class TankImageLayer : LayerBaseWpf
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Image / Tank"; } }
        public override string TypeDescription { get { return "Draws a tank in one of several styles."; } }

        [Category("Image")]
        [Description("Chooses one of the standard tank image styles.")]
        public ImageStyle Style { get; set; }

        public override void Draw(Tank tank, DrawingContext dc)
        {
            var image = Style == ImageStyle.Contour ? tank.LoadImageContourWpf() : tank.LoadImage3DWpf();
            if (image == null)
            {
                tank.AddWarning("The image for this tank is missing.");
                return;
            }
            dc.DrawImage(image, new Rect(0, 0, image.Width / image.Height * 24.0, 24));
        }
    }

    class CurrentImageLayer : LayerBaseWpf
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Image / Current"; } }
        public override string TypeDescription { get { return "Draws the icon that’s currently saved in the output directory."; } }

        public override void Draw(Tank tank, DrawingContext dc)
        {
            var image = tank.LoadImageCurrentWpf();
            if (image == null)
            {
                tank.AddWarning("There is no current image for this tank.");
                return;
            }
            dc.DrawImage(image);
        }
    }
}
