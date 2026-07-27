using System.Collections.Generic;
using RacingDSX.DSX;

namespace RacingDSX.GameParsers
{
    public class ReportableInstruction
    {
        public Instruction[] Instructions { get; set; }
        public List<RacingWorker.RacingReportStruct> RacingReportStructs { get; set; } = new List<RacingWorker.RacingReportStruct>();
    }
}
