using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RacingDSX
{

    public class ThemeManager
    {
        public static AppTheme CurrentTheme { get; set; } = Themes.Dark;

        public static bool IsDesignMode =>
            LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        public static void ApplyTheme(Control control)
        {
            if (control == null)
                return;

            Suspend(control);

            ApplyToControl(control);

            Resume(control);
        }

        private static void ApplyToControl(Control control)
        {
            control.BackColor = CurrentTheme.BackColor;
            control.ForeColor = CurrentTheme.ForeColor;

            if (control is Button button)
            {
                button.ForeColor = CurrentTheme.ButtonForeColor;
                button.BackColor = CurrentTheme.ButtonBackColor;
            }

            if (control is TextBox || control is ListView || control is ListBox || control is ComboBox)
            {
                control.ForeColor = CurrentTheme.TextBoxForeColor;
                control.BackColor = CurrentTheme.TextBoxBackColor;
            }

            if (control is ToolStrip toolStrip)
            {
                ApplyToolStripTheme(toolStrip);
            }

            foreach (Control child in control.Controls)
            {
                ApplyToControl(child);
            }
        }

        private static void ApplyToolStripTheme(ToolStrip toolStrip)
        {
            toolStrip.BackColor = CurrentTheme.SurfaceColor;
            toolStrip.ForeColor = CurrentTheme.ForeColor;

            foreach (ToolStripItem item in toolStrip.Items)
            {
                item.BackColor = CurrentTheme.SurfaceColor;
                item.ForeColor = CurrentTheme.ForeColor;
            }
        }


        private static void Suspend(Control control)
        {
            control.SuspendLayout();

            foreach (Control child in control.Controls)
            {
                Suspend(child);
            }
        }

        private static void Resume(Control control)
        {
            foreach (Control child in control.Controls)
            {
                Resume(child);
            }

            control.ResumeLayout();
        }

        public static bool IsDarkMode()
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

            if (key?.GetValue("AppsUseLightTheme") is int value)
            {
                return value == 0;
            }

            return false;
        }
    }

    public class AppTheme
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
    }

    public static class Themes
    {
        public static readonly AppTheme Plain = new()
        {
            BackColor = SystemColors.Control,
            SurfaceColor = SystemColors.ControlLight,
            ForeColor = SystemColors.ControlText,
            BorderColor = SystemColors.ControlDark,
            AccentColor = SystemColors.Highlight,
            ButtonBackColor = SystemColors.Control,
            ButtonForeColor = SystemColors.ControlText,
            TextBoxBackColor = SystemColors.Window,
            TextBoxForeColor = SystemColors.WindowText
        };

        public static AppTheme Dark = new()
        {
            BackColor = Color.FromArgb(30, 30, 30),
            SurfaceColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            BorderColor = Color.FromArgb(70, 70, 70),
            AccentColor = Color.FromArgb(0, 120, 215),
            ButtonBackColor = Color.FromArgb(60, 60, 60),
            ButtonForeColor = Color.White,
            TextBoxBackColor = Color.FromArgb(50, 50, 50),
            TextBoxForeColor = Color.White
        };

        public static AppTheme Light = new()
        {
            BackColor = Color.White,
            SurfaceColor = Color.FromArgb(240, 240, 240),
            ForeColor = Color.Black,
            BorderColor = Color.LightGray,
            AccentColor = Color.FromArgb(0, 120, 215),
            ButtonBackColor = Color.FromArgb(230, 230, 230),
            ButtonForeColor = Color.Black,
            TextBoxBackColor = Color.White,
            TextBoxForeColor = Color.Black
        };
    }
}
