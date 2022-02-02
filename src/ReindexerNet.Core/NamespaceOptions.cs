namespace ReindexerNet
{
    /// <summary>
    /// Namespace options.
    /// </summary>
    public class NamespaceOptions
    {
        /// <summary>
        /// Only in memory, don't write to storage.
        /// </summary>
        public bool EnableStorage { get; set; } = true;
        /// <summary>
        /// Drops namespace when there is error in file format.
        /// </summary>
        public bool DropOnFileFormatError { get; set; }
        /// <summary>
        /// Create namespace if missing
        /// </summary>
        public bool CreateIfMissing { get; set; } = true;
        /// <summary>
        /// Verify checksum
        /// </summary>
        public bool VerifyChecksums { get; set; }
        /// <summary>
        /// Lazy load namespace data.
        /// </summary>
        public bool LazyLoad { get; set; } = true;
        /// <summary>
        /// Namespace is temporary
        /// </summary>
        public bool Temporary { get; set; }
        /// <summary>
        /// Auto repair namespace
        /// </summary>
        public bool AutoRepair { get; set; }

        public bool FillCache { get; set; }
        public bool SlaveMode { get; set; }
        public bool Sync { get; set; }
    }
}
