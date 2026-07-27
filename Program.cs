using RacingDSX.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RacingDSX
{

    public class Program
    {
        public const string VERSION = "0.8.0";

        static void Main(string[] args)
        {
            bool isGuiMode = true;
            Process process = null;

            var configAndProfile = LoadSettings();
            var config = configAndProfile.Key;
            var profile = configAndProfile.Value;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg)
                {
                    case "-v":
                        {
                            Console.WriteLine("RacingDSX Version: " + VERSION);
                            return;
                        }
                    case "--nogui":
                    case "--headless":
                        {
                            isGuiMode = false;
                            break;
                        }
                    case "--attach":
                        {
                            process = FindGameExists(profile.executableNames);
                            if (process == null)
                            {
                                Console.WriteLine("Error: Could not find a process to attach to.");
                                return;
                            }
                            break;
                        }
                    case "--exe-attach":
                        {
                            i++;

                            if (i >= args.Length)
                            {
                                Console.WriteLine("Error: --exe-attach requires an argument");
                                return;
                            }

                            process = Process.Start(new ProcessStartInfo
                            {
                                FileName = args[i],
                                Arguments = "",
                                WorkingDirectory = Path.GetDirectoryName(args[i]),
                                UseShellExecute = true
                            });

                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Unknown argument: " + arg);
                            return;
                        }
                }
            }

            var core = new Core(process, config, profile);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext(core, isGuiMode));

            core.close();
        }

        private static KeyValuePair<Config.Config, Profile> LoadSettings()
        {
            // Get values from the config given their key and their target type.
            var currentSettings = ConfigHandler.GetConfig();
            var selectedProfile = currentSettings.Profiles.Values.First();

            if (currentSettings.DisableAppCheck && currentSettings.DefaultProfile != null)
            {
                if (currentSettings.Profiles.ContainsKey(currentSettings.DefaultProfile))
                {
                    currentSettings.ActiveProfile = currentSettings.Profiles[currentSettings.DefaultProfile];
                }
            }

            return KeyValuePair.Create(currentSettings, selectedProfile);
        }

        private static Process FindGameExists(List<string> executableNames)
        {
            for (var i = 0; i < 10; i++)
            {
                if (i != 0)
                    System.Threading.Thread.Sleep(1000);

                foreach (var processName in executableNames)
                {
                    var processes = Process.GetProcessesByName(processName);
                    if (processes.Length > 0)
                    {
                        return processes.First();
                    }
                }
            }

            return null;
        }
    }



}