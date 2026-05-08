using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    /// <summary>
    /// Configures network listeners and request limits for the embedded Reindexer server.
    /// </summary>
    public class NetworkOptions
    {
        /// <summary>
        /// Enables authentication and authorization checks for network endpoints.
        /// </summary>
        public bool EnableSecurity { get; set; } = false;
        /// <summary>
        /// Gets or sets the threading mode used by the RPC endpoint.
        /// </summary>
        public ThreadingOptions RpcThreading { get; set; } = ThreadingOptions.Shared;
        /// <summary>
        /// Gets or sets the threading mode used by the HTTP endpoint.
        /// </summary>
        public ThreadingOptions HttpThreading { get; set; } = ThreadingOptions.Shared;
        /// <summary>
        /// Gets or sets the maximum update request size in bytes.
        /// </summary>
        public int MaxUpdatesSize { get; set; } = 1024 * 1024 * 1024;
        /// <summary>
        /// Gets or sets the transaction idle timeout in seconds.
        /// </summary>
        public int TxIdleTimeout { get; set; } = 600;
        /// <summary>
        /// Gets or sets the maximum HTTP request body size in bytes.
        /// </summary>
        public int MaxHttpBodySize { get; set; } = 2 * 1024 * 1024;
    }

    /// <summary>
    /// Defines how Reindexer assigns worker threads to a network endpoint.
    /// </summary>
    public enum ThreadingOptions
    {
        /// <summary>
        /// Uses the shared Reindexer worker thread group.
        /// </summary>
        Shared,
        /// <summary>
        /// Uses dedicated worker threads for the endpoint.
        /// </summary>
        Dedicated,
        /// <summary>
        /// Uses a worker thread pool for the endpoint.
        /// </summary>
        Pool
    }
}
