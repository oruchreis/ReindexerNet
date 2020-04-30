using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReindexerNet.Embedded;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReindexerNet.EmbeddedTest
{
    [TestClass]
    public class EmbeddedServerTest : EmbeddedTest
    {
        protected override IReindexerClient Client { get; set; } = new ReindexerEmbeddedServer();
        protected override string NsName { get; set; } = nameof(EmbeddedServerTest);

        void Log(LogLevel level, string msg)
        {
            if (level <= LogLevel.Info)
                TestContext.WriteLine("{0}: {1}", level, msg);
        }

        [TestInitialize]
        public override async Task InitAsync()
        {
            Client.Connect("dbname=ServerTest;storagepath=.\\EmbeddedServer;httpAddr=127.0.0.1:9088;rpcAddr=127.0.0.1:6354");
            ReindexerEmbeddedServer.EnableLogger(Log);
            
            Client.OpenNamespace(NsName);
            await Client.TruncateNamespaceAsync(NsName);
        }

        //[TestMethod]
        //public void WaitInfinite() //for testing face and swagger.
        //{
        //    while (true)
        //        Thread.Sleep(5000);
        //}
    }
}
