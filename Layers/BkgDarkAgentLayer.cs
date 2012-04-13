using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RT.Util.Xml;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace TankIconMaker.Layers
{
    class BkgDarkAgentLayer : LayerBaseWpf
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Background / Dark Agent"; } }
        public override string TypeDescription { get { return "Draws a background using a glassy style inspired by Black_Spy’s icon set."; } }

        [ExpandableObject]
        public ConfigColors BackColors { get; set; }

        public BkgDarkAgentLayer()
        {
            BackColors = new ConfigColors();
            BackColors.ColorBy = ColorBy.Class;
            BackColors.ClassLight = Color.FromArgb(180, 35, 140, 35);
            BackColors.ClassMedium = Color.FromArgb(180, 150, 127, 37);
            BackColors.ClassHeavy = Color.FromArgb(180, 99, 99, 99);
            BackColors.ClassDestroyer = Color.FromArgb(180, 41, 83, 160);
            BackColors.ClassArtillery = Color.FromArgb(180, 181, 47, 47);
        }

        public override void Draw(Tank tank, DrawingContext dc)
        {
            var outline = new Pen(new SolidColorBrush(Colors.Black), 1);
            var outlineInner = new Pen(new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)), 1);

            var hsv = ColorHSV.FromColor(BackColors.GetColorWpf(tank));
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
