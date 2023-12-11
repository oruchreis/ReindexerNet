using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReindexerNet.Embedded;
using ReindexerNet.Embedded.Internal;
using ReindexerNet.Embedded.Internal.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using ReindexerNet.Internal;

namespace ReindexerNet.EmbeddedTest;

[TestClass]
public class ReindexerBindingTest
{
    private UIntPtr _rx;
    private reindexer_ctx_info _ctxInfo = new reindexer_ctx_info { ctx_id = 0, exec_timeout = -1 };
    public TestContext TestContext { get; set; }

    private void AssertError(reindexer_error error)
    {
        if (error.code != 0)
        {
            var errorStr = Marshal.PtrToStringAnsi(error.what);

            Assert.Fail($"{errorStr}, Error Code: {error.code}");
        }
    }

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    void Log(LogLevel level, string msg)
    {
        if (level <= LogLevel.Info)
            TestContext.WriteLine("{0}: {1}", level, msg);
    }

    private LogWriterAction _logWriter;

    [TestInitialize]
    public void InitReindexer()
    {
        _rx = ReindexerBinding.init_reindexer();
        Assert.AreNotEqual(UIntPtr.Zero, _rx);

        _logWriter = new LogWriterAction(Log);
        ReindexerBinding.reindexer_enable_logger(_logWriter);
        Connect();
    }

    [TestCleanup]
    public void DestroyReindexer()
    {
        ReindexerBinding.destroy_reindexer(_rx);
    }

    [TestMethod]
    public void Connect()
    {
        AssertError(
            ReindexerBinding.reindexer_connect(_rx,
            $"builtin://{Path.Combine(Path.GetTempPath(), "ReindexerBindingTest")}".GetStringHandle(),
            new ConnectOpts
            {
                options = ConnectOpt.kConnectOptAllowNamespaceErrors,
                storage = StorageTypeOpt.kStorageTypeOptLevelDB,
                expectedClusterID = 0
            }, ReindexerBinding.ReindexerVersion.GetStringHandle()));
    }

    [TestMethod]
    public void Ping()
    {
        AssertError(ReindexerBinding.reindexer_ping(_rx));
    }

    [TestMethod]
    public void EnableStorage()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), "TestDbForEnableStorage");
        if (Directory.Exists(dbPath))
            Directory.Delete(dbPath, true);
        AssertError(ReindexerBinding.reindexer_enable_storage(ReindexerBinding.init_reindexer(), dbPath.GetStringHandle(), _ctxInfo));
    }

    [TestMethod]
    public void InitSystemNamespaces()
    {
        AssertError(ReindexerBinding.reindexer_init_system_namespaces(_rx));
    }

    [TestMethod]
    public void OpenNamespace()
    {
        AssertError(ReindexerBinding.reindexer_open_namespace(_rx, "OpenNamespaceTest".GetStringHandle(),
            new StorageOpts { options = StorageOpt.kStorageOptCreateIfMissing | StorageOpt.kStorageOptEnabled },
            _ctxInfo));
        AssertError(ReindexerBinding.reindexer_drop_namespace(_rx, "OpenNamespaceTest".GetStringHandle(), _ctxInfo));
    }

    [TestMethod]
    public void DropNamespace()
    {
        var error = ReindexerBinding.reindexer_drop_namespace(_rx, "DropNamespaceTest".GetStringHandle(), _ctxInfo);
        Assert.AreNotEqual(0, error.code);
        AssertError(ReindexerBinding.reindexer_open_namespace(_rx, "DropNamespaceTest".GetStringHandle(),
            new StorageOpts { options = StorageOpt.kStorageOptCreateIfMissing | StorageOpt.kStorageOptEnabled },
            _ctxInfo));
        AssertError(ReindexerBinding.reindexer_drop_namespace(_rx, "DropNamespaceTest".GetStringHandle(), _ctxInfo));
    }

#if NET472
    private const string FrameworkName="NET472";
#elif NET5_0
    private const string FrameworkName="NET5";
#elif NET6_0
    private const string FrameworkName = "NET6";
#elif NET7_0
    private const string FrameworkName = "NET7";
#elif NET8_0
    private const string FrameworkName = "NET8";
#elif NETCOREAPP2_2
    private const string FrameworkName="NETCOREAPP2_2";
#elif NETCOREAPP3_1
    private const string FrameworkName="NETCOREAPP3_1";
#endif

    private const string DataTestNamespace = nameof(DataTestNamespace) + "_" + FrameworkName;

    public void ModifyItemPacked(string itemJson = null)
    {
        AssertError(ReindexerBinding.reindexer_open_namespace(_rx, DataTestNamespace.GetStringHandle(),
            new StorageOpts { options = StorageOpt.kStorageOptCreateIfMissing | StorageOpt.kStorageOptEnabled },
            _ctxInfo));

        var indexDefJson = JsonSerializer.Serialize(
        new Index
        {
            Name = "Id",
            IsPk = true,
            FieldType = FieldType.Int64,
            IndexType = IndexType.Hash,
            JsonPaths = ["Id"]
        }, _jsonSerializerOptions);
        AssertError(ReindexerBinding.reindexer_add_index(_rx, DataTestNamespace.GetStringHandle(), indexDefJson.GetStringHandle(), _ctxInfo));
        indexDefJson = JsonSerializer.Serialize(
        new Index
        {
            Name = "Guid",
            IsPk = false,
            FieldType = FieldType.String,
            IndexType = IndexType.Hash,
            JsonPaths = ["Guid"]
        }, _jsonSerializerOptions);
        AssertError(ReindexerBinding.reindexer_add_index(_rx, DataTestNamespace.GetStringHandle(), indexDefJson.GetStringHandle(), _ctxInfo));

        var rsp = ReindexerBinding.reindexer_select(_rx,
            $"SELECT 'indexes.name' FROM #namespaces WHERE name = '{DataTestNamespace}'".GetStringHandle(),
            1, [], 0, _ctxInfo);

        if (rsp.err_code != 0)
            Assert.AreEqual(null, (string)rsp.@out);
        Assert.AreNotEqual(UIntPtr.Zero, rsp.@out.results_ptr);

        var (json, offsets, explain) = BindingHelpers.RawResultToJson(rsp.@out, "items", "total_count");
        var indexNames = JsonSerializer.Deserialize<ItemsOf<Namespace>>(json).Items.SelectMany(n => n.Indexes.Select(i => i.Name)).ToList();

        CollectionAssert.Contains(indexNames as ICollection, "Id");
        CollectionAssert.Contains(indexNames as ICollection, "Guid");

        using (var ser1 = new CJsonWriter())
        {
            ser1.PutVString(DataTestNamespace);
            ser1.PutVarCUInt((int)SerializerType.Json);//format
            ser1.PutVarCUInt((int)ItemModifyMode.Upsert);//mode
            ser1.PutVarCUInt(0);//stateToken
            ser1.PutVarCUInt(0);//len(precepts)

            reindexer_buffer.PinBufferFor(ser1.CurrentBuffer, args =>
            {
                using (var data = reindexer_buffer.From(Encoding.UTF8.GetBytes(itemJson ?? $"{{\"Id\":1, \"Guid\":\"{Guid.NewGuid()}\"}}")))
                {
                    rsp = ReindexerBinding.reindexer_modify_item_packed(_rx, args, data.Buffer, _ctxInfo);
                    if (rsp.err_code != 0)
                        Assert.AreEqual(null, (string)rsp.@out);

                    Assert.AreNotEqual(UIntPtr.Zero, rsp.@out.results_ptr);

                    var reader = new CJsonReader(rsp.@out);
                    var rawQueryParams = reader.ReadRawQueryParams();

                    Assert.AreEqual(1, rawQueryParams.count);

                    reader.ReadRawItemParams();

                    if (rsp.@out.results_ptr != UIntPtr.Zero)
                        AssertError(ReindexerBinding.reindexer_free_buffer(rsp.@out));
                }
            });
        }
    }

    [TestMethod]
    public void SelectSql()
    {
        try
        {
            ModifyItemPacked();

            var rsp = ReindexerBinding.reindexer_select(_rx,
                $"SELECT * FROM {DataTestNamespace}".GetStringHandle(),
                1, [], 0, _ctxInfo);
            if (rsp.err_code != 0)
                Assert.AreEqual(null, (string)rsp.@out);
            Assert.AreNotEqual(UIntPtr.Zero, rsp.@out.results_ptr);

            var (json, offsets, explain) = BindingHelpers.RawResultToJson(rsp.@out, "items", "total_count");

            Assert.AreNotEqual(0, json.Length);
            Assert.AreNotEqual(0, offsets.Length);
        }
        finally
        {
            AssertError(ReindexerBinding.reindexer_close_namespace(_rx, DataTestNamespace.GetStringHandle(), _ctxInfo));
        }
    }

    [TestMethod]
    public void ExplainSelectSql()
    {
        try
        {
            ModifyItemPacked();

            var rsp = ReindexerBinding.reindexer_select(_rx,
                $"EXPLAIN SELECT * FROM {DataTestNamespace}".GetStringHandle(),
                1, [], 0, _ctxInfo);
            if (rsp.err_code != 0)
                Assert.AreEqual(null, (string)rsp.@out);
            Assert.AreNotEqual(UIntPtr.Zero, rsp.@out.results_ptr);

            var (json, offsets, explain) = BindingHelpers.RawResultToJson(rsp.@out, "items", "total_count");

            Assert.AreNotEqual(0, json.Length);
            Assert.AreNotEqual(0, offsets.Length);
            Assert.AreNotEqual(0, explain.Length);

            var explainDef = JsonSerializer.Deserialize<ExplainDef>(explain);
            Assert.IsNotNull(explainDef);
        }
        finally
        {
            AssertError(ReindexerBinding.reindexer_close_namespace(_rx, DataTestNamespace.GetStringHandle(), _ctxInfo));
        }
    }

    [TestMethod]
    public void DeleteSql()
    {
        AssertError(ReindexerBinding.reindexer_open_namespace(_rx, DataTestNamespace.GetStringHandle(),
           new StorageOpts { options = StorageOpt.kStorageOptCreateIfMissing | StorageOpt.kStorageOptEnabled },
           _ctxInfo));

        try
        {
            ModifyItemPacked($"{{\"Id\":2, \"Guid\":\"{Guid.NewGuid()}\"}}");

            var delRsp = ReindexerBinding.reindexer_select(_rx,
                $"DELETE FROM {DataTestNamespace} WHERE Id=2".GetStringHandle(),
                1, [], 0, _ctxInfo);
            if (delRsp.err_code != 0)
                Assert.AreEqual(null, (string)delRsp.@out);
            Assert.AreNotEqual(UIntPtr.Zero, delRsp.@out.results_ptr);

            var (json, offsets, explain) = BindingHelpers.RawResultToJson(delRsp.@out, "items", "total_count");
            Assert.AreNotEqual(0, json.Length);
            Assert.AreNotEqual(0, offsets.Length);

            var selRsp = ReindexerBinding.reindexer_select(_rx,
                $"SELECT * FROM {DataTestNamespace} WHERE Id=2".GetStringHandle(),
                1, new int[] { 0 }, 1, _ctxInfo);
            if (selRsp.err_code != 0)
                Assert.AreEqual(null, (string)selRsp.@out);
            Assert.AreNotEqual(UIntPtr.Zero, selRsp.@out.results_ptr);

            (json, offsets, explain) = BindingHelpers.RawResultToJson(selRsp.@out, "items", "total_count");
            Assert.AreNotEqual(0, json.Length);
            Assert.AreEqual(0, offsets.Length);
        }
        finally
        {
            AssertError(ReindexerBinding.reindexer_close_namespace(_rx, DataTestNamespace.GetStringHandle(), _ctxInfo));
        }
    }


    [TestMethod]
    public void ParallelModifyItemPacked()
    {
        var nsName = "ParallelTestNs";
        AssertError(ReindexerBinding.reindexer_open_namespace(_rx, nsName.GetStringHandle(),
            new StorageOpts { options = StorageOpt.kStorageOptCreateIfMissing | StorageOpt.kStorageOptEnabled },
            _ctxInfo));

        var indexDefJson = JsonSerializer.Serialize(
        new Index
        {
            Name = "Id",
            IsPk = true,
            FieldType = FieldType.Int64,
            IndexType = IndexType.Hash,
            JsonPaths = ["Id"]
        }, _jsonSerializerOptions);
        AssertError(ReindexerBinding.reindexer_add_index(_rx, nsName.GetStringHandle(), indexDefJson.GetStringHandle(), _ctxInfo));

        using (var ser1 = new CJsonWriter())
        {
            ser1.PutVString(nsName);
            ser1.PutVarCUInt((int)SerializerType.Json);
            ser1.PutVarCUInt((int)ItemModifyMode.Upsert);
            ser1.PutVarCUInt(0);
            ser1.PutVarCUInt(0);

            reindexer_buffer.PinBufferFor(ser1.CurrentBuffer, args =>
            {
                Parallel.For(0, 30000, i =>
                {
                    var dataHandle = reindexer_buffer.From(Encoding.UTF8.GetBytes($"{{\"Id\":{i}, \"Guid\":\"{Guid.NewGuid()}\"}}"));
                    var rsp = ReindexerBinding.reindexer_modify_item_packed(_rx, args, dataHandle.Buffer, _ctxInfo);
                    try
                    {
                        if (rsp.err_code != 0)
                        {
                            Assert.AreEqual(null, (string)rsp.@out);
                        }

                        Assert.AreNotEqual(UIntPtr.Zero, rsp.@out.results_ptr);

                        var reader = new CJsonReader(rsp.@out);
                        var rawQueryParams = reader.ReadRawQueryParams();

                        Assert.AreEqual(1, rawQueryParams.count);

                        reader.ReadRawItemParams();
                    }
                    finally
                    {
                        dataHandle.Dispose();
                        rsp.@out.Free();
                    }
                });
            });
        }

        Thread.Sleep(6000);
#pragma warning disable S1215 // "GC.Collect" should not be called
        GC.Collect();
        GC.WaitForPendingFinalizers();
#pragma warning restore S1215 // "GC.Collect" should not be called
        AssertError(ReindexerBinding.reindexer_truncate_namespace(_rx, nsName.GetStringHandle(), _ctxInfo));
    }
}
