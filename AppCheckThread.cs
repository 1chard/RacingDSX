using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RacingDSX
{
	public struct AppCheckReportStruct
	{
		public enum AppType : ushort
		{
			NONE = 0,
			DSX = 1,
			GAME = 2
		}

		public AppCheckReportStruct(AppType type, string msg, bool val = false)
		{
			this.type = type;
			this.message = msg;
			this.value = val;
		}

		public AppCheckReportStruct(AppType type, bool val, string msg = null)
		{
			this.type = type;
			this.message = msg;
			this.value = val;
		}

		public AppCheckReportStruct(AppType type)
		{
			this.type = type;
			this.message = String.Empty;
			this.value = false;
		}

		public AppType type = 0;
		public string message = string.Empty;
		public bool value = false;
	}

	public class AppCheckThread
	{
		readonly RacingDSX.Config.Config settings;
		private Dictionary<String, String> processProfilePairs = new Dictionary<string, string>();
		private Process process;

        readonly IProgress<AppCheckReportStruct> progressReporter;

		protected bool bRunning = false;

		public AppCheckThread(ref RacingDSX.Config.Config currentSettings, IProgress<AppCheckReportStruct> progressReporter, Process process = null)
		{

			settings = currentSettings;
			
            this.progressReporter = progressReporter;
			this.process = process;

            bRunning = false;
		}

		public void updateExecutables()
		{
			lock (this)
			{
                processProfilePairs.Clear();

                foreach (var profile in settings.Profiles)
				{
					if (!profile.Value.IsEnabled) { continue; }
					profile.Value.executableNames.ForEach((name) =>
					{
                        if (!processProfilePairs.ContainsKey(name))
                        {
                            processProfilePairs.Add(name, profile.Key);
                        }
                    });
				}
			//	settings = currentSettings;
			}
        }

		public void Run()
		{
            processProfilePairs.Clear();

            foreach (var profile in settings.Profiles)
            {
                if (!profile.Value.IsEnabled) { continue; }
                profile.Value.executableNames.ForEach((name) =>
                {
					if (!processProfilePairs.ContainsKey(name))
					{
						processProfilePairs.Add(name, profile.Key);
					}
                });
            }
            bRunning = true;
			try
			{
				if (progressReporter != null)
				{
					progressReporter.Report(new AppCheckReportStruct(0, "Connecting to Forza and DSX"));
				}

				//int forzaProcesses = 0;
				Process[] DSX, DSX_2, DSY;

				AppCheckReportStruct dsxReport = new AppCheckReportStruct(AppCheckReportStruct.AppType.DSX);
				AppCheckReportStruct forzaReport = new AppCheckReportStruct(AppCheckReportStruct.AppType.GAME);

				while (bRunning/*forzaProcesses == 0 || DSX.Length + DSX_2.Length == 0*/)
				{
					System.Threading.Thread.Sleep(1000);
					lock (this) { 
						if (!bRunning) { break; }

                        forzaReport.value = false;
                        forzaReport.message = "";
                        if (process != null)
						{
							if (!process.HasExited)
							{
								forzaReport.value = true;
								forzaReport.message = processProfilePairs[process.ProcessName];
                            }

						}
						else
						{
                            foreach (var processName in processProfilePairs.Keys.AsEnumerable())
                            {

                                if (Process.GetProcessesByName(processName).Length > 0)
                                {
                                    forzaReport.value = true;
                                    forzaReport.message = processProfilePairs[processName];
                                    break;
                                }
                            }
                        }

						// DSX = "DSX" or "DualSenseX"
						DSX = Process.GetProcessesByName("DSX");
						DSX_2 = Process.GetProcessesByName("DualsenseX");
                        DSY = Process.GetProcessesByName("DualSenseY");


                        dsxReport.value = (DSX.Length + DSX_2.Length + DSY.Length) > 0;
						//forzaReport.value = forzaProcesses > 0;

						//forzaReport.value = true;
						//TODO: CHANGE
						if (!bRunning) { break; }
						if (progressReporter != null)
						{
							progressReporter.Report(dsxReport);
							progressReporter.Report(forzaReport);
						}
					}
				}
			}
			catch (Exception e)
			{
				if (progressReporter != null)
				{
					progressReporter.Report(new AppCheckReportStruct(0, "Application encountered an exception: " + e.Message));
				}
			}
		}

		public void Stop()
		{
			bRunning = false;
		}
	}
}
