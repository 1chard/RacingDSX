using RacingDSX.GameParsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RacingDSX.Config
{
    public class ConfigHandler
    {
        //Methods to read config file
        //Method to initialize config file (Using default values), need to account for the different default profiles
        //Method to write config file
        //Singleton access to config data
        private static readonly String configFilePath = "RacingDSX.json";
        private static Config configData;

        private static void InitializeConfig()
        {
            configData ??= ReadConfigFromDisk();
            configData ??= new Config();
            configData = AddDefaultProfiles(configData);
            UpgradeConfig(configData);
            SaveConfig();
        }

        private static Config AddDefaultProfiles(Config config)
        {
            config.Profiles ??= new Dictionary<string, Profile>();
            if (!config.Profiles.ContainsKey("Forza"))
            {
                Profile profile = new Profile
                {
                    Name = "Forza",
                    gameUDPPort = 5300,
                    GameType = GameTypes.Forza,
                };
                profile.executableNames.AddRange(new string[] { "forzahorizon6", "ForzaHorizon5", "ForzaHorizon4", "ForzaMotorsport7", "forza_gaming.desktop.x64_release_final", "forza_steamworks_release_final" });
                config.Profiles.Add("Forza", profile);
            }
            if (!config.Profiles.ContainsKey("Dirt"))
            {
                Profile profile = new Profile
                {
                    Name = "Dirt",
                    gameUDPPort = 5300,
                    GameType = GameTypes.Dirt
                };
                profile.throttleSettings.GripLossValue = 0.4f;
                profile.executableNames.AddRange(new string[] { "drt", "dirtrally2" });
                config.Profiles.Add("Dirt", profile);
            }

            return config;
        }

        private static (byte major, byte minor, byte patch) ParseVersionString(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
            {
                return (0, 0, 0);
            }
            var versionParts = versionString.Split('.');
            if (versionParts.Length != 3)
            {
                return (0, 0, 0);
            }
            if (!byte.TryParse(versionParts[0], out byte major) ||
                !byte.TryParse(versionParts[1], out byte minor) ||
                !byte.TryParse(versionParts[2], out byte patch))
            {
                return (0, 0, 0);
            }
            return (major, minor, patch);
        }

        private static void UpgradeConfig(Config config)
        {
            var currentVersion = ParseVersionString(config.Version);

            if (currentVersion == (0, 0, 0)) // < 0.8.0
            {
                foreach (var profile in config.Profiles.Values)
                {
                    if (profile.Name == "Forza")
                    {
                        profile.executableNames.RemoveAll(name => string.Equals(name, "ForzaHorizon6", StringComparison.OrdinalIgnoreCase));
                        profile.executableNames.Insert(0, "forzahorizon6");
                        break;
                    }
                }
            }

            config.Version = Program.VERSION;
        }


        private static Config ReadConfigFromDisk()
        {

            try
            {
                if (!File.Exists(configFilePath))
                {
                    return null;
                }
                string jsonString = File.ReadAllText(configFilePath);

                Config config = JsonSerializer.Deserialize<Config>(jsonString);
                return config;
            }
            catch (Exception)
            {
                return null;
            }

        }
        private static void WriteConfigToDisk()
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(configData);
                File.WriteAllText(configFilePath, jsonString);
            }
            catch (Exception)
            {

            }
        }

        public static void SaveConfig()
        {
            WriteConfigToDisk();

        }

        public static Config GetConfig()
        {
            if (configData == null)
            {
                InitializeConfig();
            }
            return configData;
        }
    }
}
