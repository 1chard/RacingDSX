using DualSenseSharp;
using RacingDualSense.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static RacingDualSense.RacingWorker;

namespace RacingDualSense
{
    public class Core
    {
        public RacingWorker racingWorker;
        public AppCheckThread appCheckWorker;
        public Config.Config CurrentSettings;
        public Profile selectedProfile;
        public BindingList<string> executables = new();

        public Dictionary<string, (DualSense BluetoothDs, DualSense UsbDs)> dualSenses = new Dictionary<string, (DualSense, DualSense)>();

        public bool bForzaConnected = false;
        public bool bDsxConnected = false;
        public bool bControllerConnected = false;

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
            CurrentSettings = config;
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
            racingWorker = new RacingWorker(CurrentSettings, forzaProgressHandler, () => activeController);

            var progressHandler = new Progress<AppCheckReportStruct>(appCheckHandler);
            appCheckWorker = new AppCheckThread(ref CurrentSettings, progressHandler, process);

            if (!CurrentSettings.DisableAppCheck || targetExecutableName != null)
            {
                StartAppCheckThread();
            }
            else
            {
                bForzaConnected = true;
                bDsxConnected = CurrentSettings.DSXPort != null;
                StartRacingDualSenseThread();
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
            if (CurrentSettings.ActiveProfile != null && CurrentSettings.ActiveProfile.Name == profileName)
                return false;

            if (profileName != null && CurrentSettings.Profiles.ContainsKey(profileName))
            {
                profile = CurrentSettings.Profiles[profileName];

            }
            CurrentSettings.ActiveProfile = profile;
            ConfigHandler.SaveConfig();

            return true;
        }

        public void RestartRacingDualSenseThread()
        {
            StopRacingDualSenseThread();
            StartRacingDualSenseThread();
        }
        public void StartRacingDualSenseThread()
        {
            if (racingDSXTask != null || racingWorker == null)
                return;
            if (CurrentSettings.ActiveProfile == null)
                return;

            racingDSXTask = Task.Factory.StartNew(racingWorker.Run, TaskCreationOptions.LongRunning);
        }

        public void StopRacingDualSenseThread()
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

            bControllerConnected = true;

            dualSenseHandler(new DualSenseReportStruct(DualSenseReportStruct.StatusType.SELECT, "Controller selected", true, dualSense.UniqueId.ToString()));
        }

        public async void NoUseController()
        {
            var mac = activeController.UniqueId.ToString();
            activeController = null;

            bControllerConnected = false;

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
                mac = (await dualSense.ComputeUniqueId())?.ToString();
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

                if(mac != null)
                {
                    if (mac == activeController?.UniqueId.ToString() && dualSense.IsBluetooth == false && activeController.IsBluetooth) // prefer usb
                        shouldSelectController = true;
                    else if ((macAddressAutoConnect == null && CurrentSettings.DSXPort == null) || macAddressAutoConnect == mac)
                    {
                        macAddressAutoConnect = mac;
                        shouldSelectController = true;
                    }
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
