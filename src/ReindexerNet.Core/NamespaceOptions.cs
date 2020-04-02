using System;
using System.Collections.Generic;
using System.Text;

namespace ReindexerNet
{
    public class NamespaceOptions
    {
        /// <summary>
        /// Only in memory, don't write to storage.
        /// </summary>
        public bool EnableStorage { get; set; } = true;
        public bool DropOnFileFormatError { get; set; }
        /// <summary>
        /// Create namespace if missing
        /// </summary>
        public bool CreateIfMissing { get; set; } = true;
        public bool VerifyChecksums { get; set; }
        /// <summary>
        /// Lazy load namespace data.
        /// </summary>
        public bool LazyLoad { get; set; } = true;
        /// <summary>
        /// Namespace is temporary
        /// </summary>
        public bool Temporary { get; set; }
        public bool AutoRepair { get; set; }
    }
}
