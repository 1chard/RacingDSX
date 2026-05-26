using RacingDSX.Config;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static RacingDSX.RacingDSXWorker;

namespace RacingDSX
{
    public class Core
    {
        public RacingDSXWorker racingDSXWorker;
        public AppCheckThread appCheckWorker;
        public RacingDSX.Config.Config currentSettings;
        public RacingDSX.Config.Profile selectedProfile;
        public BindingList<String> executables = new BindingList<string>();

        public bool bForzaConnected = false;
        public bool bDsxConnected = false;

        public Task appCheckTask;
        public Task racingDSXTask;

        public ManualResetEvent eventTimeoutAttach;
        public string targetExecutableName = null;

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
            racingDSXWorker.Stop();
        }

        public Core(Process process, RacingDSX.Config.Config config, RacingDSX.Config.Profile profile)
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

        public void Initialize(Action<RacingDSXReportStruct> racingDsxHandler, Action<AppCheckReportStruct> appCheckHandler)
        {
            var forzaProgressHandler = new Progress<RacingDSXReportStruct>(racingDsxHandler);
            racingDSXWorker = new RacingDSXWorker(currentSettings, forzaProgressHandler);

            var progressHandler = new Progress<AppCheckReportStruct>(appCheckHandler);
            appCheckWorker = new AppCheckThread(ref currentSettings, progressHandler, this.process);

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
            if (racingDSXTask != null
                || racingDSXWorker == null)
                return;
            if (currentSettings.ActiveProfile == null)
                return;
            racingDSXTask = Task.Factory.StartNew(racingDSXWorker.Run, TaskCreationOptions.LongRunning);
        }

        public void StopRacingDSXThread()
        {
            try
            {
                if (racingDSXTask != null)
                {
                    racingDSXWorker.Stop();
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

        protected void StartAppCheckThread()
        {
            appCheckTask = Task.Factory.StartNew(appCheckWorker.Run, TaskCreationOptions.LongRunning);
        }
    }
}
