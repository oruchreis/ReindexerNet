using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    /// <summary>
    /// Configures storage location, engine, and recovery behavior for the embedded Reindexer server.
    /// </summary>
    public sealed class StorageOptions
    {
        /// <summary>
        /// Gets or sets the filesystem path used by the embedded server storage.
        /// </summary>
        public string Path { get; set; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ReindexerEmbeddedServer");
        /// <summary>
        /// Gets or sets the storage engine used by the embedded server.
        /// </summary>
        public StorageEngine Engine { get; set; } = StorageEngine.LevelDb;
        /// <summary>
        /// Allows the server to start when storage contains recoverable errors.
        /// </summary>
        public bool StartWithErrors { get; set; } = false;
        /// <summary>
        /// Enables automatic storage repair during startup.
        /// </summary>
        public bool AutoRepair { get; set; } = false;
    }

}
