using RacingDSX.Config;
using System;
using System.ComponentModel;
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

        private void LoadSettings()
        {
            // Get values from the config given their key and their target type.
            currentSettings = ConfigHandler.GetConfig();
            selectedProfile = this.currentSettings.Profiles.Values.First();

            if (currentSettings.DisableAppCheck && currentSettings.DefaultProfile != null)
            {
                if (currentSettings.Profiles.ContainsKey(currentSettings.DefaultProfile))
                {
                    currentSettings.ActiveProfile = currentSettings.Profiles[currentSettings.DefaultProfile];
                }
            }
        }

        public void Initialize(Action<RacingDSXReportStruct> racingDsxHandler, Action<AppCheckReportStruct> appCheckHandler)
        {
            LoadSettings();

            var forzaProgressHandler = new Progress<RacingDSXReportStruct>(racingDsxHandler);

            RacingDSXWorker = new RacingDSXWorker(currentSettings, forzaProgressHandler);

            forzaThreadCancellationToken = new CancellationTokenSource();
            forzaThreadToken = forzaThreadCancellationToken.Token;

            forzaThreadToken.Register(() => RacingDSXWorker.Stop());
            var progressHandler = new Progress<AppCheckReportStruct>(appCheckReportStruct =>
            {
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

                appCheckHandler(appCheckReportStruct);
            });
            appCheckWorker = new AppCheckThread(ref currentSettings, progressHandler);
            appCheckThreadCancellationToken = new CancellationTokenSource();
            appCheckThreadToken = appCheckThreadCancellationToken.Token;

            appCheckThreadToken.Register(() => appCheckWorker.Stop());
            if (!currentSettings.DisableAppCheck)
            {
                startAppCheckThread();

            }
            else
            {
                StartRacingDSXThread();
            }
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
