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

        public TestContext TestContext { get; set; }

        void Log(LogLevel level, string msg)
        {
            if (level <= LogLevel.Info)
                TestContext.WriteLine("{0}: {1}", level, msg);
        }

        [TestInitialize]
        public override async Task InitAsync()
        {
            Client.Connect("dbname=ServerTest;storagepath=.\\EmbeddedServer");
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
