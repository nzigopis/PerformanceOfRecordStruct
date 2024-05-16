using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceOfRecordStruct
{
    public class RunAggregates
    {
        public string InstrumentId { get; set; } = default!;
        public double NVATimeInMilliseconds { get; set; }
        public double ActiveTimeInMilliseconds { get; set; }
        public int CompletedSuccessfullyCount { get; set; }
        public int CompletedWithWarningsCount { get; set; }
        public int CompletedWithErrorsCount { get; set; }
    }
}
