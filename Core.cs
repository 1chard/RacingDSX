using DualSenseSharp;
using HidSharp.Utility;
using RacingDSX.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RacingDSX.RacingWorker;

namespace RacingDSX
{
    public class Core
    {
        public RacingWorker racingWorker;
        public AppCheckThread appCheckWorker;
        public RacingDSX.Config.Config currentSettings;
        public RacingDSX.Config.Profile selectedProfile;
        public BindingList<String> executables = new BindingList<string>();

        public Dictionary<string, (DualSense BluetoothDs, DualSense UsbDs)> dualSenses = new Dictionary<string, (DualSense, DualSense)>();

        public bool bForzaConnected = false;
        public bool bDsxConnected = false;

        public Task appCheckTask;
        public Task racingDSXTask;

        public ManualResetEvent eventTimeoutAttach;
        public string targetExecutableName = null;

        public DualSense activeController { get; private set; }
        public string macAddressAutoConnect { get; private set; }
        public Action<DualSenseReportStruct> dualSenseHandler;

        public bool bForzaOpenedOnceAttached = false;
        readonly Process process;

        public void Join()
        {
            if (racingDSXTask != null)
            {
                racingDSXTask.Wait();
            }
        }

        public void close()
        {
            appCheckWorker.Stop();
            racingWorker.Stop();
        }

        public Core(Process process, Config.Config config, Profile profile)
        {
            this.process = process;
            currentSettings = config;
            selectedProfile = profile;

            if (process != null)
            {
                targetExecutableName = process.ProcessName;
                bForzaOpenedOnceAttached = true;
            }
        }

        public void Initialize(Action<RacingReportStruct> racingDsxHandler, Action<AppCheckReportStruct> appCheckHandler, Action<DualSenseReportStruct> dualSenseHandler)
        {
            var forzaProgressHandler = new Progress<RacingReportStruct>(racingDsxHandler);
            racingWorker = new RacingWorker(currentSettings, forzaProgressHandler, () => activeController);

            var progressHandler = new Progress<AppCheckReportStruct>(appCheckHandler);
            appCheckWorker = new AppCheckThread(ref currentSettings, progressHandler, process);

            if (!currentSettings.DisableAppCheck || targetExecutableName != null)
            {
                StartAppCheckThread();
            }
            else
            {
                bDsxConnected = true;
                bForzaConnected = true;
                StartRacingDSXThread();
            }

            this.dualSenseHandler = dualSenseHandler;

            DualSense.Manager.Start();
            DualSense.Manager.Controllers.ForEach(PrepareController);
            DualSense.Manager.ControllerConnected += (x, ev) => PrepareController(ev.DualSense);
            DualSense.Manager.ControllerDisconnected += (x, ev) => RemoveController(ev.DualSense, true);
        }

        public bool SwitchActiveProfile(string profileName)
        {
            Profile profile = null;

            if (profileName == "")
            {
                return false;
            }
            if (currentSettings.ActiveProfile != null && currentSettings.ActiveProfile.Name == profileName)
                return false;

            if (profileName != null && currentSettings.Profiles.ContainsKey(profileName))
            {
                profile = currentSettings.Profiles[profileName];

            }
            currentSettings.ActiveProfile = profile;
            ConfigHandler.SaveConfig();

            return true;
        }

        public void StartRacingDSXThread()
        {
            if (racingDSXTask != null || racingWorker == null)
                return;
            if (currentSettings.ActiveProfile == null)
                return;

            racingDSXTask = Task.Factory.StartNew(racingWorker.Run, TaskCreationOptions.LongRunning);
        }

        public void StopRacingDSXThread()
        {
            try
            {
                if (racingDSXTask != null)
                {
                    racingWorker.Stop();
                }
            }
            catch (Exception)
            {
                throw;
            }

            racingDSXTask = null;
        }

        public void RestartAppCheckThread()
        {
            StopAppCheckThread();
            StartAppCheckThread();
        }

        public void StopAppCheckThread()
        {
            try
            {
                if (appCheckTask != null)
                {
                    appCheckWorker.Stop();
                }
            }
            catch (Exception)
            {
                throw;
            }

            appCheckTask = null;
        }

        public void StartAppCheckThread()
        {
            appCheckTask = Task.Factory.StartNew(appCheckWorker.Run, TaskCreationOptions.LongRunning);
        }

        public void UseController(DualSense dualSense)
        {
            activeController = dualSense;
            macAddressAutoConnect = dualSense.UniqueId.ToString();

            dualSenseHandler(new DualSenseReportStruct(DualSenseReportStruct.StatusType.SELECT, "Controller selected", true, dualSense.UniqueId.ToString()));
        }

        public async void NoUseController()
        {
            var mac = activeController.UniqueId.ToString();
            activeController = null;

            dualSenseHandler(new DualSenseReportStruct(DualSenseReportStruct.StatusType.UNSELECT, "Controller unselected", true, mac));
        }

        private async void PrepareController(DualSense dualSense)
        {
            bool result = false;
            string message = "";
            string mac = null;
            bool shouldSelectController = false;
            try
            {
                dualSense.Open();
                mac = (await dualSense.ComputeUniqueId()).ToString();
                if (mac == null)
                {
                    message = $"Failed to connect a controller";
                    dualSense.Dispose();
                    return;
                }

                var obj = dualSenses.GetValueOrDefault(mac, (null, null));
                string modeString;
                if (dualSense.IsBluetooth)
                {
                    obj.BluetoothDs = dualSense;
                    modeString = "Bluetooth";
                }
                else
                {
                    obj.UsbDs = dualSense;
                    modeString = "USB";
                }
                dualSenses[mac] = obj;
                dualSense.Disconnected += (x, ev) => RemoveController(dualSense, false);

                if(mac != null && mac == activeController?.UniqueId.ToString() && dualSense.IsBluetooth == false && activeController.IsBluetooth) // prefer usb
                    shouldSelectController = true;
                else if((macAddressAutoConnect == null && currentSettings.DSXPort == null) || macAddressAutoConnect == mac)
                {
                    macAddressAutoConnect = mac;
                    shouldSelectController = true;
                }
                
                message = $"Controller connected: {mac} ({modeString})";
                result = true;
            }
            catch (Exception ex)
            {
                message = $"Exception on connect: {ex.Message}";
            }
            finally
            {
                dualSenseHandler(new DualSenseReportStruct(DualSenseReportStruct.StatusType.CONNECT, message, result, mac));
                if (shouldSelectController)
                    UseController(dualSense);
            }
        }

        private async void RemoveController(DualSense dualSense, bool physicalRemoval)
        {
            bool result = false;
            string message = "";
            string mac = null;
            try
            {
                mac = (await dualSense.ComputeUniqueId()).ToString();
                if (mac == null)
                {
                    message = $"Controller without mac address";
                    return;
                }

                if (!dualSenses.ContainsKey(mac))
                {
                    message = $"Controller not registered (duplicate)";
                    return;
                }

                var obj = dualSenses[mac];
                string modeString;
                if (dualSense.IsBluetooth)
                {
                    obj.BluetoothDs = null;
                    modeString = "Bluetooth";
                }
                else
                {
                    obj.UsbDs = null;
                    modeString = "USB";
                }
                dualSenses[mac] = obj;

                if (obj.UsbDs == null && obj.BluetoothDs == null)
                {
                    dualSenses.Remove(mac);
                }
                
                if(mac == activeController?.UniqueId.ToString())
                {
                    var targetDS = (obj.UsbDs ?? obj.BluetoothDs);
                    if (targetDS != null)
                        UseController(targetDS);
                    else
                        NoUseController();
                }
                message = $"Controller disconnected: {mac} ({modeString})";
                result = true;
            }
            catch (Exception ex)
            {
                message = $"Exception on disconnect: {ex.Message}";
            }
            finally
            {
                dualSense.Dispose();
                dualSenseHandler(new DualSenseReportStruct(DualSenseReportStruct.StatusType.DISCONNECT, message, result, mac));
            }
        }
    }
}
