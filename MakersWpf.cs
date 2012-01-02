using System;
using System.Globalization;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using D = System.Drawing;
using DI = System.Drawing.Imaging;

namespace TankIconMaker
{
    abstract class IconMakerWpf : IconMaker
    {
        public override BitmapSource DrawTankInternal(Tank tank)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
                DrawTank(tank, context);
            var bitmap = new RenderTargetBitmap(80, 24, 96, 96, PixelFormats.Default);
            bitmap.Render(visual);
            return bitmap;
        }

        public abstract void DrawTank(Tank tank, DrawingContext context);
    }

    class TextCompareWpf : IconMakerWpf
    {
        public override string Name { get { return "Text: WPF"; } }
        public override string Author { get { return "Romkyns"; } }
        public override int Version { get { return 1; } }

        int FontSize { get; set; }

        public TextCompareWpf()
        {
            FontSize = 8;
        }

        public override void DrawTank(Tank tank, DrawingContext context)
        {
            context.DrawRectangle(Brushes.Black, null, new Rect(1, 1, 78, 22));
            var txt = new FormattedText("Matilda", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), FontSize * 1.333333, Brushes.White);
            context.DrawText(txt, new Point(2, 2));
        }
    }


    class DarkScout : IconMakerWpf
    {
        public override string Name { get { return "Dark Scout (Black Spy replica)"; } }
        public override string Author { get { return "Romkyns"; } }
        public override int Version { get { return 1; } }

        private int _opacity;
        public int Opacity { get { return _opacity; } set { _opacity = Math.Max(0, Math.Min(255, value)); } }
        public Color ColorLight { get; set; }
        public Color ColorMedium { get; set; }
        public Color ColorHeavy { get; set; }
        public Color ColorDestroyer { get; set; }
        public Color ColorArtillery { get; set; }

        private Pen _outline, _outlineInner;
        private Brush _lightBackground, _mediumBackground, _heavyBackground, _destroyerBackground, _artilleryBackground;

        public DarkScout()
        {
            Opacity = 180;
            ColorLight = ColorHSV.FromHSV(120, 75, 55).ToColorWpf();
            ColorMedium = ColorHSV.FromHSV(48, 75, 59).ToColorWpf();
            ColorHeavy = ColorHSV.FromHSV(0, 0, 39).ToColorWpf();
            ColorDestroyer = ColorHSV.FromHSV(219, 74, 63).ToColorWpf();
            ColorArtillery = ColorHSV.FromHSV(0, 74, 71).ToColorWpf();

            _outline = new Pen(Brushes.Black, 1); _outline.Freeze();
            _outlineInner = new Pen(new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)), 1); _outlineInner.Freeze();
        }

        public override void Initialize()
        {
            _lightBackground = makeBackgroundBrush(ColorLight.WithAlpha(Opacity));
            _mediumBackground = makeBackgroundBrush(ColorMedium.WithAlpha(Opacity));
            _heavyBackground = makeBackgroundBrush(ColorHeavy.WithAlpha(Opacity));
            _destroyerBackground = makeBackgroundBrush(ColorDestroyer.WithAlpha(Opacity));
            _artilleryBackground = makeBackgroundBrush(ColorArtillery.WithAlpha(Opacity));
        }

        private Brush makeBackgroundBrush(Color color)
        {
            var hsv = ColorHSV.FromColor(color);
            var result = new LinearGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(hsv.ToColorWpf(), 0),
                    new GradientStop(hsv.ScaleValue(0.56).ToColorWpf(), 0.49),
                    new GradientStop(hsv.ScaleValue(0.39).ToColorWpf(), 0.51),
                    new GradientStop(hsv.ScaleValue(0.56).ToColorWpf(), 1),
                },
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
            };
            result.Freeze();
            return result;
        }

        public override void DrawTank(Tank tank, DrawingContext context)
        {
            context.DrawRectangle(tank.Class.Pick(_lightBackground, _mediumBackground, _heavyBackground, _destroyerBackground, _artilleryBackground),
                    _outline, new Rect(0.5, 1.5, 79, 21));
            context.DrawRectangle(null, _outlineInner, new Rect(1.5, 2.5, 77, 19));

            var font = new D.Font("Arial", 8f);
            var textbmp = Ut.NewGdiBitmap((D.Graphics g) =>
            {
                g.TextRenderingHint = D.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                g.DrawString(tank["OfficialName"], font, D.Brushes.White, right: 80 - 4, bottom: 24 - 5);
                g.DrawString(tank.Tier.ToString(), font, D.Brushes.White, left: 3, top: 1);
            });
            textbmp.DrawImage(textbmp.GetOutline());
            textbmp = textbmp.GetBlurred().DrawImage(textbmp);
            context.DrawImage(textbmp);
            context.DrawImage(textbmp.GetWpfSource());
            System.GC.KeepAlive(textbmp);
        }
    }

}
