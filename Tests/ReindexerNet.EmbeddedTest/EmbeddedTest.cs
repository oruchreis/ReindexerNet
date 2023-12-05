using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReindexerNet.CoreTest;
using ReindexerNet.Embedded;
using ReindexerNet.Embedded.Internal.Helpers;
using System.IO;
using System.Threading.Tasks;

namespace ReindexerNet.EmbeddedTest;

[TestCategory("LevelDb")]
[TestClass]
public class EmbeddedTest : BaseTest<IReindexerClient>
{
    public EmbeddedTest() : base(true, true)
    {
    }

    protected override IReindexerClient Client { get; set; }
    protected override string NsName { get; set; } = nameof(EmbeddedTest);
    protected override string DbPath { get; set; }
    protected override StorageEngine Storage => StorageEngine.LevelDb;

    [TestInitialize]
    public virtual async Task InitAsync()
    {
        DbPath = Path.Combine(Path.GetTempPath(), "ReindexerEmbedded", TestContext.TestName, Storage.ToString());
        if (Directory.Exists(DbPath))
            Directory.Delete(DbPath, true);
        Client = new ReindexerEmbedded(DbPath);
        ReindexerEmbedded.EnableLogger(Log);
        await Client.ConnectAsync(new ConnectionOptions { Engine = Storage });
        await Client.OpenNamespaceAsync(NsName);
        await Client.TruncateNamespaceAsync(NsName);
    }

    protected void Log(LogLevel level, string msg)
    {
        if (level <= LogLevel.Info)
            DebugHelper.Log($"[{level}] {msg}");
    }

    [TestCleanup]
    public virtual void Cleanup()
    {
        Client?.Dispose();
        if (Directory.Exists(DbPath))
            Directory.Delete(DbPath, true);
    }

    [TestMethod]
    public override async Task ExecuteQueryJson()
    {
        await Microsoft.VisualStudio.TestTools.UnitTesting.Assert.ThrowsExceptionAsync<ReindexerException>(base.ExecuteQueryJson, 
            "Reindexer returned an error response, ErrCode: 8, Msg:Unknown type 34 while parsing binary buffer");
    }
}
