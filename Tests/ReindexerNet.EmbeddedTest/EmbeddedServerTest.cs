using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReindexerNet.Embedded;
using System.Threading.Tasks;

namespace ReindexerNet.EmbeddedTest
{
    [TestClass]
#pragma warning disable S2187 // TestCases should contain tests
    public class EmbeddedServerTest : EmbeddedTest
#pragma warning restore S2187 // TestCases should contain tests
    {
        protected override IReindexerClient Client { get; set; } = new ReindexerEmbeddedServer();
        protected override string NsName { get; set; } = nameof(EmbeddedServerTest);

        private void Log(LogLevel level, string msg)
        {
            if (level <= LogLevel.Info)
                TestContext.WriteLine("{0}: {1}", level, msg);
        }

        [TestInitialize]
        public override async Task InitAsync()
        {
            Client.Connect("dbname=ServerTest;storagepath=.\\EmbeddedServer;httpAddr=127.0.0.1:9088;rpcAddr=127.0.0.1:6354;logFile=stdout");
            ReindexerEmbeddedServer.EnableLogger(Log);

            Client.OpenNamespace(NsName);
            await Client.TruncateNamespaceAsync(NsName).ConfigureAwait(false);
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
