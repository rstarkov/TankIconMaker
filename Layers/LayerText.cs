using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using Microsoft.Windows.Controls.PropertyGrid.Attributes;

namespace TankIconMaker
{
    abstract class LayerText : LayerBaseGdi
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Text: "; } }

        [Category("Tank tier"), DisplayName("Rendering style")]
        [Description("Determines how the tank name should be anti-aliased.")]
        public TextAntiAliasStyle AntiAlias { get; set; }

        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
        public bool Baseline { get; set; }

        [ExpandableObject]
        public ConfigColors TextColors { get; set; }

        protected abstract string GetText(Tank tank);

        public LayerText()
        {
            FontFamily = "Arial";
            FontSize = 8.5;
            Left = 3;
            Top = 3;
            Right = -1;
            Bottom = -1;
            Baseline = false;
            TextColors = new ConfigColors();
        }

        public override void Draw(Tank tank, Graphics dc)
        {
            dc.TextRenderingHint = AntiAlias.ToGdi();
            dc.DrawString(GetText(tank), new Font(FontFamily, (float) FontSize), new SolidBrush(TextColors.GetColorGdi(tank)),
                Left < 0 ? null : (int?) Left, Right < 0 ? null : (int?) Right, Top < 0 ? null : (int?) Top, Bottom < 0 ? null : (int?) Bottom,
                Baseline);
        }
    }

    //class LayerTextProperty
}
