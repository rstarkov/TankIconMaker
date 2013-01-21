using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using RT.Util.Lingo;

namespace TankIconMaker.Layers
{
    class BkgDarkAgentLayer : LayerBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.BkgDarkAgentLayer.LayerName; } }
        public override string TypeDescription { get { return App.Translation.BkgDarkAgentLayer.LayerDescription; } }

        public ColorSelector BackColor { get; set; }
        public static MemberTr BackColorTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.BkgDarkAgentLayer.BackColor); }

        public BkgDarkAgentLayer()
        {
            BackColor = new ColorSelector(Colors.White);
            BackColor.By = SelectBy.Class;
            BackColor.ClassLight = Color.FromArgb(180, 35, 140, 35);
            BackColor.ClassMedium = Color.FromArgb(180, 150, 127, 37);
            BackColor.ClassHeavy = Color.FromArgb(180, 99, 99, 99);
            BackColor.ClassDestroyer = Color.FromArgb(180, 41, 83, 160);
            BackColor.ClassArtillery = Color.FromArgb(180, 181, 47, 47);
            BackColor.ClassNone = Color.FromArgb(180, 255, 255, 255);
        }

        public override LayerBase Clone()
        {
            var result = (BkgDarkAgentLayer) base.Clone();
            result.BackColor = BackColor.Clone();
            return result;
        }

        public override BitmapBase Draw(Tank tank)
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

            return Ut.NewBitmapWpf(dc =>
            {
                dc.DrawRectangle(brush, outline, new Rect(0.5, 1.5, 79, 21));
                dc.DrawRectangle(null, outlineInner, new Rect(1.5, 2.5, 77, 19));
            }).ToBitmapRam();
        }
    }

}
