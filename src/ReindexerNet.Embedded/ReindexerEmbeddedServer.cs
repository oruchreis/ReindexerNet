using ReindexerNet.Embedded.Internal;
using ReindexerNet.Embedded.Internal.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded
{
    /// <summary>
    /// Reindexer Embedded Server implementation.
    /// </summary>
    public sealed class ReindexerEmbeddedServer : ReindexerEmbedded
    {
        private readonly ServerOptions _serverOptions;
        private UIntPtr _pServer;
        /// <summary>
        /// Creates a reindexer server instance
        /// </summary>
        /// <param name="serverOptions">Connection string for the server.</param>
        /// <param name="serializer"></param>
        public ReindexerEmbeddedServer(ServerOptions serverOptions, IReindexerSerializer serializer = null)
            : base(serverOptions.DatabaseName, serializer)
        {
            _serverOptions = serverOptions;
            _pServer = ReindexerBinding.init_reindexer_server();
        }


        /// <summary>
        /// Starts Reindexer Embedded Server
        /// </summary>
        /// <param name="options">Reindexer connection options.</param>
        public override void Connect(ConnectionOptions options = null)
        {
            if (IsStarted)
                throw new ReindexerNetException("Server has been already started. Stop first.");

            if (_serverOptions.DatabaseName == null)
            {
                throw new ArgumentException("You must provide a db name with 'dbname' config key.");
            }

            if (_serverOptions.HttpAddress == null)
                _serverOptions.HttpAddress = "0.0.0.0:9088";
            if (_serverOptions.RpcAddress == null)
                _serverOptions.RpcAddress = "0.0.0.0:6534";
            if (_serverOptions.GrpcAddress == null)
                _serverOptions.GrpcAddress = "0.0.0.0:16534";
            if (_serverOptions.Storage.Path == null)
                _serverOptions.Storage.Path = Path.Combine(Path.GetTempPath(), "ReindexerEmbeddedServer");

            var dbPath = Path.Combine(_serverOptions.Storage.Path, _serverOptions.DatabaseName);
            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath); //reindexer sometimes throws permission exception from c++ mkdir func. so we try to crate directory before.
            }

            Start(_serverOptions.ToYaml(),
                _serverOptions.DatabaseName, _serverOptions.Username, _serverOptions.Password);
        }

        private long _isServerThreadStarted = 0;

        /// <summary>
        /// Is server started.
        /// </summary>
        public bool IsStarted => Interlocked.Read(ref _isServerThreadStarted) == 1;
        private readonly object _serverStartupLocker = new object();
        private Thread _serverThread;

        /// <summary>
        /// Starts the server with server yaml and waits for ready for 5 seconds. Use <see cref="Connect(ConnectionOptions)"/> instead.
        /// </summary>
        /// <param name="serverConfigYaml">Reindexer server configuration yaml</param>        
        /// <param name="dbName"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="waitTimeoutForReady">Wait timeout for the server is ready. Default is 60sec.</param>
        /// <exception cref="TimeoutException">Throws if the server doesn't start in timeout interval.</exception>
        public void Start(string serverConfigYaml, string dbName, string user = null, string pass = null, TimeSpan? waitTimeoutForReady = null)
        {
            lock (_serverStartupLocker) //for not spinning extra threads and double checking lock.
            {
                if (Interlocked.Read(ref _isServerThreadStarted) == 1) //Interlocked for not extra locking in future to check is startup
                {
                    return;
                }
                _serverThread = new Thread(() =>
                {
                    Interlocked.Exchange(ref _isServerThreadStarted, 1);
                    try
                    {
                        DebugHelper.Log("Starting reindexer server...");
                        using (var configYaml = serverConfigYaml.GetStringHandle())
                            ReindexerBinding.start_reindexer_server(_pServer, configYaml);
                    }
                    catch (Exception e)
                    {
                        DebugHelper.Log(e.Message);
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _isServerThreadStarted, 0);
                    }
                })
                {
                    IsBackground = false
                };
                _serverThread.Start();
            }

            var waitTimeout = waitTimeoutForReady ?? TimeSpan.FromSeconds(60);
            var startTime = DateTime.UtcNow;
            while (ReindexerBinding.check_server_ready(_pServer) == 0)
            {
                if (DateTime.UtcNow - startTime > waitTimeout)
                    throw new TimeoutException($"Reindexer Embedded Server couldn't be started in {waitTimeout.TotalSeconds} seconds. Check configs.");
                Thread.Sleep(100);
            }
            DebugHelper.Log("Reindexer server is started.");
            using (var dbNameRx = dbName.GetStringHandle())
            using (var userRx = user.GetStringHandle())
            using (var passRx = pass.GetStringHandle())
                Assert.ThrowIfError(() => ReindexerBinding.get_reindexer_instance(_pServer, dbNameRx, userRx, passRx, ref Rx));
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            DebugHelper.Log("Stopping reindexer server...");
            try
            {
                Parallel.ForEach(ExecuteSql<Namespace>(GetNamespacesQuery).Items, ns =>
                {
                    CloseNamespace(ns.Name);
                });
            }
            catch
            { //sometimes server had been stopped before this stop method because of ctrl+c on console or any sigterm signal.
            }

            Assert.ThrowIfError(() =>
               ReindexerBinding.stop_reindexer_server(_pServer)
            );

            var waitTimeout = TimeSpan.FromSeconds(10);
            var startTime = DateTime.UtcNow;
            while (IsStarted)
            {
                if (DateTime.UtcNow - startTime > waitTimeout)
                {
                    DebugHelper.Log($"Reindexer Embedded Server couldn't be stopped in {waitTimeout.TotalSeconds} seconds.");
                    break;
                }
                Thread.Sleep(100);
            }

            lock (_serverStartupLocker)
            {
                if (_serverThread.IsAlive)
                    _serverThread.Join();
            }

            Rx = default;
            DebugHelper.Log("Reindexer server is stopped.");
        }

        /// <summary>
        /// Reopens server log files.
        /// </summary>
        public void ReopenLogFiles()
        {
            Assert.ThrowIfError(() =>
              ReindexerBinding.reopen_log_files(_pServer)
           );
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Stop();
            ReindexerBinding.destroy_reindexer_server(_pServer);
            _pServer = default;
        }
    }
}
