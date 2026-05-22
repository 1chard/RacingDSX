using System.Drawing;
using System.Windows.Forms;


namespace RacingDSX.Graphics
{
    public class ThemeObject
    {
        public Color BackColor { get; set; }
        public Color SurfaceColor { get; set; }
        public Color ForeColor { get; set; }
        public Color BorderColor { get; set; }
        public Color AccentColor { get; set; }
        public Color ButtonBackColor { get; set; }
        public Color ButtonForeColor { get; set; }
        public Color TextBoxBackColor { get; set; }
        public Color TextBoxForeColor { get; set; }
        public FlatStyle ButtonFlatStyle { get; set; }
        public int ButtonAppearanceBorderSize { get; set; }
        public Color ButtonAppearanceBorderColor { get; set; }
        public Color ButtonAppearanceMouseOverBackColor { get; set; }
        public bool UseCustomMenuRenderer { get; set; }
    }
}
