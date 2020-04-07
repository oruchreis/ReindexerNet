namespace ReindexerNet.Embedded
{
    /// <summary>
    /// Log writer delegate to collect internal logs of Reindexer.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="msg"></param>
    public delegate void LogWriterAction(LogLevel level, string msg);

    /// <summary>
    /// Reindexer log levels.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Error
        /// </summary>
        Error,
        /// <summary>
        /// Warning
        /// </summary>
        Warning,
        /// <summary>
        /// Info
        /// </summary>
        Info,
        /// <summary>
        /// Trace
        /// </summary>
        Trace
    }
}
