using Microsoft.Win32;
using RacingDSX.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RacingDSX.Graphics
{

    class ThemeManager
    {
        public static ThemeObject CurrentTheme { get; set; } = Themes.Dark;

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
                button.FlatStyle = CurrentTheme.ButtonFlatStyle;
                button.FlatAppearance.BorderSize = CurrentTheme.ButtonAppearanceBorderSize;
                button.FlatAppearance.BorderColor = CurrentTheme.ButtonAppearanceBorderColor;
                button.FlatAppearance.MouseOverBackColor = CurrentTheme.ButtonAppearanceMouseOverBackColor;
            }

            if (control is TextBox || control is ListView || control is ListBox || control is ComboBox)
            {
                control.ForeColor = CurrentTheme.TextBoxForeColor;
                control.BackColor = CurrentTheme.TextBoxBackColor;
            }

            if (control is ToolStrip toolStrip)
            {
                foreach (ToolStripItem item in toolStrip.Items)
                {
                    if(item is ToolStripDropDownButton dropdownButton)
                    {
                        dropdownButton.Owner.Renderer = new MenuRenderer(CurrentTheme.BackColor, CurrentTheme.ButtonAppearanceMouseOverBackColor, CurrentTheme.ForeColor);
                    }
                }
            }

            foreach (Control child in control.Controls)
            {
                ApplyToControl(child);
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
}
