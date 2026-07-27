using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RacingDualSense.Graphics
{
    public class CustomTabControl : TabControl
    {
        private Color _tabBackColor = SystemColors.Control;

        private Color _textColor = SystemColors.ControlText;

        private Color _selectedTabColor = SystemColors.Highlight;

        private Color _selectedTextColor = SystemColors.HighlightText;


        [Category("Appearance")]
        public Color TabBackColor
        {
            get => _tabBackColor;
            set
            {
                _tabBackColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color TabTextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                Invalidate();
            }
        }


        [Category("Appearance")]
        public Color SelectedTabColor
        {
            get => _selectedTabColor;
            set
            {
                _selectedTabColor = value;
                Invalidate();
            }
        }


        [Category("Appearance")]
        public Color SelectedTextColor
        {
            get => _selectedTextColor;
            set
            {
                _selectedTextColor = value;
                Invalidate();
            }
        }

        public CustomTabControl()
        {
            DrawMode = TabDrawMode.OwnerDrawFixed;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(TabBackColor);

            for (int i = 0; i < TabPages.Count; i++)
            {
                var rect = GetTabRect(i);

                bool selected = (SelectedIndex == i);

                Color bg = selected
                    ? SelectedTabColor
                    : TabBackColor;

                using var brush = new SolidBrush(bg);

                e.Graphics.FillRectangle(brush, rect);

                TextRenderer.DrawText(
                    e.Graphics,
                    TabPages[i].Text,
                    Font,
                    rect,
                    selected ? SelectedTextColor : TabTextColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter
                );
            }
        }
    }
}
