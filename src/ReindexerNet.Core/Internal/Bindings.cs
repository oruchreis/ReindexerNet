using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ReindexerNet.EmbeddedTest")]

namespace ReindexerNet.Internal;

internal static class Bindings
{
    public const int CInt32Max = int.MaxValue;
    public const string ReindexerVersion = "v3.20.0";//Api version that supports. increase with grpc and openapi version    

    internal enum LogLevel
    {
        ERROR = 1,
        WARNING = 2,
        INFO = 3,
        TRACE = 4
    }

    internal enum Agg
    {
        Sum = 0,
        Avg = 1,
        Facet = 2,
        Min = 3,
        Max = 4,
        Distinct = 5
    }

    internal enum CollationType
    {
        None = 0,
        ASCII = 1,
        UTF8 = 2,
        Numeric = 3,
        Custom = 4
    }

    internal enum Op
    {
        Or = 1,
        And = 2,
        Not = 3
    }

    internal enum Value
    {
        Int64 = 0,
        Double = 1,
        String = 2,
        Bool = 3,
        Null = 4,
        Int = 8,
        Undefined = 9,
        Composite = 10,
        Tuple = 11,
        Uuid = 12
    }

    internal enum Query
    {
        Condition = 0,
        Distinct = 1,
        SortIndex = 2,
        JoinOn = 3,
        Limit = 4,
        Offset = 5,
        ReqTotal = 6,
        DebugLevel = 7,
        Aggregation = 8,
        SelectFilter = 9,
        SelectFunction = 10,
        End = 11,
        Explain = 12,
        EqualPosition = 13,
        UpdateField = 14,
        AggregationLimit = 15,
        AggregationOffset = 16,
        AggregationSort = 17,
        OpenBracket = 18,
        CloseBracket = 19,
        JoinCondition = 20,
        DropField = 21,
        UpdateObject = 22,
        WithRank = 23,
        StrictMode = 24,
        UpdateFieldV2 = 25,
        BetweenFieldsCondition = 26,
        AlwaysFalseCondition = 27
    }

    internal enum JoinType
    {
        LeftJoin = 0,
        InnerJoin = 1,
        OrInnerJoin = 2,
        Merge = 3
    }

    internal enum CacheMode
    {
        On = 0,
        Aggressive = 1,
        Off = 2
    }

    internal enum FormatType
    {
        Json = 0,
        CJson = 1
    }

    internal enum OperationMode
    {
        Update = 0,
        Insert = 1,
        Upsert = 2,
        Delete = 3
    }

    internal enum TotalMode
    {
        NoCalc = 0,
        CachedTotal = 1,
        AccurateTotal = 2
    }

    internal enum QueryResultType
    {
        End = 0,
        Aggregation = 1,
        Explain = 2
    }

    internal enum StrictModeType
    {
        NotSet = 0,
        None = 1,
        Names = 2,
        Indexes = 3
    }

    internal enum ResultsFormat
    {
        Pure = 0x0,
        Ptrs = 0x1,
        CJson = 0x2,
        Json = 0x3
    }

    internal enum ResultsOptions
    {
        WithPayloadTypes = 0x10,
        WithItemID = 0x20,
        WithPercents = 0x40,
        WithNsID = 0x80,
        WithJoined = 0x100,
        SupportIdleTimeout = 0x2000
    }

    internal enum IndexOptions
    {
        OptPK = 1 << 7,
        OptArray = 1 << 6,
        OptDense = 1 << 5,
        OptAppendable = 1 << 4,
        OptSparse = 1 << 3
    }

    internal enum StorageOptions
    {
        Enabled = 1,
        DropOnFileFormatError = 1 << 1,
        CreateIfMissing = 1 << 2
    }

    internal enum ConnectOptions
    {
        OpenNamespaces = 1,
        AllowNamespaceErrors = 1 << 1,
        Autorepair = 1 << 2,
        WarnVersion = 1 << 4
    }

    internal enum ErrorCode
    {
        OK = 0,
        ParseSQL = 1,
        QueryExec = 2,
        Params = 3,
        Logic = 4,
        ParseJson = 5,
        ParseDSL = 6,
        Conflict = 7,
        ParseBin = 8,
        Forbidden = 9,
        WasRelock = 10,
        NotValid = 11,
        Network = 12,
        NotFound = 13,
        StateInvalidated = 14,
        Timeout = 19,
        Canceled = 20,
        TagsMismatch = 21,
        ReplParams = 22,
        NamespaceInvalidated = 23,
        ParseMsgPack = 24,
        ParseProtobuf = 25,
        UpdatesLost = 26,
        WrongReplicationData = 27,
        UpdateReplication = 28,
        ClusterConsensus = 29,
        Terminated = 30,
        TxDoesNotExist = 31,
        AlreadyConnected = 32,
        TxInvalidLeader = 33,
        AlreadyProxied = 34,
        StrictMode = 35,
        QrUIDMismatch = 36,
        System = 37,
        Assert = 38
    }
}