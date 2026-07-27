using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RacingDualSense.Graphics
{
    public class MenuRenderer : ToolStripProfessionalRenderer
    {
        Color colorDefault;
        Color colorPress;
        Color colorArrow;

        public MenuRenderer(Color colorDefault, Color colorPress, Color colorArrow) : base()
        {
            this.colorDefault = colorDefault;
            this.colorPress = colorPress;
            this.colorArrow = colorArrow;
        }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            var g = e.Graphics;
            var rect = new Rectangle(Point.Empty, e.Item.Size);

            Color bg;

            if (e.Item.Pressed)
            {
                bg = colorPress;
            }
            else if (e.Item.Selected)
            {
                bg = colorPress;
            }
            else
            {
                bg = colorDefault;
            }

            using var brush = new SolidBrush(bg);
            g.FillRectangle(brush, rect);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var g = e.Graphics;
            var rect = new Rectangle(Point.Empty, e.ImageRectangle.Size);

            g.FillRectangle(e.Item is ToolStripMenuItem item && item.Checked ? new SolidBrush(Color.FromArgb(120, 69, 162, 255)) : Brushes.Transparent, rect);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = colorArrow;

            base.OnRenderArrow(e);
        }
    }
}
