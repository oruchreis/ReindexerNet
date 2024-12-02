using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    public class NetworkOptions
    {
        public bool EnableSecurity { get; set; } = false;
        public ThreadingOptions RpcThreading { get; set; } = ThreadingOptions.Shared;
        public ThreadingOptions HttpThreading { get; set; } = ThreadingOptions.Shared;
        public int MaxUpdatesSize { get; set; } = 1024 * 1024 * 1024;
        public int TxIdleTimeout { get; set; } = 600;
        public int MaxHttpBodySize { get; set; } = 2 * 1024 * 1024;
    }

    public enum ThreadingOptions
    {
        Shared,
        Dedicated,
        Pool
    }
}