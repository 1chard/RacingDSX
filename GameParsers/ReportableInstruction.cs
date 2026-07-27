using System.Collections.Generic;
using RacingDualSense.DSX;

namespace RacingDualSense.GameParsers
{
    public class ReportableInstruction
    {
        public Instruction[] Instructions { get; set; }
        public List<RacingWorker.RacingReportStruct> RacingReportStructs { get; set; } = new List<RacingWorker.RacingReportStruct>();
    }
}
