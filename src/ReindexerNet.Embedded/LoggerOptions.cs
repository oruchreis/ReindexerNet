using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    public class LoggerOptions
    {
        public string ServerLogFile { get; set; } = "none";
        public string CoreLogFile { get; set; } = "none";
        public string HttpLogFile { get; set; } = "none";
        public string RpcLogFile { get; set; } = "none";

        public LogLevel Level { get; set; } = LogLevel.Info;
    }
}
