namespace ReindexerNet.Embedded;

/// <summary>
/// Defines how embedded async operations behave when the native operation queue is full.
/// </summary>
public enum EmbeddedNativeQueueFullMode
{
    /// <summary>
    /// Wait asynchronously until queue capacity becomes available.
    /// </summary>
    Wait = 0,

    /// <summary>
    /// Fail immediately with an exception when queue capacity is exhausted.
    /// </summary>
    Throw = 1
}
