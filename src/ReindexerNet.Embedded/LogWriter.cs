using System;
using System.Collections.Generic;
using System.Text;

namespace ReindexerNet.Embedded
{
    public delegate void LogWriterAction(LogLevel level, string msg);
    public enum LogLevel
    {
        None,
        Error,
        Warning,
        Info,
        Trace
    }
}
