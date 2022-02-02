using ReindexerNet.Remote.Grpc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using ReindexerNet.Embedded;
using System.IO;
using System;
using System.Diagnostics;
using System.Linq;
using ReindexerNet.CoreTest;
using System.Threading;

namespace ReindexerNet.Remote.Grpc.Tests
{
    [TestClass()]
    public class GrpcTest: BaseTest<ReindexerGrpcClient>
    {
        protected override ReindexerGrpcClient Client { get; set; }
        protected virtual ReindexerEmbeddedServer Server { get; set; }
        protected override string NsName { get; set; } = nameof(GrpcTest);
        protected override string DbPath { get; set; }
        protected override StorageEngine Storage => StorageEngine.LevelDb;

        public TestContext TestContext { get; set; }

        private string _logFile;

        public GrpcTest() : base(testModifedItemCount: false, testPrecepts: false)
        {
        }

        private static long _testIndex = -1;

        [TestInitialize]
        public virtual async Task InitAsync()
        {
            var index = Interlocked.Increment(ref _testIndex)*2;
            var db = "RemoteServerDb";

            var DbPath = Path.Combine(Path.GetTempPath(), db, TestContext.TestName);
            if (Directory.Exists(DbPath))
                Directory.Delete(DbPath, true);
            _logFile = Path.Combine(DbPath, "..", TestContext.TestName + ".log");
            if (File.Exists(_logFile))
                File.Delete(_logFile);
            var grpcAddr = $"127.0.0.1:{16534 + index}";
            Server = new ReindexerEmbeddedServer(new ServerOptions
            {
                HttpAddress = $"127.0.0.1:{11088 + index}",
                RpcAddress = $"127.0.0.1:{13534 + index}",
                EnableGrpc = true,
                GrpcAddress = grpcAddr,
                DatabaseName = db,
                Storage = new StorageOptions
                {
                    Path = DbPath,
                    Engine = Storage,
                },
                Logger = new LoggerOptions
                {
                    CoreLogFile = _logFile,
                    HttpLogFile = _logFile,
                    RpcLogFile = _logFile,
                    ServerLogFile = _logFile,
                }
            });

            await Server.ConnectAsync();

            await Server.OpenNamespaceAsync(NsName);
            await Server.TruncateNamespaceAsync(NsName).ConfigureAwait(false);

            Client = new ReindexerGrpcClient(new ReindexerConnectionString
            {
                DatabaseName = db,
                GrpcAddress = grpcAddr
            });
            await Client.ConnectAsync();
        }

        [TestCleanup]
        public virtual async Task Cleanup()
        {
            await Server.DisposeAsync();
            await Task.Delay(100);
            var fs = new FileStream(_logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var textReader = new StreamReader(fs))
                TestContext.WriteLine(textReader.ReadToEnd());
            if (Directory.Exists(DbPath))
                Directory.Delete(DbPath, true);
            await Client.DisposeAsync();
        }

        protected void Log(LogLevel level, string msg)
        {
            if (level <= LogLevel.Info)
                Debug.WriteLine("[{0:HH:mm:ss.fff}]\t{1}", DateTime.Now, $"[{level}] {msg}");
        }        
        
        [TestMethod()]
        public async Task RenameNamespaceAsyncTest()
        {
            await Assert.ThrowsExceptionAsync<NotSupportedException>(async () => await Client.RenameNamespaceAsync(NsName, ".."));
        }

    }
}
