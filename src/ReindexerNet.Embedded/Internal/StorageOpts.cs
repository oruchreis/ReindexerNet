#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable RCS1018 // Add accessibility modifiers.
#pragma warning disable S101 // Types should be named in PascalCase
using System;
using System.Runtime.InteropServices;
using uint16_t = System.UInt16;

namespace ReindexerNet.Embedded.Internal
{
    [Flags]
    enum StorageOpt : uint16_t
    {
        kStorageOptEnabled = 1 << 0,
        kStorageOptDropOnFileFormatError = 1 << 1,
        kStorageOptCreateIfMissing = 1 << 2,
        kStorageOptVerifyChecksums = 1 << 3,
        kStorageOptFillCache = 1 << 4,
        kStorageOptSync = 1 << 5,
        kStorageOptLazyLoad = 1 << 6,
        kStorageOptSlaveMode = 1 << 7,
        kStorageOptTemporary = 1 << 8,
        kStorageOptAutorepair = 1 << 9,
    }

    [StructLayout(LayoutKind.Sequential, Size = 4)]
    struct StorageOpts
    {
        public StorageOpt options;
        public uint16_t noQueryIdleThresholdSec;

        public static implicit operator StorageOpts(NamespaceOptions options)
        {
            return new StorageOpts
            {
                options = 
                    (options.AutoRepair ? StorageOpt.kStorageOptAutorepair : 0) |
                    (options.CreateIfMissing ? StorageOpt.kStorageOptCreateIfMissing : 0) |
                    (options.DropOnFileFormatError ? StorageOpt.kStorageOptDropOnFileFormatError : 0) |
                    (options.EnableStorage ? StorageOpt.kStorageOptEnabled : 0) |
                    (options.LazyLoad ? StorageOpt.kStorageOptLazyLoad : 0) |
                    (options.Temporary ? StorageOpt.kStorageOptTemporary : 0) |
                    (options.VerifyChecksums ? StorageOpt.kStorageOptVerifyChecksums : 0)
            };
        }
    }
}
#pragma warning restore S101 // Types should be named in PascalCase
#pragma warning restore RCS1018 // Add accessibility modifiers.
#pragma warning restore IDE1006 // Naming Styles
