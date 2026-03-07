using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows.Forms;

namespace RacingDSX
{

    public class Program
    {
        public const String VERSION = "0.7.0";
        
        static void Main(string[] args)
        {
            bool isGuiMode = true;
            bool followGame = false;
            Process process = null;

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
                    case "--cli":
                        {
                            isGuiMode = false;
                            break;
                        }
                    case "--attach":
                        {
                            followGame = true;
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

                            Console.WriteLine("Starting process: " + args[i]);

                            process = Process.Start(new ProcessStartInfo
                            {
                                FileName = args[i],
                                Arguments = "",
                                WorkingDirectory = Path.GetDirectoryName(args[i]),
                                UseShellExecute = true
                            });
                            Console.WriteLine($"Jogo iniciado: {process.ProcessName} ({process.Id})");

                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Unknown argument: " + arg);
                            return;
                        }
                }
            }

            var core = new Core(process);

            if (followGame)
            {
                Console.WriteLine("Trying to attach");
                if (!TestGameExists(core.selectedProfile.executableNames))
                {
                    Console.WriteLine("Failed to attach in 10 seconds");
                    return;
                } else
                {
                    core.bForzaOpenedOnceAttached = true;
                    Console.WriteLine("Attached successfully");
                }
            }

            if (isGuiMode)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new UI(core));
            }
            else
            {
                new CLI(core);
            }
        }

        public static bool TestGameExists(List<string> executableNames)
        {
            for (var i = 0; i < 10; i++)
            {
                if(i != 0)
                    System.Threading.Thread.Sleep(1000);

                foreach (var processName in executableNames)
                {

                    if (Process.GetProcessesByName(processName).Length > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

   

    public class ParametersConverter : JsonConverter<object[]>
    {
        public override object[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected start of array");
            }

            var parameters = new List<object>();
            reader.Read(); // Move past StartArray

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.Number:
                        if (reader.TryGetInt32(out int intValue))
                            parameters.Add(intValue);
                        else if (reader.TryGetDouble(out double doubleValue))
                            parameters.Add(doubleValue);
                        break;
                    case JsonTokenType.String:
                        string stringValue = reader.GetString();
                        // Convert enum strings to their numeric values
                        if (Enum.TryParse<Trigger>(stringValue, out var trigger))
                            parameters.Add((int)trigger);
                        else if (Enum.TryParse<TriggerMode>(stringValue, out var triggerMode))
                            parameters.Add((int)triggerMode);
                        else if (Enum.TryParse<CustomTriggerValueMode>(stringValue, out var customTrigger))
                            parameters.Add((int)customTrigger);
                        else if (Enum.TryParse<PlayerLEDNewRevision>(stringValue, out var playerLed))
                            parameters.Add((int)playerLed);
                        else if (Enum.TryParse<MicLEDMode>(stringValue, out var micLed))
                            parameters.Add((int)micLed);
                        else
                            parameters.Add(stringValue);
                        break;
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        parameters.Add(reader.GetBoolean());
                        break;
                    case JsonTokenType.Null:
                        parameters.Add(null);
                        break;
                }
                reader.Read();
            }

            return parameters.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, object[] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                if (item == null)
                {
                    writer.WriteNullValue();
                    continue;
                }

                switch (item)
                {
                    case int intValue:
                        writer.WriteNumberValue(intValue);
                        break;
                    case double doubleValue:
                        writer.WriteNumberValue(doubleValue);
                        break;
                    case string stringValue:
                        writer.WriteStringValue(stringValue);
                        break;
                    case bool boolValue:
                        writer.WriteBooleanValue(boolValue);
                        break;
                    case Enum enumValue:
                        writer.WriteNumberValue(Convert.ToInt32(enumValue));
                        break;
                    default:
                        writer.WriteNumberValue(Convert.ToInt32(item));
                        break;
                }
            }
            writer.WriteEndArray();
        }
    }

    public static class Triggers
    {
        public static IPAddress localhost = new IPAddress(new byte[] { 127, 0, 0, 1 });

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            Converters = 
            { 
                new ParametersConverter()
            }
        };

        public static string PacketToJson(Packet packet)
        {
            try
            {
                return JsonSerializer.Serialize(packet, jsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to serialize packet to JSON", ex);
            }
        }

        public static Packet JsonToPacket(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<Packet>(json, jsonOptions) 
                    ?? throw new InvalidOperationException("Deserialized packet is null");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to deserialize JSON to packet", ex);
            }
        }
    }

    public enum TriggerMode
    {
        Normal = 0,
        GameCube = 1,
        VerySoft = 2,
        Soft = 3,
        Hard = 4,
        VeryHard = 5,
        Hardest = 6,
        Rigid = 7,
        VibrateTrigger = 8,
        Choppy = 9,
        Medium = 10,
        VibrateTriggerPulse = 11,
        CustomTriggerValue = 12,
        Resistance = 13,
        Bow = 14,
        Galloping = 15,
        SemiAutomaticGun = 16,
        AutomaticGun = 17,
        Machine = 18,
        OFF = 19,
        FEEDBACK = 20,
        WEAPON = 21,
        VIBRATION = 22,
        SLOPE_FEEDBACK = 23,
        MULTIPLE_POSITION_FEEDBACK = 24,
        MULTIPLE_POSITION_VIBRATION = 25,
        VIBRATE_TRIGGER_10Hz = 26
    }

    public enum CustomTriggerValueMode
    {
        OFF = 0,
        Rigid = 1,
        RigidA = 2,
        RigidB = 3,
        RigidAB = 4,
        Pulse = 5,
        PulseA = 6,
        PulseB = 7,
        PulseAB = 8,
        VibrateResistance = 9,
        VibrateResistanceA = 10,
        VibrateResistanceB = 11,
        VibrateResistanceAB = 12,
        VibratePulse = 13,
        VibratePulseA = 14,
        VibratePulsB = 15,
        VibratePulseAB = 16
    }

    public enum Trigger
    {
        Invalid,
        Left,
        Right
    }

    public enum InstructionType
    {
        Invalid,
        TriggerUpdate,
        RGBUpdate,
        PlayerLED,
        PlayerLEDNewRevision,
        MicLED,
        TriggerThreshold,
        ResetToUserSettings,
        GetDSXStatus
    }

    public enum PlayerLEDNewRevision
    {
        One,
        Two,
        Three,
        Four,
        Five
    }

    public enum MicLEDMode
    {
        Off,
        On,
        Pulse
    }

    public class Instruction
    {
        [JsonConstructor]
        public Instruction(InstructionType type)
        {
            Type = type;
        }

        [JsonPropertyName("type")]
        public InstructionType Type { get; set; }

        [JsonPropertyName("parameters")]
        [JsonConverter(typeof(ParametersConverter))]
        public object[] Parameters { get; set; }
    }

    public class Packet
    {
        [JsonPropertyName("instructions")]
        public Instruction[] Instructions { get; set; }

        public Packet()
        {
            Instructions = Array.Empty<Instruction>();
        }
    }
}