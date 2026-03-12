using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RacingDSX.RacingDSXWorker;
using static RacingDSX.RacingDSXWorker.RacingDSXReportStruct;

namespace RacingDSX
{
    class AppContext : ApplicationContext
    {
        readonly Core core;
        readonly bool startGUI;
        NotifyIcon tray;
        UI ui;
        DateTime lastUpdate = DateTime.MinValue;
        ToolStripMenuItem dsxConnectionMenuItem;
        ToolStripMenuItem forzaConnectionMenuItem;
        ToolStripMenuItem udpForzaConnectionMenuItem;
        ToolStripMenuItem appCheckMenuItem;

        public AppContext(Core core, bool startGUI)
        {
            this.core = core;
            this.startGUI = startGUI;

            Application.Idle += Load;
        }

        private void Load(object sender, EventArgs e)
        {
            Application.Idle -= Load;

            core.Initialize(WorkerThreadReporter, AppCheckReporter, Application.Exit);

            tray = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "RacingDSX" + (core.targetExecutableName != null ? $" [{core.targetExecutableName}] " : "")
            };

            dsxConnectionMenuItem = new ToolStripMenuItem("")
            {
                Enabled = false
            };
            forzaConnectionMenuItem = new ToolStripMenuItem("")
            {
                Enabled = false
            };
            udpForzaConnectionMenuItem = new ToolStripMenuItem("")
            {
                Enabled = false
            };
            appCheckMenuItem = new ToolStripMenuItem("")
            {
                Enabled = false
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add(dsxConnectionMenuItem);
            menu.Items.Add(forzaConnectionMenuItem);
            menu.Items.Add(udpForzaConnectionMenuItem);
            menu.Items.Add(appCheckMenuItem);
            menu.Items.Add("Open Interface", null, OpenUI);
            menu.Items.Add("Exit", null, Exit);

            tray.ContextMenuStrip = menu;

            if (startGUI)
            {
                OpenUI(this, EventArgs.Empty);
            }

            Loop();
        }

        private async void Loop()
        {
            while (true)
            {
                var bConnectionUdp = (DateTime.Now - lastUpdate).TotalSeconds < 2;

                if (ui != null && !ui.IsDisposed)
                {
                    ui.SetUDPForzaConnectionStatus(bConnectionUdp);
                }

                dsxConnectionMenuItem.Text = $"DSX Connection: {(core.bDsxConnected ? "On" : "Off")}";
                forzaConnectionMenuItem.Text = $"Game Connection: {(core.bForzaConnected ? "On" : "Off")}";
                udpForzaConnectionMenuItem.Text = $"UDP Game Connection: {(bConnectionUdp ? "On" : "Off")}";
                appCheckMenuItem.Text = $"App Check: {(core.currentSettings.DisableAppCheck ? "Off" : "On")}";

                await Task.Delay(1000);
            }
        }

        private void OpenUI(object sender, EventArgs e)
        {
            if (ui == null || ui.IsDisposed)
                ui = new UI(core);

            ui.Show();
            ui.WindowState = FormWindowState.Normal;
            ui.BringToFront();
        }

        private void Exit(object sender, EventArgs e)
        {
            tray.Visible = false;
            ui?.Close();
            Application.Exit();
        }

        public void AppCheckReporter(AppCheckReportStruct value)
        {
            if (ui != null && !ui.IsDisposed)
            {
                ui.AppCheckReporter(value);
            }
        }

        public void WorkerThreadReporter(RacingDSXReportStruct value)
        {
            Console.WriteLine("[" + value.type + "] " + value.message);

            if(value.type == ReportType.HEARTBEAT)
            {
                lastUpdate = DateTime.Now;
                return;
            }

            if (ui != null && !ui.IsDisposed)
            {
                ui.WorkerThreadReporter(value);
            }
        }
    }


}
