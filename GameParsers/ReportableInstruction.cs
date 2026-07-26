using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacingDSX.GameParsers
{
    public class ReportableInstruction
    {
        public Instruction[] Instructions { get; set; }
        public List<RacingWorker.RacingReportStruct> RacingReportStructs { get; set; } = new List<RacingWorker.RacingReportStruct>();
    }
}
