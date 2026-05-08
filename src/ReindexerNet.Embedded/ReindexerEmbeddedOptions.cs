using System;

namespace ReindexerNet.Embedded;

/// <summary>
/// Configures embedded Reindexer client behavior.
/// </summary>
public sealed class ReindexerEmbeddedOptions
{
    /// <summary>
    /// Gets or sets the maximum number of native embedded operations that may run concurrently.
    /// </summary>
    public int MaxNativeConcurrency { get; set; } = Math.Max(1, Environment.ProcessorCount);

    /// <summary>
    /// Gets or sets the maximum number of native operations that may wait in the async queue.
    /// A value less than or equal to zero disables the queue limit.
    /// </summary>
    public int NativeQueueCapacity { get; set; } = 1024;

    /// <summary>
    /// Gets or sets the behavior used when the async native operation queue is full.
    /// </summary>
    public EmbeddedNativeQueueFullMode NativeQueueFullMode { get; set; } = EmbeddedNativeQueueFullMode.Wait;
}
