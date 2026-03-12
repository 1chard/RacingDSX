using RacingDSX.Config;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static RacingDSX.RacingDSXWorker;

namespace RacingDSX
{
    public class Core
    {
        public RacingDSXWorker RacingDSXWorker;
        public AppCheckThread appCheckWorker;
        public RacingDSX.Config.Config currentSettings;
        public RacingDSX.Config.Profile selectedProfile;
        public BindingList<String> executables = new BindingList<string>();
        public RacingDSX.Config.Config CurrentSettings { get => currentSettings; set => currentSettings = value; }

        public bool bForzaConnected = false;
        public bool bDsxConnected = false;

        public Thread appCheckThread;
        public Thread RacingDSXThread;

        public CancellationTokenSource appCheckThreadCancellationToken;
        public CancellationToken appCheckThreadToken;

        public CancellationTokenSource forzaThreadCancellationToken;
        public CancellationToken forzaThreadToken;

        public ManualResetEvent eventTimeoutAttach;
        public string targetExecutableName = null;

        public bool bForzaOpenedOnceAttached = false;
        readonly Process process;

        public void Join()
        {
            if (RacingDSXThread != null)
            {
                RacingDSXThread.Join();
            }
        }

        public void close()
        {
            appCheckThreadCancellationToken.Cancel();
            appCheckThreadCancellationToken.Dispose();

            forzaThreadCancellationToken.Cancel();
            forzaThreadCancellationToken.Dispose();
        }

        public Core(Process process, RacingDSX.Config.Config config, RacingDSX.Config.Profile profile)
        {
            this.process = process;
            if (process != null)
            {
                targetExecutableName = process.ProcessName;
                bForzaOpenedOnceAttached = true;
            }
            currentSettings = config;
            selectedProfile = profile;
        }

        public void Initialize(Action<RacingDSXReportStruct> racingDsxHandler, Action<AppCheckReportStruct> appCheckHandler, Action appCloseHandler)
        {
            var forzaProgressHandler = new Progress<RacingDSXReportStruct>(racingDsxHandler);

            RacingDSXWorker = new RacingDSXWorker(currentSettings, forzaProgressHandler);

            forzaThreadCancellationToken = new CancellationTokenSource();
            forzaThreadToken = forzaThreadCancellationToken.Token;

            forzaThreadToken.Register(() => RacingDSXWorker.Stop());
            var progressHandler = new Progress<AppCheckReportStruct>(appCheckReportStruct =>
            {

                if (appCheckReportStruct.type == AppCheckReportStruct.AppType.DSX)
                {
                    bDsxConnected = appCheckReportStruct.value;
                }
                else if (appCheckReportStruct.type == AppCheckReportStruct.AppType.GAME)
                {
                    bForzaConnected = appCheckReportStruct.value;

                    var profileName = appCheckReportStruct.value ? appCheckReportStruct.message : null;

                    if (SwitchActiveProfile(profileName))
                    {
                        StopRacingDSXThread();
                        StartRacingDSXThread();
                    }
                }

                if (RacingDSXThread == null)
                {
                    if (bForzaConnected && bDsxConnected)
                    {
                        StartRacingDSXThread();
                    }
                }
                else
                {
                    if (!bForzaConnected || !bDsxConnected)
                    {
                        StopRacingDSXThread();
                    }
                }

                if (bForzaOpenedOnceAttached && appCheckReportStruct.type == AppCheckReportStruct.AppType.GAME && appCheckReportStruct.value == false)
                {
                    appCloseHandler();
                    return;
                }

                appCheckHandler(appCheckReportStruct);
            });

            appCheckWorker = new AppCheckThread(ref currentSettings, progressHandler, this.process);
            appCheckThreadCancellationToken = new CancellationTokenSource();
            appCheckThreadToken = appCheckThreadCancellationToken.Token;

            appCheckThreadToken.Register(() => appCheckWorker.Stop());
            if (!currentSettings.DisableAppCheck)
            {
                startAppCheckThread();
            }
            else
            {
                bDsxConnected = true;
                bForzaConnected = true;
                StartRacingDSXThread();
            }
        }

        public bool SwitchActiveProfile(String profileName)
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
            if (RacingDSXThread != null
                || RacingDSXWorker == null)
                return;
            if (currentSettings.ActiveProfile == null)
                return;
            RacingDSXThread = new Thread(new ThreadStart(RacingDSXWorker.Run));
            RacingDSXThread.IsBackground = true;

            RacingDSXThread.Start();
        }

        public void StopRacingDSXThread()
        {
            try
            {
                if (RacingDSXThread != null
                    && forzaThreadCancellationToken != null)
                {
                    forzaThreadCancellationToken.Cancel();
                }
            }
            catch (Exception)
            {

                throw;
            }

            RacingDSXThread = null;
        }

        public void RestartAppCheckThread()
        {
            StopAppCheckThread();
            System.Threading.Thread.Sleep(1100);
            startAppCheckThread();
        }

        public void StopAppCheckThread()
        {
            try
            {
                if (appCheckThread != null
                    && appCheckThreadCancellationToken != null)
                {
                    appCheckThreadCancellationToken.Cancel();
                }
            }
            catch (Exception)
            {
                throw;
            }

            appCheckThread = null;
        }

        protected void startAppCheckThread()
        {
            appCheckThread = new Thread(new ThreadStart(appCheckWorker.Run));
            appCheckThread.IsBackground = true;

            appCheckThread.Start();
        }
    }
}
