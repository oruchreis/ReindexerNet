﻿using ReindexerNet.Embedded.Helpers;
using ReindexerNet.Embedded.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ReindexerNet.Embedded
{
    /// <summary>
    /// Reindexer Embedded Server implementation. Currently only one server can exist in the application. Use <see cref="ReindexerEmbedded.Server"/> singleton property to use server.
    /// </summary>
    public sealed class ReindexerEmbeddedServer : ReindexerEmbedded
    {
        private readonly UIntPtr _pServer;
        public ReindexerEmbeddedServer()
        {
            _pServer = ReindexerBinding.init_reindexer_server();
        }

        private const string _defaultServerYamlConfig = @"
  storage:
    path: {0}
    engine: {3}
    startwitherrors: false
    autorepair: {4}
  net:
    httpaddr: {1}
    rpcaddr: {2}
    #webroot:
    security: false
  logger:
    serverlog: stdout
    corelog: stdout
    httplog: stdout
    #rpclog: stdout
    loglevel: {5}
  system:
    user:
  debug:
    pprof: {8}
    allocs: {9}
  metrics:
    prometheus: {7}
    collect_period: 1000
    clientsstats: {6}
";
        /// <summary>
        /// Starts Reindexer Embedded Server
        /// </summary>
        /// <param name="connectionString">Connection string in this format of string <c>key1=value1;key2=value2</c>.
        /// <list type="bullet">
        /// <listheader>Supported parameters:</listheader>
        /// <item><term>httpAddr</term><description>(default "0.0.0.0:9088")</description></item>
        /// <item><term>rpcAddr</term><description>(default "0.0.0.0:6534")</description></item>
        /// <item><term>dbName</term><description>(*required)</description></item>
        /// <item><term>storagePath</term><description>(default "%TEMP%\ReindexerEmbeddedServer")</description></item>
        /// <item><term>user</term><description>(default null)</description></item>
        /// <item><term>pass</term><description>(default null)</description></item>
        /// <item><term>engine</term><description>(default leveldb)</description></item>
        /// <item><term>autorepair</term><description>(default false)</description></item>
        /// <item><term>loglevel</term><description>(default info)</description></item>
        /// <item><term>clientsstats</term><description>(default false)</description></item>
        /// <item><term>prometheus</term><description>(default false)</description></item>
        /// <item><term>pprof</term><description>(default false)</description></item>
        /// <item><term>allocs</term><description>(default false)</description></item>
        /// </list>
        /// </param>
        /// <param name="options"></param>
        public override void Connect(string connectionString, ConnectionOptions options = null)
        {
            if (IsStarted)
                throw new ReindexerNetException("Server has been already started. Stop first.");

            var config = new Dictionary<string, string>
            {
                ["dbname"] = null,
                ["user"] = null,
                ["pass"] = null,

                ["storagepath"] = Path.Combine(Path.GetTempPath(), "ReindexerEmbeddedServer"), // 0
                ["httpaddr"] = "0.0.0.0:9088",                                                 // 1
                ["rpcaddr"] = "0.0.0.0:6534",                                                  // 2
                ["engine"] = "leveldb",                                                        // 3
                ["autorepair"] = "false",                                                      // 4
                ["loglevel"] = "info",                                                         // 5
                ["clientsstats"] = "false",                                                    // 6
                ["prometheus"] = "false",                                                      // 7
                ["pprof"] = "false",                                                           // 8
                ["allocs"] = "false"                                                           // 9
            };

            var connStringParts = connectionString.Split(';');
            foreach (var (key, value) in connStringParts.Select(p => p.Split('=')).Select(p => p.Length > 1 ? (p[0].ToLowerInvariant(), p[1]) : (p[0].ToLowerInvariant(), "")))
            {
                config[key] = value;
            }

            if (config["dbname"] == null)
            {
                throw new ArgumentException("You must provide a db name with 'dbname' config key.");
            }

            var dbPath = Path.Combine(config["storagepath"], config["dbname"]);
            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath); //reindexer sometimes throws permission exception from c++ mkdir func. so we try to crate directory before.
            }

            Start(string.Format(_defaultServerYamlConfig,
                config["storagepath"], config["httpaddr"], config["rpcaddr"],
                config["engine"], config["autorepair"], config["loglevel"],
                config["clientsstats"], config["prometheus"], config["pprof"], config["allocs"]),

                config["dbname"], config["user"], config["pass"]);
        }

        private long _isServerThreadStarted = 0;

        /// <summary>
        /// Is server started.
        /// </summary>
        public bool IsStarted => Interlocked.Read(ref _isServerThreadStarted) == 1;
        private readonly object _serverStartupLocker = new object();

        /// <summary>
        /// Starts the server with server yaml and waits for ready for 5 seconds. Use <see cref="Connect(string, ConnectionOptions)"/> instead.
        /// </summary>
        /// <param name="serverConfigYaml">Reindexer server configuration yaml</param>        
        /// <param name="dbName"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <exception cref="TimeoutException">Throws if the server doesn't start in 5 seconds.</exception>
        public void Start(string serverConfigYaml, string dbName, string user = null, string pass = null)
        {
            lock (_serverStartupLocker) //for not spinning extra threads and double checking lock.
            {
                if (Interlocked.Read(ref _isServerThreadStarted) == 1) //Interlocked for not extra locking in future to check is startup
                {
                    return;
                }

                new Thread(() =>
                {
                    try
                    {
                        ReindexerBinding.start_reindexer_server(_pServer, serverConfigYaml);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _isServerThreadStarted, 0);
                    }
                })
                {
                    Name = nameof(ReindexerEmbeddedServer),
                    IsBackground = true
                }.Start();
                Interlocked.Exchange(ref _isServerThreadStarted, 1);
            }

            var waitTimeout = TimeSpan.FromSeconds(5);
            var startTime = DateTime.UtcNow;
            while (ReindexerBinding.check_server_ready(_pServer) == 0)
            {
                if (DateTime.UtcNow - startTime > waitTimeout)
                    throw new TimeoutException($"Reindexer Embedded Server couldn't be started in {waitTimeout.TotalSeconds} seconds. Check configs.");
                Thread.Sleep(100);
            }

            Assert.ThrowIfError(() => ReindexerBinding.get_reindexer_instance(_pServer, dbName, user, pass, ref Rx));
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void Stop()
        {
            Assert.ThrowIfError(() =>
               ReindexerBinding.stop_reindexer_server(_pServer)
            );
            Rx = default;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Stop();
            base.Dispose(disposing);
        }
    }
}