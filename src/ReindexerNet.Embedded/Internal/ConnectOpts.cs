#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable RCS1018 // Add accessibility modifiers.
#pragma warning disable S101 // Types should be named in PascalCase
using System;
using System.Runtime.InteropServices;
using uint16_t = System.UInt16;

namespace ReindexerNet.Embedded.Internal
{
    [Flags]
    enum ConnectOpt : uint16_t
    {
        kConnectOptOpenNamespaces = 1 << 0,
        kConnectOptAllowNamespaceErrors = 1 << 1,
        kConnectOptAutorepair = 1 << 2,
        kConnectOptCheckClusterID = 1 << 3,
        kConnectOptWarnVersion = 1 << 4,
    }

    enum StorageTypeOpt : uint16_t
    {
        kStorageTypeOptLevelDB = 0,
        kStorageTypeOptRocksDB = 1,
    }

    [StructLayout(LayoutKind.Sequential, Size = 8)]
    struct ConnectOpts
    {
        public StorageTypeOpt storage;
        public ConnectOpt options;
        public int expectedClusterID;

        public static implicit operator ConnectOpts(ConnectionOptions options)
        {
            return new ConnectOpts
            {
                expectedClusterID = options.ExpectedClusterId,
                storage = (StorageTypeOpt)(int)options.Storage,
                options = 
                    (options.AllowNamespaceErrors ? ConnectOpt.kConnectOptAllowNamespaceErrors : 0) |
                    (options.AutoRepair ? ConnectOpt.kConnectOptAutorepair : 0) |
                    (options.CheckClusterId ? ConnectOpt.kConnectOptCheckClusterID : 0) |
                    (options.OpenNamespaces ? ConnectOpt.kConnectOptOpenNamespaces : 0) |
                    (options.WarnVersion ? ConnectOpt.kConnectOptWarnVersion : 0)
            };
        }
    }
}
#pragma warning restore S101 // Types should be named in PascalCase
#pragma warning restore RCS1018 // Add accessibility modifiers.
#pragma warning restore IDE1006 // Naming Styles
