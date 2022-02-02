namespace ReindexerNet
{
    /// <summary>
    /// Connection Options
    /// </summary>
    public class ConnectionOptions
    {
        /// <summary>
        /// Open all namespaces on connect
        /// </summary>
        public bool OpenNamespaces { get; set; } = true;
        /// <summary>
        /// Allows namespace errors.
        /// </summary>
        public bool AllowNamespaceErrors { get; set; }
        /// <summary>
        /// Auto repair db.
        /// </summary>
        public bool AutoRepair { get; set; }
        /// <summary>
        /// Expected cluster id for replication.
        /// </summary>
        public int ExpectedClusterId { get; set; }
        /// <summary>
        /// Check if the client cluster id is different form server.
        /// </summary>
        public bool CheckClusterId { get; set; }
        /// <summary>
        /// Warn if the client version is different form server.
        /// </summary>
        public bool WarnVersion { get; set; }

        /// <summary>
        /// Storage engines.
        /// </summary>
        public StorageEngine Engine { get; set; } = StorageEngine.LevelDb;
        public bool DisableReplication { get; set; }
    }

    /// <summary>
    /// Storage options.
    /// </summary>
    public enum StorageEngine
    {
        /// <summary>
        /// LevelDb from Google.
        /// </summary>
        LevelDb = 0,
        /// <summary>
        /// RocksDb from Facebook.
        /// </summary>
        RocksDb = 1
    }
}
