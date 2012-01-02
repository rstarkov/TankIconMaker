using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using Microsoft.Windows.Controls;
using Microsoft.Windows.Controls.PropertyGrid.Editors;

namespace TankIconMaker
{
    abstract class IconMakerGdi : IconMaker
    {
        public override System.Windows.Media.Imaging.BitmapSource DrawTankInternal(Tank tank)
        {
            return DrawTank(tank).GetWpfSource();
        }

        public abstract BytesBitmap DrawTank(Tank tank);
    }


    class TextCompareGdi : IconMakerGdi
    {
        public override string Name { get { return "Text: GDI"; } }
        public override string Author { get { return "Romkyns"; } }
        public override int Version { get { return 1; } }

        int FontSize { get; set; }

        public TextCompareGdi()
        {
            FontSize = 8;
        }

        public override BytesBitmap DrawTank(Tank tank)
        {
            var result = Ut.NewGdiBitmap();
            using (var g = Graphics.FromImage(result.Bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                g.FillRectangle(Brushes.Black, new Rectangle(1, 1, 78, 22));
                g.DrawString("Matilda", new Font("Arial", FontSize), Brushes.White, new Point(2, 2), StringFormat.GenericTypographic);
            }
            return result;
        }
    }


    class Test1Maker : IconMakerGdi
    {
        public override string Name { get { return "Test 1"; } }
        public override string Author { get { return "Romkyns"; } }
        public override int Version { get { return 1; } }

        [Category("Tank name")]
        [DisplayName("Normal color")]
        public Color NameColor { get; set; }
        [Category("Tank name")]
        [DisplayName("Premium color")]
        public Color NameColorPremium { get; set; }
        [Category("Tank name")]
        [DisplayName("Special color")]
        [Description("For example, tanks that are given out to alpha-testers and can't be obtained otherwise.")]
        public Color NameColorSpecial { get; set; }

        public Test1Maker()
        {
            NameColor = Color.White;
            NameColorPremium = Color.Yellow;
            NameColorSpecial = Color.FromArgb(255, 100, 50);
        }

        public override BytesBitmap DrawTank(Tank tank)
        {
            var nameFont = new Font("Arial", 9f);
            var nameBrush = new SolidBrush(tank.Category == Category.Normal ? NameColor :
                tank.Category == Category.Premium ? NameColorPremium : NameColorSpecial);
            var backBrush = new SolidBrush(Color.FromArgb(128, 100, 150, 255));
            var result = Ut.NewGdiBitmap();
            using (var g = Graphics.FromImage(result.Bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                g.FillRectangle(backBrush, new Rectangle(1, 1, 78, 22));
                g.DrawString(tank["OfficialName"] ?? "-", nameFont, nameBrush, new Point(2, 2), StringFormat.GenericTypographic);
            }
            return result;
        }
    }
}
