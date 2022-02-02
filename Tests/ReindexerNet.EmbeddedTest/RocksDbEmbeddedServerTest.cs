using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ReindexerNet.EmbeddedTest
{
    [TestCategory("RocksDb")]
    [TestClass]
#pragma warning disable S2187 // TestCases should contain tests
    public class RocksDbEmbeddedServerTest: EmbeddedServerTest
#pragma warning restore S2187 // TestCases should contain tests
    {
        protected override StorageEngine Storage => StorageEngine.RocksDb;
    }
}
