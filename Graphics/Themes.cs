using System.Drawing;
using System.Windows.Forms;

namespace RacingDSX.Graphics
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
            ButtonBackColor = SystemColors.Control,
            ButtonForeColor = SystemColors.ControlText,
            TextBoxBackColor = SystemColors.Window,
            TextBoxForeColor = SystemColors.WindowText,
            ButtonFlatStyle = FlatStyle.Standard
        };

        public static readonly ThemeObject Dark = new()
        {
            BackColor = Color.FromArgb(30, 30, 30),
            SurfaceColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            BorderColor = Color.FromArgb(70, 70, 70),
            AccentColor = Color.FromArgb(0, 120, 215),
            ButtonBackColor = Color.FromArgb(50, 50, 50),
            ButtonForeColor = Color.White,
            TextBoxBackColor = Color.FromArgb(50, 50, 50),
            TextBoxForeColor = Color.White,
            ButtonFlatStyle = FlatStyle.Flat,
            ButtonAppearanceBorderSize = 0,
            ButtonAppearanceMouseOverBackColor = Color.FromArgb(60, 60, 60),
        };

        public static readonly ThemeObject Light = new()
        {
            BackColor = Color.White,
            SurfaceColor = Color.FromArgb(240, 240, 240),
            ForeColor = Color.Black,
            BorderColor = Color.LightGray,
            AccentColor = Color.FromArgb(0, 120, 215),
            ButtonBackColor = Color.FromArgb(230, 230, 230),
            ButtonForeColor = Color.Black,
            TextBoxBackColor = Color.White,
            TextBoxForeColor = Color.Black,
            ButtonFlatStyle = FlatStyle.Flat,
            ButtonAppearanceBorderSize = 1,
            ButtonAppearanceBorderColor = Color.DarkGray,
            ButtonAppearanceMouseOverBackColor = Color.FromArgb(220, 220, 220),
        };
    }
}