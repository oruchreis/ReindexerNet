using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ReindexerNet.EmbeddedTest
{
    [TestCategory("RocksDb")]
    [TestClass]
#pragma warning disable S2187 // TestCases should contain tests
    public class RocksDbEmbeddedTest: EmbeddedTest
#pragma warning restore S2187 // TestCases should contain tests
    {
        protected override StorageEngine Storage => StorageEngine.RocksDb;
    }
}
