using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    public class MetricOptions
    {
        public bool EnableClientStats { get; set; } = false;
        public bool EnablePrometheus { get; set; } = false;
        public int CollectPeriod { get; set; } = 1000;
    }
}
