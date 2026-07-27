using DualSenseSharp;
using RacingDualSense.Config;
using RacingDualSense.DSX;
using RacingDualSense.GameParsers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace RacingDualSense
{
    public interface IDataSender
    {
        void Send(Packet data);
        void Connect();
        void Stop();
    }

    public enum InstructionTriggerMode : sbyte
    {
        NONE,
        RESISTANCE,
        VIBRATION
    }

    public sealed class RacingWorker
    {
        public struct RacingReportStruct
        {
            public enum ReportType : ushort
            {
                VERBOSEMESSAGE = 0,
                NORACE = 1,
                RACING = 2,
                HEARTBEAT = 3,
            }

            public enum RacingReportType : ushort
            {
                // 0 = Throttle vibration message
                THROTTLE_VIBRATION = 0,
                // 1 = Throttle message
                THROTTLE,
                // 2 = Brake vibration message
                BRAKE_VIBRATION,
                // 3 = Brake message
                BRAKE
            }
            public RacingReportStruct(VerboseLevel level, ReportType type, RacingReportType racingType, string msg)
            {
                this.verboseLevel = level;
                this.type = type;
                this.racingType = racingType;
                this.message = msg;
            }

            public RacingReportStruct(ReportType type, RacingReportType racingType, string msg)
            {
                this.type = type;
                this.racingType = racingType;
                this.message = msg;
            }

            public RacingReportStruct(VerboseLevel level, ReportType type, string msg)
            {
                this.verboseLevel = level;
                this.type = type;
                this.message = msg;
            }

            public RacingReportStruct(ReportType type, string msg)
            {
                this.type = type;
                this.message = msg;
            }

            public RacingReportStruct(VerboseLevel level, string msg)
            {
                this.verboseLevel = level;
                this.type = ReportType.VERBOSEMESSAGE;
                this.message = msg;
            }

            public RacingReportStruct(string msg)
            {
                this.type = ReportType.VERBOSEMESSAGE;
                this.message = msg;
            }

            public ReportType type = 0;
            public RacingReportType racingType = 0;
            public string message = string.Empty;
            public VerboseLevel verboseLevel = VerboseLevel.Limited;
        }

        internal Config.Config settings;
        internal IProgress<RacingReportStruct> progressReporter;
        private Parser parser;

        private Func<DualSense> supplierController;
        private IDataSender dataSender;

        // JSON serialization options


        public RacingWorker(Config.Config currentSettings, IProgress<RacingReportStruct> progressReporter, Func<DualSense> getController)
        {
            settings = currentSettings;
            this.progressReporter = progressReporter;
            supplierController = getController;
        }

        public void SetSettings(Config.Config currentSettings)
        {
            lock (this)
            {
                settings = currentSettings;
            }
        }

        void SendData()
        {
            CsvData csvRecord = new CsvData();
            Profile activeProfile = settings.ActiveProfile;
            List<Instruction> instructionsList = new List<Instruction>();
            ReportableInstruction reportableInstruction;

            // No race = normal triggers
            if (!parser.IsRaceOn())
            {
                reportableInstruction = parser.GetPreRaceInstructions();
                reportableInstruction.RacingReportStructs.ForEach(x =>
                {
                    if (x.verboseLevel <= settings.VerboseLevel && progressReporter != null)
                    {
                        progressReporter.Report(x);
                    }
                });

                //Send the commands to DSX
                instructionsList.AddRange(reportableInstruction.Instructions);
            }
            else
            {
                reportableInstruction = parser.GetInRaceRightTriggerInstruction();
                instructionsList.AddRange(reportableInstruction.Instructions);
                reportableInstruction = parser.GetInRaceLeftTriggerInstruction();
                instructionsList.AddRange(reportableInstruction.Instructions);
                reportableInstruction = parser.GetInRaceLightbarInstruction();
                instructionsList.AddRange(reportableInstruction.Instructions);
            }

            Packet p = new Packet
            {
                Instructions = [.. instructionsList]
            };
            dataSender.Send(p);
        }

        static IPEndPoint ipEndPoint = null;
        static UdpClient client = null;

        public struct UdpState
        {
            public UdpClient u;
            public IPEndPoint e;

            public UdpState(UdpClient u, IPEndPoint e)
            {
                this.u = u;
                this.e = e;
            }
        }

        private bool bRunning = false;

        public void Run()
        {
            bRunning = true;
            dataSender = (settings.DSXPort != null) ? new DsxSender(this) : new ControllerSender(this, supplierController);

            try
            {
                dataSender.Connect();
                if (settings.ActiveProfile == null)
                {
                    if (progressReporter != null)
                    {
                        progressReporter.Report(new RacingReportStruct("No active profile selected. Exiting..."));
                    }
                    return;
                }

                switch (settings.ActiveProfile.GameType)
                {
                    case GameTypes.Forza:
                        parser = new ForzaParser(settings);
                        break;
                    case GameTypes.Dirt:
                        parser = new DirtParser(settings);
                        break;
                    default:
                        parser = new NullParser(settings);
                        break;
                }

                ipEndPoint = new IPEndPoint(IPAddress.Loopback, settings.ActiveProfile.gameUDPPort);
                client = new UdpClient(settings.ActiveProfile.gameUDPPort);

                byte[] resultBuffer;
                while (bRunning)
                {
                    resultBuffer = client.Receive(ref ipEndPoint);
                    if (resultBuffer == null)
                        continue;

                    progressReporter.Report(new RacingReportStruct(VerboseLevel.Off, RacingReportStruct.ReportType.HEARTBEAT, 0, ""));

                    if (settings.VerboseLevel > VerboseLevel.Limited && progressReporter != null)
                    {
                        progressReporter.Report(new RacingReportStruct("received Message from Forza!"));
                    }

                    if (!AdjustToBufferType(resultBuffer.Length))
                    {
                        continue;
                    }

                    parser.ParsePacket(resultBuffer);
                    if (settings.VerboseLevel > VerboseLevel.Limited && progressReporter != null)
                    {
                        progressReporter.Report(new RacingReportStruct("Data Parsed"));
                    }

                    SendData();
                }
            }
            catch (Exception e)
            {
                if (progressReporter != null)
                {
                    progressReporter.Report(new RacingReportStruct("Application encountered an exception: " + e.Message));
                }
            }
            finally
            {
                Stop();
            }
        }

        public void Stop()
        {
            bRunning = false;

            if (settings.VerboseLevel > VerboseLevel.Off && progressReporter != null)
            {
                progressReporter.Report(new RacingReportStruct($"Cleaning Up"));
            }

            if (client != null)
            {
                client.Close();
                client.Dispose();
            }

            if(dataSender != null)
            {
                dataSender.Stop();
            }

            if (settings.VerboseLevel > VerboseLevel.Off)
            {
                progressReporter.Report(new RacingReportStruct($"Cleanup Finished. Exiting..."));
            }
        }

        static bool AdjustToBufferType(int bufferLength)
        {
            switch (bufferLength)
            {
                case 232: // FM7 sled
                    return false;
                case 264: // Dirt Rally 1
                    FMData.BufferOffset = 0;
                    return true;
                case 311: // FM7 dash
                    FMData.BufferOffset = 0;
                    return true;
                case 331: // FM8 dash
                    FMData.BufferOffset = 0;
                    return true;
                case 324: // FH4
                    FMData.BufferOffset = 12;
                    return true;
                default:
                    return false;
            }
        }
    }
}
