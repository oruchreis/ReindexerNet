using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    public class ServerOptions : ReindexerConnectionString
    {
        public bool EnableGrpc { get; set; } = false;
        public StorageOptions Storage { get; set; } = new StorageOptions();
        public NetworkOptions Network { get; set; } = new NetworkOptions();
        public LoggerOptions Logger { get; set; } = new LoggerOptions();
        public MetricOptions Metrics { get; set; } = new MetricOptions();
        public DebugOptions Debug { get; set; } = new DebugOptions();

        public ServerOptions()
        {

        }

        /// <summary></summary>
        /// <param name="keyValuePair">Connection string in this format of string <c>key1=value1;key2=value2</c>.
        /// <list type="bullet">
        /// <listheader>Supported parameters:</listheader>
        /// <item><term>httpaddr</term><description>(default "0.0.0.0:9088")</description></item>
        /// <item><term>rpcaddr</term><description>(default "0.0.0.0:6534")</description></item>
        /// <item><term>grpc</term><description>(default false)</description></item>
        /// <item><term>grpcaddr</term><description>(default "0.0.0.0:16534")</description></item>
        /// <item><term>dbName</term><description>(*required)</description></item>
        /// <item><term>storagePath</term><description>(default "%TEMP%\ReindexerEmbeddedServer")</description></item>
        /// <item><term>user</term><description>(default null)</description></item>
        /// <item><term>pass</term><description>(default null)</description></item>
        /// <item><term>engine</term><description>(default leveldb)</description></item>
        /// <item><term>autorepair</term><description>(default true)</description></item>
        /// <item><term>logfile</term><description>(default none)</description></item>
        /// <item><term>loglevel</term><description>(default info)</description></item>
        /// <item><term>clientsstats</term><description>(default false)</description></item>
        /// <item><term>prometheus</term><description>(default false)</description></item>
        /// <item><term>pprof</term><description>(default false)</description></item>
        /// <item><term>allocs</term><description>(default false)</description></item>
        /// <item><term>rpc_threading</term><description>shared,dedicated,pool (default shared)</description></item>
        /// <item><term>http_threading</term><description>shared,dedicated,pool (default shared)</description></item>
        /// <item><term>maxupdatessize</term><description>(default 1024 * 1024 * 1024)</description></item>
        /// <item><term>tx_idle_timeout</term><description>as seconds (default 600)</description></item>
        /// <item><term>max_http_body_size</term><description>(default 2 * 1024 * 1024)</description></item>
        /// </list>
        /// </param>
        public ServerOptions(string keyValuePair): base(keyValuePair)
        {

        }

        protected override void FillValue(string key, string value)
        {
            if (key.Equals("storagepath", StringComparison.InvariantCultureIgnoreCase))
                Storage.Path = value;
            else if (key.Equals("engine", StringComparison.InvariantCultureIgnoreCase) && Enum.TryParse(value, true, out StorageEngine engine))
                Storage.Engine = engine;
            else if (key.Equals("autorepair", StringComparison.InvariantCultureIgnoreCase) && bool.TryParse(value, out var autoRepair))
                Storage.AutoRepair = autoRepair;
            else if (key.Equals("security", StringComparison.InvariantCultureIgnoreCase) && bool.TryParse(value, out var security))
                Network.EnableSecurity = security;
            else if (key.Equals("grpc", StringComparison.InvariantCultureIgnoreCase) && bool.TryParse(value, out var grpc))
                EnableGrpc = grpc;
            else if (key.Equals("rpc_threading", StringComparison.InvariantCultureIgnoreCase) && Enum.TryParse(value, true, out ThreadingOptions rpcThreading))
                Network.RpcThreading = rpcThreading;
            else if (key.Equals("http_threading", StringComparison.InvariantCultureIgnoreCase) && Enum.TryParse(value, true, out ThreadingOptions httpThreading))
                Network.HttpThreading = httpThreading;
            else if (key.Equals("maxupdatessize", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(value, out var maxUpdatesSize))
                Network.MaxUpdatesSize = maxUpdatesSize;
            else if (key.Equals("tx_idle_timeout", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(value, out var txIdleTimeout))
                Network.TxIdleTimeout = txIdleTimeout;
            else if (key.Equals("max_http_body_size", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(value, out var httpBodySize))
                Network.MaxHttpBodySize = httpBodySize;
            else if (key.Equals("loglevel", StringComparison.InvariantCultureIgnoreCase) && Enum.TryParse(value, true, out LogLevel level))
                Logger.Level = level;
            else if (key.Equals("logfile", StringComparison.InvariantCultureIgnoreCase))
            {
                Logger.ServerLogFile = value;
                Logger.HttpLogFile = value;
                Logger.CoreLogFile = value;
                Logger.RpcLogFile = value;
            }
            else if (key.Equals("pprof", StringComparison.InvariantCultureIgnoreCase) && bool.TryParse(value, out var pprof))
                Debug.PProf = pprof;
            else if (key.Equals("allocs", StringComparison.InvariantCultureIgnoreCase) && bool.TryParse(value, out var allocs))
                Debug.Allocs = allocs;
            else if (key.Equals("clientsstats", StringComparison.InvariantCultureIgnoreCase) && bool.TryParse(value, out var clientStats))
                Metrics.EnableClientStats = clientStats;
            else if (key.Equals("prometheus", StringComparison.InvariantCultureIgnoreCase) && bool.TryParse(value, out var prometheus))
                Metrics.EnablePrometheus = prometheus;
            else if (key.Equals("collect_period", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(value, out var collectPeriod))
                Metrics.CollectPeriod = collectPeriod;
            else
                base.FillValue(key, value);
        }

        public static implicit operator ServerOptions(string keyValueString)
        {
            return new ServerOptions(keyValueString);
        }

        public static implicit operator string(ServerOptions serverOptions)
        {
            return serverOptions?.ToYaml();
        }

        public override string ToString()
        {
            return ToYaml();
        }

        public string ToYaml()
        {
            return $@"
  storage:
    path: ""{Storage.Path}""
    engine: {Storage.Engine.ToString().ToLowerInvariant()}
    startwitherrors: false
    autorepair: {Storage.AutoRepair.ToString().ToLowerInvariant()}
  net:
    httpaddr: {HttpAddress}
    rpcaddr: {RpcAddress}
    security: {Network.EnableSecurity.ToString().ToLowerInvariant()}
    grpc: {EnableGrpc.ToString().ToLowerInvariant()}
    grpcaddr: {GrpcAddress}
    rpc_threading: {Network.RpcThreading.ToString().ToLowerInvariant()}
    http_threading: {Network.HttpThreading.ToString().ToLowerInvariant()}
    maxupdatessize: {Network.MaxUpdatesSize}
    tx_idle_timeout: {Network.TxIdleTimeout}
    max_http_body_size: {Network.MaxHttpBodySize}
  logger:
    serverlog: ""{Logger.ServerLogFile}""
    corelog: ""{Logger.CoreLogFile}""
    httplog: ""{Logger.HttpLogFile}""
    rpclog: ""{Logger.RpcLogFile}""
    loglevel: {Logger.Level.ToString().ToLowerInvariant()}
  debug:
    pprof: {Debug.PProf.ToString().ToLowerInvariant()}
    allocs: {Debug.Allocs.ToString().ToLowerInvariant()}
  metrics:
    prometheus: {Metrics.EnablePrometheus.ToString().ToLowerInvariant()}
    collect_period: {Metrics.CollectPeriod}
    clientsstats: {Metrics.EnableClientStats.ToString().ToLowerInvariant()}
";
        }
    }
}
