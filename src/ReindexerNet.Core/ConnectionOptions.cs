using System;
using System.Collections.Generic;
using System.Text;

namespace ReindexerNet
{
    public class ConnectionOptions
    {
        /// <summary>
        /// Open all namespaces on connect
        /// </summary>
        public bool OpenNamespaces { get; set; } = true;
        public bool AllowNamespaceErrors { get; set; }
        public bool AutoRepair { get; set; }
        public int ExpectedClusterId { get; set; }
        public bool CheckClusterId { get; set; }
        public bool WarnVersion { get; set; }
        public StorageOption Storage { get; set; } = StorageOption.LevelDb;
    }

    public enum StorageOption
    {
        LevelDb = 0,
        RocksDb = 1
    }
}
