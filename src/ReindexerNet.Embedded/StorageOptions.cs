using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    public sealed class StorageOptions
    {
        public string Path { get; set; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ReindexerEmbeddedServer");
        public StorageEngine Engine { get; set; } = StorageEngine.LevelDb;
        public bool StartWithErrors { get; set; } = false;
        public bool AutoRepair { get; set; } = false;
    }

}
