using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReindexerNet.Embedded;
using System;
using System.Diagnostics;
using System.IO;
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

        [TestInitialize]
        public override async Task InitAsync()
        {
            var index = Interlocked.Increment(ref _testIndex)*2;
            DbPath = Path.Combine(Path.GetTempPath(), "ReindexerEmbeddedServer", TestContext.TestName, Storage.ToString());
            if (Directory.Exists(DbPath))
                Directory.Delete(DbPath, true);
            _logFile = Path.Combine(DbPath, "..", TestContext.TestName + ".log");
            if (File.Exists(_logFile))
                File.Delete(_logFile);
            var storage = Storage == StorageEngine.RocksDb ? "rocksdb": "leveldb";
            Client = new ReindexerEmbeddedServer($"dbname=ServerTest;storagepath={DbPath};httpAddr=127.0.0.1:{9088 + index};rpcAddr=127.0.0.1:{6354 + index};logFile={_logFile};engine={storage}");
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

#pragma warning disable S125 // Sections of code should not be commented out
        //[TestMethod]
        //public void WaitInfinite() //for testing face and swagger.
        //{
        //    while (true)
        //        Thread.Sleep(5000);
        //}
    }
#pragma warning restore S125 // Sections of code should not be commented out
}
