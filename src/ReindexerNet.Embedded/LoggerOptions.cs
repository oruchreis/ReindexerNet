using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    /// <summary>
    /// Configures log destinations and verbosity for the embedded Reindexer server.
    /// </summary>
    public class LoggerOptions
    {
        /// <summary>
        /// Gets or sets the server log file path. Use <c>none</c> to disable file logging.
        /// </summary>
        public string ServerLogFile { get; set; } = "none";
        /// <summary>
        /// Gets or sets the core log file path. Use <c>none</c> to disable file logging.
        /// </summary>
        public string CoreLogFile { get; set; } = "none";
        /// <summary>
        /// Gets or sets the HTTP log file path. Use <c>none</c> to disable file logging.
        /// </summary>
        public string HttpLogFile { get; set; } = "none";
        /// <summary>
        /// Gets or sets the RPC log file path. Use <c>none</c> to disable file logging.
        /// </summary>
        public string RpcLogFile { get; set; } = "none";

        /// <summary>
        /// Gets or sets the minimum log level emitted by the embedded server.
        /// </summary>
        public LogLevel Level { get; set; } = LogLevel.Info;
    }
}
