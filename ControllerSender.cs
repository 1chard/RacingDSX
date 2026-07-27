using DualSenseSharp;
using DualSenseSharp.Components;
using DualSenseSharp.Components.Triggers.DSX;
using RacingDSX.Config;
using RacingDSX.DSX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static RacingDSX.RacingWorker;

namespace RacingDSX
{
    public class ControllerSender : IDataSender
    {
        private static byte Normalize(object input)
        {
            return (byte)Math.Clamp(Convert.ToInt32(input), 0, 255);
        }

        private readonly RacingWorker racingWorker;
        private readonly Func<DualSense> controllerSupplier;

        public ControllerSender(RacingWorker racingWorker, Func<DualSense> controllerSupplier)
        {
            this.racingWorker = racingWorker;
            this.controllerSupplier = controllerSupplier;
        }


        public void Send(Packet data)
        {
            lock (this)
            {
                var settings = racingWorker.settings;
                var progressReporter = racingWorker.progressReporter;
                var dualSense = controllerSupplier();

                if (dualSense == null)
                {
                    if (settings.VerboseLevel > VerboseLevel.Limited && progressReporter != null)
                    {
                        progressReporter.Report(new RacingReportStruct($"No Controller Available"));
                    }
                    return;
                }

                if (settings.VerboseLevel > VerboseLevel.Limited && progressReporter != null)
                {
                    progressReporter.Report(new RacingReportStruct($"Converting Message to Controller Output"));
                }

                try
                {
                    foreach (var instruction in data.Instructions)
                    {
                        switch (instruction.Type)
                        {
                            case InstructionType.TriggerUpdate:
                                {
                                    var triggerType = (Trigger)instruction.Parameters[1];
                                    var trigger = triggerType == Trigger.Right ? dualSense.RightTrigger : dualSense.LeftTrigger;
                                    var effectType = (DSX.TriggerMode)instruction.Parameters[2];

                                    AdaptiveTrigger.TriggerMode mode;
                                    switch (effectType)
                                    {
                                        case DSX.TriggerMode.Normal:
                                            mode = new Normal();
                                            break;
                                        case DSX.TriggerMode.Resistance:
                                            var start = Normalize(instruction.Parameters[3]);
                                            var force = Normalize(instruction.Parameters[4]);
                                            if (force > 8 || start > 9)
                                                mode = new Normal();
                                            else
                                                mode = new Resistance(force, start);
                                            break;
                                        case DSX.TriggerMode.CustomTriggerValue:
                                            mode = new CustomTriggerValue((CustomTriggerValue.CustomTriggerValueMode)Normalize(instruction.Parameters[3]),
                                                [.. instruction.Parameters[4..].Select(Normalize)]);
                                            break;
                                        default:
                                            mode = new Normal();
                                            progressReporter.Report(new RacingReportStruct($"Unknown Trigger Update Type: " + effectType));
                                            break;
                                    }

                                    trigger.Mode = mode;
                                }
                                break;
                            case InstructionType.RGBUpdate:
                                dualSense.LightBar.Red = Normalize(instruction.Parameters[1]);
                                dualSense.LightBar.Green = Normalize(instruction.Parameters[2]);
                                dualSense.LightBar.Blue = Normalize(instruction.Parameters[3]);
                                break;
                            default:
                                progressReporter.Report(new RacingReportStruct($"Unknown Instruction Type: " + instruction.Type));
                                break;
                        }
                    }

                    if (settings.VerboseLevel > VerboseLevel.Limited && progressReporter != null)
                    {
                        progressReporter.Report(new RacingReportStruct($"Sending Message to HID..."));
                    }


                    var swOutput = Stopwatch.StartNew();
                    var task = dualSense.UpdateOutputAsync().AsTask();
                    if (task.Result)
                    {
                        if (settings.VerboseLevel > VerboseLevel.Limited && progressReporter != null)
                        {
                            progressReporter.Report(new RacingReportStruct($"Message sent to HID"));
                        }
                    }
                    else
                    {
                        if (settings.VerboseLevel > VerboseLevel.Limited && progressReporter != null)
                        {
                            progressReporter.Report(new RacingReportStruct($"Failed to send output to controller"));
                        }
                    }
                }
                catch (Exception e)
                {
                    if (progressReporter != null)
                    {
                        progressReporter.Report(new RacingReportStruct("Error Sending Message: " + e.Message));
                    }
                }
            }
        }

        public void Connect()
        {
        }

        public void Stop()
        {
        }
    }
}