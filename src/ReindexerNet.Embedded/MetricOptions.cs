using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    /// <summary>
    /// Configures metrics collection for the embedded Reindexer server.
    /// </summary>
    public class MetricOptions
    {
        /// <summary>
        /// Enables per-client statistics collection.
        /// </summary>
        public bool EnableClientStats { get; set; } = false;
        /// <summary>
        /// Enables Prometheus-compatible metrics output.
        /// </summary>
        public bool EnablePrometheus { get; set; } = false;
        /// <summary>
        /// Gets or sets the metrics collection period in milliseconds.
        /// </summary>
        public int CollectPeriod { get; set; } = 1000;
    }
}
