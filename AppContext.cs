using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RacingDSX.RacingWorker.RacingReportStruct;
using static RacingDSX.RacingWorker;

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
        ToolStripMenuItem controllerConnectionMenuItem;
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

            core.Initialize(WorkerThreadReporter, AppCheckReporter, DualSenseReporter);

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
            controllerConnectionMenuItem = new ToolStripMenuItem("")
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
            menu.Items.Add(controllerConnectionMenuItem);
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
                controllerConnectionMenuItem.Text = $"Controller Connection: {(core.bControllerConnected ? "On" : "Off")}";
                forzaConnectionMenuItem.Text = $"Game Is Running: {(core.bForzaConnected ? "On" : "Off")}";
                udpForzaConnectionMenuItem.Text = $"Game Connection: {(bConnectionUdp ? "On" : "Off")}";
                appCheckMenuItem.Text = $"App Check: {(core.CurrentSettings.DisableAppCheck && core.targetExecutableName == null ? "Off" : "On")}";

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

        public void AppCheckReporter(AppCheckReportStruct appCheckReportStruct)
        {
            if (appCheckReportStruct.type == AppCheckReportStruct.AppType.DSX)
            {
                core.bDsxConnected = appCheckReportStruct.value;
            }
            else if (appCheckReportStruct.type == AppCheckReportStruct.AppType.GAME)
            {
                core.bForzaConnected = appCheckReportStruct.value;

                var profileName = appCheckReportStruct.value ? appCheckReportStruct.message : null;

                if (core.SwitchActiveProfile(profileName))
                {
                    core.RestartAppCheckThread();
                }
            }

            if (core.racingDSXTask == null)
            {
                if (core.bForzaConnected)
                {
                    core.StartRacingDSXThread();
                }
            }
            else
            {
                if (!core.bForzaConnected)
                {
                    core.StopRacingDSXThread();
                }
            }

            if (core.bForzaOpenedOnceAttached && appCheckReportStruct.type == AppCheckReportStruct.AppType.GAME && appCheckReportStruct.value == false)
            {
                Application.Exit();
                return;
            }

            if (ui != null && !ui.IsDisposed)
            {
                ui.AppCheckReporter(appCheckReportStruct);
            }
        }

        public void WorkerThreadReporter(RacingReportStruct value)
        {
            if (value.type == ReportType.HEARTBEAT)
            {
                lastUpdate = DateTime.Now;
                return;
            }

            if (ui != null && !ui.IsDisposed)
            {
                ui.WorkerThreadReporter(value);
            }
        }

        public void DualSenseReporter(DualSenseReportStruct value)
        {
            if (ui != null && !ui.IsDisposed)
            {
                ui.DualSenseReporter(value);
            }
        }
    }


}
