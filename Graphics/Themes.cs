using System.Drawing;
using System.Windows.Forms;

namespace RacingDualSense.Graphics
{
    public static class Themes
    {
        public static readonly ThemeObject Plain = new()
        {
            BackColor = SystemColors.Control,
            SurfaceColor = SystemColors.ControlLight,
            ForeColor = SystemColors.ControlText,
            BorderColor = SystemColors.ControlDark,
            AccentColor = SystemColors.Highlight,
            AccentTextColor = SystemColors.HighlightText,
            ButtonBackColor = SystemColors.Control,
            ButtonForeColor = SystemColors.ControlText,
            TextBoxBackColor = SystemColors.Window,
            TextBoxForeColor = SystemColors.WindowText,
            ButtonFlatStyle = FlatStyle.Standard,
            UseCustomMenuRenderer = false
        };

        public static readonly ThemeObject Dark = new()
        {
            BackColor = Color.FromArgb(30, 30, 30),
            SurfaceColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            BorderColor = Color.FromArgb(70, 70, 70),
            AccentColor = Color.FromArgb(0, 120, 215),
            AccentTextColor = Color.White,
            ButtonBackColor = Color.FromArgb(50, 50, 50),
            ButtonForeColor = Color.White,
            TextBoxBackColor = Color.FromArgb(50, 50, 50),
            TextBoxForeColor = Color.White,
            ButtonFlatStyle = FlatStyle.Flat,
            ButtonAppearanceBorderSize = 0,
            ButtonAppearanceMouseOverBackColor = Color.FromArgb(60, 60, 60),
            UseCustomMenuRenderer = true
        };

        public static readonly ThemeObject Light = new()
        {
            BackColor = Color.White,
            SurfaceColor = Color.FromArgb(240, 240, 240),
            ForeColor = Color.Black,
            BorderColor = Color.LightGray,
            AccentColor = Color.FromArgb(0, 120, 215),
            AccentTextColor = Color.White,
            ButtonBackColor = Color.FromArgb(230, 230, 230),
            ButtonForeColor = Color.Black,
            TextBoxBackColor = Color.White,
            TextBoxForeColor = Color.Black,
            ButtonFlatStyle = FlatStyle.Flat,
            ButtonAppearanceBorderSize = 1,
            ButtonAppearanceBorderColor = Color.DarkGray,
            ButtonAppearanceMouseOverBackColor = Color.FromArgb(220, 220, 220),
            UseCustomMenuRenderer = true
        };

        public static readonly ThemeObject Amoled = new()
        {
            BackColor = Color.FromArgb(0, 0, 0),
            SurfaceColor = Color.FromArgb(18, 18, 18),
            ForeColor = Color.FromArgb(245, 245, 245),
            BorderColor = Color.FromArgb(35, 35, 35),
            AccentColor = Color.FromArgb(0, 120, 215),
            AccentTextColor = Color.White,
            ButtonBackColor = Color.FromArgb(20, 20, 20),
            ButtonForeColor = Color.FromArgb(245, 245, 245),
            TextBoxBackColor = Color.FromArgb(10, 10, 10),
            TextBoxForeColor = Color.FromArgb(245, 245, 245),
            ButtonFlatStyle = FlatStyle.Flat,
            ButtonAppearanceBorderSize = 1,
            ButtonAppearanceBorderColor = Color.FromArgb(35, 35, 35),
            ButtonAppearanceMouseOverBackColor = Color.FromArgb(35, 35, 35),
            UseCustomMenuRenderer = true
        };
    }
}