using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReindexerNet.Embedded;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ReindexerNet.EmbeddedTest
{
    [TestCategory("LevelDb")]
    [TestClass]
#pragma warning disable S2187 // TestCases should contain tests
    public class EmbeddedServerTest : EmbeddedTest
#pragma warning restore S2187 // TestCases should contain tests
    {
        private static long _testIndex = -1;

        protected override IReindexerClient Client { get; set; }
        protected override string NsName { get; set; } = nameof(EmbeddedServerTest);

        private string _logFile;
        private long _currentIndex = Interlocked.Increment(ref _testIndex)*2;

        [TestInitialize]
        public override async Task InitAsync()
        {
            DbPath = Path.Combine(Path.GetTempPath(), "ReindexerEmbeddedServer", TestContext.TestName, Storage.ToString());
            if (Directory.Exists(DbPath))
                Directory.Delete(DbPath, true);
            _logFile = Path.Combine(DbPath, "..", TestContext.TestName + ".log");
            if (File.Exists(_logFile))
                File.Delete(_logFile);
            var storage = Storage == StorageEngine.RocksDb ? "rocksdb": "leveldb";
            Client = new ReindexerEmbeddedServer($"dbname=ServerTest;storagepath={DbPath};httpAddr=127.0.0.1:{9088 + _currentIndex};rpcAddr=127.0.0.1:{6354 + _currentIndex};logFile={_logFile};engine={storage}");
            Client.Connect();

            Client.OpenNamespace(NsName);
            await Client.TruncateNamespaceAsync(NsName).ConfigureAwait(false);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            Client.Dispose();
            Thread.Sleep(100);
            var fs = new FileStream(_logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var textReader = new StreamReader(fs))
                TestContext.WriteLine(textReader.ReadToEnd());
            if (Directory.Exists(DbPath))
                Directory.Delete(DbPath, true);
        }

        [TestMethod]
        public async Task FaceTestAsync()
        {
            var httpClient = new HttpClient();
            var rsp = await httpClient.GetAsync($"http://127.0.0.1:{9088+_currentIndex}/face");
            Assert.AreEqual(200, (int)rsp.StatusCode);
        }

        [TestMethod]
        public async Task SwaggerTestAsync()
        {
            var httpClient = new HttpClient();
            var rsp = await httpClient.GetAsync($"http://127.0.0.1:{9088+_currentIndex}/swagger");
            Assert.AreEqual(200, (int)rsp.StatusCode);
        }
    }
}
