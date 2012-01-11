using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using D = System.Drawing;

namespace TankIconMaker
{
#if DEBUG
    class MakerTextCompare : MakerBaseWpf
    {
        public override string Name { get { return "Text compare"; } }
        public override string Author { get { return "Romkyns"; } }
        public override int Version { get { return 1; } }
        public override string Description { get { return "This is a text rendering test."; } }

        [Description("Only has any effect on the GDI version! Determines the anti-aliasing algorithm used.")]
        public TextAntiAliasStyle AntiAlias { get; set; }
        public double FontSize { get; set; }
        public bool UseWPF { get; set; }
        public string Text { get; set; }

        public MakerTextCompare()
        {
            AntiAlias = TextAntiAliasStyle.AliasedHinted;
            FontSize = 8.5;
            UseWPF = false;
            Text = "Matilda, medium";
        }

        public override void DrawTank(Tank tank, DrawingContext context)
        {
            context.DrawRectangle(Brushes.Black, null, new Rect(1, 1, 78, 22));

            if (UseWPF)
            {
                var txt = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), FontSize * 1.333333, Brushes.White);
                context.DrawText(txt, new Point(2, 2));
            }
            else
            {
                var layer = Ut.NewBitmapGdi((D.Graphics g) =>
                {
                    g.TextRenderingHint = AntiAlias.ToGdi();
                    g.DrawString(Text, new D.Font("Arial", (float) FontSize), D.Brushes.White, new D.Point(2, 2), D.StringFormat.GenericTypographic);
                });
                context.DrawImage(layer.ToWpf());
            }
        }
    }
#endif
}
