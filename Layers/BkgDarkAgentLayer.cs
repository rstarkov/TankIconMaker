using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace TankIconMaker.Layers
{
    class BkgDarkAgentLayer : LayerBaseWpf
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Background / Dark Agent"; } }
        public override string TypeDescription { get { return "Draws a background using a glassy style inspired by Black_Spy’s icon set."; } }

        [Category("Settings")]
        [Description("Background color.")]
        public ColorSelector BackColor { get; set; }

        public BkgDarkAgentLayer()
        {
            BackColor = new ColorSelector(Colors.White);
            BackColor.By = SelectBy.Class;
            BackColor.ClassLight = Color.FromArgb(180, 35, 140, 35);
            BackColor.ClassMedium = Color.FromArgb(180, 150, 127, 37);
            BackColor.ClassHeavy = Color.FromArgb(180, 99, 99, 99);
            BackColor.ClassDestroyer = Color.FromArgb(180, 41, 83, 160);
            BackColor.ClassArtillery = Color.FromArgb(180, 181, 47, 47);
        }

        public override void Draw(Tank tank, DrawingContext dc)
        {
            var outline = new Pen(new SolidColorBrush(Colors.Black), 1);
            var outlineInner = new Pen(new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)), 1);

            var hsv = ColorHSV.FromColor(BackColor.GetColorWpf(tank));
            var brush = new LinearGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(hsv.ToColorWpf(), 0.1),
                    new GradientStop(hsv.ScaleValue(0.56).ToColorWpf(), 0.49),
                    new GradientStop(hsv.ScaleValue(0.39).ToColorWpf(), 0.51),
                    new GradientStop(hsv.ScaleValue(0.56).ToColorWpf(), 0.9),
                },
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
            };

            dc.DrawRectangle(brush, outline, new Rect(0.5, 1.5, 79, 21));
            dc.DrawRectangle(null, outlineInner, new Rect(1.5, 2.5, 77, 19));
        }
    }

}
