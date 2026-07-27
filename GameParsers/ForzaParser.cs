using RacingDualSense.Config;
using RacingDualSense.DSX;
using System;
using static RacingDualSense.RacingWorker;

namespace RacingDualSense.GameParsers
{
    internal class ForzaParser : Parser
    {
        // Colors for Light Bar while in menus -> using car's PI colors from Forza
        public const uint CarClassD = 0;
        public const uint CarClassC = 1;
        public const uint CarClassB = 2;
        public const uint CarClassA = 3;
        public const uint CarClassS1 = 4;
        public const uint CarClassS2 = 5;
        public const uint CarClassR = 6; // forza horizon 6's class between S2 and X
        public const uint CarClassX = 7; // fh 4-5's X is 6, fh 6 is 7

        public static readonly int[] ColorClassD = { 20, 130, 255 };
        public static readonly int[] ColorClassC = { 240, 150, 0 };
        public static readonly int[] ColorClassB = { 255, 40, 5 };
        public static readonly int[] ColorClassA = { 240, 10, 4 };
        public static readonly int[] ColorClassS1 = { 90, 0, 255 };
        public static readonly int[] ColorClassS2 = { 0, 8, 255 };
        public static readonly int[] ColorClassR = { 220, 0, 70 };
        public static readonly int[] ColorClassX = { 30, 255, 0 };
        public static readonly int[] ColorClassOther = { 255, 138, 138 };
        public ForzaParser(Config.Config settings) : base(settings)
        {
        }

        public override bool IsRaceOn()
        {
            bool bInRace = data.IsRaceOn;
            float currentRPM = data.CurrentEngineRpm;

            if (currentRPM == LastEngineRPM
            && data.Power <= 0)
            {
                LastRPMAccumulator++;
                if (LastRPMAccumulator > RPMAccumulatorTriggerRaceOff)
                {
                    bInRace = false;
                }
            }
            else
            {
                LastRPMAccumulator = 0;
            }

            LastEngineRPM = currentRPM;
            return bInRace;
        }

        public override ReportableInstruction GetPreRaceInstructions()
        {
            ReportableInstruction reportableInstruction = new ReportableInstruction();

            RightTrigger.Parameters = [controllerIndex, Trigger.Right, DSX.TriggerMode.Normal, 0, 0];
            LeftTrigger.Parameters = [controllerIndex, Trigger.Left, DSX.TriggerMode.Normal, 0, 0];

            #region Light Bar color
            int[] RGBarray;
            if(LastValidCarCPI < 100)
            {
                RGBarray = ColorClassOther;
            } else
            {
                RGBarray = LastValidCarClass switch
                {
                    CarClassD => ColorClassD,
                    CarClassC => ColorClassC,
                    CarClassB => ColorClassB,
                    CarClassA => ColorClassA,
                    CarClassS1 => ColorClassS1,
                    CarClassS2 => ColorClassS2,
                    CarClassR => LastValidCarCPI == 999 ? ColorClassX : ColorClassR, // fh6: R => 901-998, fh5: X => 999
                    CarClassX => ColorClassX,
                    _ => ColorClassOther
                };
            }

            LightBar.Parameters = [controllerIndex, RGBarray[0], RGBarray[1], RGBarray[2]];
            #endregion

            reportableInstruction.RacingReportStructs.Add(new RacingReportStruct(VerboseLevel.Limited, RacingReportStruct.ReportType.NORACE, $"No race going on. Normal Triggers. Car's Class = {LastValidCarClass}; CPI = {LastValidCarCPI}; Color [{RGBarray[0]}, {RGBarray[1]}, {RGBarray[2]}]"));

            reportableInstruction.Instructions = [LightBar, LeftTrigger, RightTrigger];
            return reportableInstruction;
        }


        public override void ParsePacket(byte[] packet)
        {
            data = new DataPacket
            {
                // sled
                IsRaceOn = packet.IsRaceOn(),
                EngineMaxRpm = packet.EngineMaxRpm(),
                EngineIdleRpm = packet.EngineIdleRpm(),
                CurrentEngineRpm = packet.CurrentEngineRpm(),
                AccelerationX = packet.AccelerationX(),
                AccelerationZ = packet.AccelerationZ(),

                TireCombinedSlipFrontLeft = packet.TireCombinedSlipFl(),
                TireCombinedSlipFrontRight = packet.TireCombinedSlipFr(),
                TireCombinedSlipRearLeft = packet.TireCombinedSlipRl(),
                TireCombinedSlipRearRight = packet.TireCombinedSlipRr(),

                CarClass = packet.CarClass(),
                CarPerformanceIndex = packet.CarPerformanceIndex(),

                Speed = packet.Speed(),
                Power = packet.Power(),

                Accelerator = packet.Accelerator(),
                Brake = packet.Brake()
            };

            if (data.CarPerformanceIndex > 0)
            {
                LastValidCarClass = data.CarClass;
                LastValidCarCPI = data.CarPerformanceIndex;
            }


            data.FourWheelCombinedTireSlip = (Math.Abs(data.TireCombinedSlipFrontLeft) + Math.Abs(data.TireCombinedSlipFrontRight) + Math.Abs(data.TireCombinedSlipRearLeft) + Math.Abs(data.TireCombinedSlipRearRight)) / 4;
            data.FrontWheelsCombinedTireSlip = (Math.Abs(data.TireCombinedSlipFrontLeft) + Math.Abs(data.TireCombinedSlipFrontRight)) / 2;
            data.RearWheelsCombinedTireSlip = (Math.Abs(data.TireCombinedSlipRearLeft) + Math.Abs(data.TireCombinedSlipRearRight)) / 2;


            /* data.TimestampMS = packet.TimestampMs();
             data.AccelerationY = packet.AccelerationY();

             data.SuspensionTravelMetersFrontLeft = packet.SuspensionTravelMetersFl();
             data.SuspensionTravelMetersFrontRight = packet.SuspensionTravelMetersFr();
             data.SuspensionTravelMetersRearLeft = packet.SuspensionTravelMetersRl();
             data.SuspensionTravelMetersRearRight = packet.SuspensionTravelMetersRr();
             data.CarOrdinal = packet.CarOrdinal();
             data.DrivetrainType = packet.DriveTrain();
             data.NumCylinders = packet.NumCylinders();

             // dash
             data.PositionX = packet.PositionX();
             data.PositionY = packet.PositionY();
             data.PositionZ = packet.PositionZ();
             data.Torque = packet.Torque();
             data.TireTempFl = packet.TireTempFl();
             data.TireTempFr = packet.TireTempFr();
             data.TireTempRl = packet.TireTempRl();
             data.TireTempRr = packet.TireTempRr();
             data.Boost = packet.Boost();
             data.Fuel = packet.Fuel();
             data.Distance = packet.Distance();
             data.BestLapTime = packet.BestLapTime();
             data.LastLapTime = packet.LastLapTime();
             data.CurrentLapTime = packet.CurrentLapTime();
             data.CurrentRaceTime = packet.CurrentRaceTime();
             data.Lap = packet.Lap();
             data.RacePosition = packet.RacePosition();
             data.Clutch = packet.Clutch();
             data.Handbrake = packet.Handbrake();
             data.Gear = packet.Gear();
             data.Steer = packet.Steer();
             data.NormalDrivingLine = packet.NormalDrivingLine();
             data.NormalAiBrakeDifference = packet.NormalAiBrakeDifference();
             data.VelocityX = packet.VelocityX();
             data.VelocityY = packet.VelocityY();
             data.VelocityZ = packet.VelocityZ();
             data.AngularVelocityX = packet.AngularVelocityX();
             data.AngularVelocityY = packet.AngularVelocityY();
             data.AngularVelocityZ = packet.AngularVelocityZ();
             data.Yaw = packet.Yaw();
             data.Pitch = packet.Pitch();
             data.Roll = packet.Roll();
             data.NormalizedSuspensionTravelFrontLeft = packet.NormSuspensionTravelFl();
             data.NormalizedSuspensionTravelFrontRight = packet.NormSuspensionTravelFr();
             data.NormalizedSuspensionTravelRearLeft = packet.NormSuspensionTravelRl();
             data.NormalizedSuspensionTravelRearRight = packet.NormSuspensionTravelRr();
             data.TireSlipRatioFrontLeft = packet.TireSlipRatioFl();
             data.TireSlipRatioFrontRight = packet.TireSlipRatioFr();
             data.TireSlipRatioRearLeft = packet.TireSlipRatioRl();
             data.TireSlipRatioRearRight = packet.TireSlipRatioRr();
             data.WheelRotationSpeedFrontLeft = packet.WheelRotationSpeedFl();
             data.WheelRotationSpeedFrontRight = packet.WheelRotationSpeedFr();
             data.WheelRotationSpeedRearLeft = packet.WheelRotationSpeedRl();
             data.WheelRotationSpeedRearRight = packet.WheelRotationSpeedRr();
             data.WheelOnRumbleStripFrontLeft = packet.WheelOnRumbleStripFl();
             data.WheelOnRumbleStripFrontRight = packet.WheelOnRumbleStripFr();
             data.WheelOnRumbleStripRearLeft = packet.WheelOnRumbleStripRl();
             data.WheelOnRumbleStripRearRight = packet.WheelOnRumbleStripRr();
             data.WheelInPuddleDepthFrontLeft = packet.WheelInPuddleFl();
             data.WheelInPuddleDepthFrontRight = packet.WheelInPuddleFr();
             data.WheelInPuddleDepthRearLeft = packet.WheelInPuddleRl();
             data.WheelInPuddleDepthRearRight = packet.WheelInPuddleRr();
             data.SurfaceRumbleFrontLeft = packet.SurfaceRumbleFl();
             data.SurfaceRumbleFrontRight = packet.SurfaceRumbleFr();
             data.SurfaceRumbleRearLeft = packet.SurfaceRumbleRl();
             data.SurfaceRumbleRearRight = packet.SurfaceRumbleRr();
             data.TireSlipAngleFrontLeft = packet.TireSlipAngleFl();
             data.TireSlipAngleFrontRight = packet.TireSlipAngleFr();
             data.TireSlipAngleRearLeft = packet.TireSlipAngleRl();
             data.TireSlipAngleRearRight = packet.TireSlipAngleRr();
             return data;
             throw new NotImplementedException();*/
        }
    }
}
