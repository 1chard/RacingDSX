using RacingDualSense.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RacingDualSense
{

    public class Program
    {
        public const string VERSION = "0.8.0";

        static void Main(string[] args)
        {
            if (Path.GetFileName(Application.ExecutablePath).Contains("headless", StringComparison.CurrentCultureIgnoreCase) && args.Length == 0)
            {
                args = ["--attach", "--nogui"];
            }

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
                            Console.WriteLine("RacingDualSense Version: " + VERSION);
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
                            var processAndProfile = FindGameExists(config);
                            if (processAndProfile == null)
                            {
                                Console.WriteLine("Error: Could not find a process to attach to.");
                                return;
                            }

                            process = processAndProfile.Value.Process;
                            profile = processAndProfile.Value.Profile;
                            break;
                        }
                    case "--run":
                        {
                            i++;

                            if (i >= args.Length)
                            {
                                Console.WriteLine("Error: --run requires an argument");
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

        private static (Process Process, Profile Profile)? FindGameExists(Config.Config config)
        {
            for (var i = 0; i < 10; i++)
            {
                if (i != 0)
                    System.Threading.Thread.Sleep(1000);
                foreach (var profile in config.Profiles)
                {
                    foreach (var processName in profile.Value.executableNames)
                    {
                        var processes = Process.GetProcessesByName(processName);
                        if (processes.Length > 0)
                        {
                            return (processes.First(), profile.Value);
                        }
                    }
                }
            }

            return null;
        }
    }



}