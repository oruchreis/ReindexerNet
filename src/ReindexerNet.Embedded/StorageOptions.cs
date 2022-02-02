using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    public sealed class StorageOptions
    {
        public string Path { get; set; } = @"%TEMP%\ReindexerEmbeddedServer";
        public StorageEngine Engine { get; set; } = StorageEngine.LevelDb;
        public bool StartWithErrors { get; set; } = false;
        public bool AutoRepair { get; set; } = false;
    }

}
