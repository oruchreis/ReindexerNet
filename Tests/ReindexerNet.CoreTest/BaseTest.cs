using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReindexerNet.CoreTest;

[TestClass]
public abstract class BaseTest<TClient>
    where TClient : IAsyncReindexerClient
{
    private readonly bool _testModifiedItemCount;
    private readonly bool _testPrecepts;

    protected abstract TClient Client { get; set; }
    protected abstract string NsName { get; set; }
    protected abstract string DbPath { get; set; }
    protected virtual StorageEngine Storage => StorageEngine.LevelDb;

    public TestContext TestContext { get; set; }

    public BaseTest(bool testModifedItemCount, bool testPrecepts)
    {
        _testModifiedItemCount = testModifedItemCount;
        _testPrecepts = testPrecepts;
    }

    [TestMethod()]
    public async Task PingAsyncTest()
    {
        await Client.PingAsync();
    }

    [TestMethod()]
    public async Task OpenDropEnumNamespaceTest()
    {
        await Client.OpenNamespaceAsync(nameof(OpenDropEnumNamespaceTest), new NamespaceOptions { CreateIfMissing = true });
        var namespaces = await Client.EnumNamespacesAsync();
        CollectionAssert.Contains(namespaces.Select(ns => ns.Name).ToList(), nameof(OpenDropEnumNamespaceTest));
        await Client.DropNamespaceAsync(nameof(OpenDropEnumNamespaceTest));
        namespaces = await Client.EnumNamespacesAsync();
        CollectionAssert.DoesNotContain(namespaces.Select(ns => ns.Name).ToList(), nameof(OpenDropEnumNamespaceTest));
    }

    private async Task AddIndexesAsync()
    {
        foreach (var index in new[]{
            new Index
            {
                Name = "Id",
                IsPk = true,
                FieldType = FieldType.Int64,
                IsDense = true,
                IndexType = IndexType.Hash
            },
            new Index
            {
                Name = "Name",
                IsDense = true,
                FieldType = FieldType.String,
                IndexType = IndexType.Hash
            },
            new Index
            {
                Name = "ArrayIndex",
                IsArray = true,
                IsDense = true,
                FieldType = FieldType.String,
                IndexType = IndexType.Hash
            },
            new Index
            {
                Name = "RangeIndex",
                FieldType = FieldType.Double,
                IndexType = IndexType.Tree
            },
            new Index
            {
                Name = "SerialPrecept",
                FieldType = FieldType.Int,
                IndexType = IndexType.Tree
            },
            new Index
            {
                Name = "UpdateTime",
                FieldType = FieldType.Int64,
                IndexType = IndexType.Tree
            },
            new Index
            {
                Name = "Utf8String",
                FieldType = FieldType.String,
                IndexType = IndexType.Hash
            }
            })
        {
            await Client.AddIndexAsync(NsName, index);
        }            

        var nsInfo = (await Client.ExecuteSqlAsync<Namespace>($"SELECT * FROM #namespaces WHERE name='{NsName}' LIMIT 1")).Items.FirstOrDefault();
        Assert.IsNotNull(nsInfo);
        Assert.AreEqual(NsName, nsInfo.Name);
        var indexNames = nsInfo.Indexes.Select(i => i.Name).ToList();
        CollectionAssert.Contains(indexNames, "Id");
        CollectionAssert.Contains(indexNames, "Name");
        CollectionAssert.Contains(indexNames, "ArrayIndex");
        CollectionAssert.Contains(indexNames, "RangeIndex");
    }

    public class TestDocument
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string[] ArrayIndex { get; set; }
        public double RangeIndex { get; set; }
        public byte[] Payload { get; set; }
        public int SerialPrecept { get; set; } //precepts cant be null
        public long UpdateTime { get; set; } //precepts cant be null
        public string NullablePayload { get; set; }
        public int? NullableIntPayload { get; set; }
        public string? Utf8String { get; set; }
    }

    private async Task AddItemsAsync(int idStart, int idEnd)
    {
        var insertedItemCount = await Client.UpsertAsync(NsName, Enumerable.Range(idStart, idEnd - idStart).Select(i =>
                  new TestDocument
                  {
                      Id = i,
                      Name = $"Name of {i}",
                      ArrayIndex = Enumerable.Range(0, i).Select(r => $"..{r}..").ToArray(),
                      RangeIndex = i * 0.125,
                      Payload = Enumerable.Range(0, i).Select(r => (byte)(r % 255)).ToArray(),
                      NullablePayload = i % 2 == 0 ? i.ToString() : null,
                      NullableIntPayload = i % 2 == 0 ? i : (int?)null,
                      Utf8String = "ÇŞĞÜÖİöçşğüı" + i
                  }));


        Assert.AreEqual(idEnd - idStart, insertedItemCount);
    }

    [TestMethod]
    public async Task ErrorTest()
    {
        var excp = await Assert.ThrowsExceptionAsync<ReindexerException>(async () =>
        {
            await Client.DropNamespaceAsync(Guid.NewGuid().ToString());
        });

        Assert.AreNotEqual(ReindexerErrorCode.OK, excp?.ErrorCode);
    }

    [TestMethod]
    public virtual async Task ExecuteQueryJson()
    {
        await AddIndexesAsync();

        await AddItemsAsync(0, 1000);

        var docs = await Client.ExecuteAsync<TestDocument>(Encoding.UTF8.GetBytes($$"""
        {
            "namespace": "{{NsName}}",
            "req_total": "enabled",
            "filters": [
            {
                "field": "Id",
                "cond": "LT",
                "value": [
                "1000"
                ]
            }
            ],                  
            "explain": false,
            "select_filter": [],
            "strict_mode": "names"
        }                
        """), SerializerType.Json);
        Assert.AreEqual(1000, docs.QueryTotalItems);
        var item = docs.Items.FirstOrDefault(i => i.Id == 2);
        Assert.AreEqual($"Name of 2", item.Name);
        CollectionAssert.AreEqual(new[] { "..0..", "..1.." }, item.ArrayIndex);
        Assert.AreEqual(2 * 0.125, item.RangeIndex);
        CollectionAssert.AreEqual(Enumerable.Range(0, 2).Select(r => (byte)(r % 255)).ToArray(), item.Payload);
    }

    [TestMethod]
    public async Task ExecuteSql()
    {
        await AddIndexesAsync();

        await AddItemsAsync(0, 1000);

        var docs = await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id < 1000");
        Assert.AreEqual(1000, docs.QueryTotalItems);
        var item = docs.Items.FirstOrDefault(i => i.Id == 2);
        Assert.AreEqual($"Name of 2", item.Name);
        CollectionAssert.AreEqual(new[] { "..0..", "..1.." }, item.ArrayIndex);
        Assert.AreEqual(2 * 0.125, item.RangeIndex);
        CollectionAssert.AreEqual(Enumerable.Range(0, 2).Select(r => (byte)(r % 255)).ToArray(), item.Payload);

        var arrayContainsDocs = await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE ArrayIndex IN ('..997..','..998..')");
        Assert.AreEqual(2, arrayContainsDocs.QueryTotalItems);

        var rangeQueryDocs = await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE RangeIndex > 5.1 AND RangeIndex < 6");
        Assert.AreEqual(5.125, rangeQueryDocs.Items.FirstOrDefault()?.RangeIndex);

        var deletedCount = await Client.DeleteAsync(NsName, new[] { new TestDocument { Id = 500 } });
        Assert.AreEqual(1, deletedCount);
        var deletedQ = await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=500");
        Assert.AreEqual(0, deletedQ.QueryTotalItems);

        var notExistDeletedCount = await Client.DeleteAsync(NsName, new[] { new TestDocument { Id = 500 } });
        if (_testModifiedItemCount)//Grpc api doesn't support modified item count, only handled count returned.
            Assert.AreEqual(0, notExistDeletedCount);

        var multipleDeletedCount = await Client.ExecuteSqlAsync($"DELETE FROM {NsName} WHERE Id > 501");
        Assert.AreEqual(498, multipleDeletedCount.QueryTotalItems);

        if (_testPrecepts)
        {
            var preceptQueryCount = await Client.UpdateAsync(NsName, new[] { new TestDocument { Id = 1, Name = "Updated" } },
            precepts: new[] { "SerialPrecept=SERIAL()", "UpdateTime=NOW(msec)" });
            Assert.AreEqual(1, preceptQueryCount);
            var preceptQ = (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=1")).Items.First();
            Assert.AreEqual("Updated", preceptQ.Name);
            Assert.AreNotEqual(0, preceptQ.SerialPrecept);
            Assert.AreNotEqual(0, preceptQ.UpdateTime);
        }

        var nullableStringQ = (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=100")).Items.First();
        Assert.AreEqual("100", nullableStringQ.NullablePayload);
        var nullableStringQ2 = (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=101")).Items.First();
        Assert.AreEqual(null, nullableStringQ2.NullablePayload);

        var nullableIntQ = (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=100")).Items.First();
        Assert.AreEqual(100, nullableIntQ.NullableIntPayload);
        var nullableIntQ2 = (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=101")).Items.First();
        Assert.AreEqual(null, nullableIntQ2.NullableIntPayload);
    }

    [TestMethod]
    public async Task Transaction()
    {
        await AddIndexesAsync();

        using (var tran = await Client.StartTransactionAsync(NsName))
        {
            await tran.ModifyItemsAsync(ItemModifyMode.Insert, new[] { new TestDocument { Id = 10500 } } );
            Assert.AreEqual(0, (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=10500")).QueryTotalItems);
            tran.Commit();
        }
        Assert.AreEqual(1, (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=10500")).QueryTotalItems);

        using (var tran = await Client.StartTransactionAsync(NsName))
        {
            await tran.ModifyItemsAsync(ItemModifyMode.Insert, new[] { new TestDocument { Id = 10501 } } );
            await tran.RollbackAsync();
        }
        Assert.AreEqual(0, (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=10501")).QueryTotalItems);

        try
        {
            using (var tran = await Client.StartTransactionAsync(NsName))
            {
                await tran.ModifyItemsAsync(ItemModifyMode.Insert, new[] { new TestDocument { Id = 10502 } });
                throw new Exception();
            }
        }
        catch
        {
            //ignored because testing rollback
        }
        Assert.AreEqual(0, (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=10502")).QueryTotalItems);
    }

    [TestMethod]
    public async Task Utf8Test()
    {
        await AddIndexesAsync();

        var utf8Str = "İŞĞÜÇÖışğüöç   بِسْــــــــــــــــــــــمِ اﷲِارَّحْمَنِ ارَّحِيم";
        await Client.UpsertAsync(NsName,
                new[]{new TestDocument
                {
                    Id = 10001,
                    Name = utf8Str
                } });

        var itemWithPayloadUtf8 = (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=10001")).Items.FirstOrDefault();
        Assert.AreEqual(utf8Str, itemWithPayloadUtf8?.Name);

        var itemWithQueryUtf8 = (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Name='{utf8Str}'")).Items.FirstOrDefault();
        Assert.AreEqual(utf8Str, itemWithQueryUtf8?.Name);
    }

    [TestMethod]
    public async Task QueryBuilderTest()
    {
        await AddIndexesAsync();

        await AddItemsAsync(0, 1000);

        var docs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Where("Id", Condition.LT ,1000));
        Assert.AreEqual(1000, docs.QueryTotalItems);
        var item = docs.Items.FirstOrDefault(i => i.Id == 2);
        Assert.AreEqual($"Name of 2", item.Name);
        CollectionAssert.AreEqual(new[] { "..0..", "..1.." }, item.ArrayIndex);
        Assert.AreEqual(2 * 0.125, item.RangeIndex);
        CollectionAssert.AreEqual(Enumerable.Range(0, 2).Select(r => (byte)(r % 255)).ToArray(), item.Payload);

        var arrayItems = Enumerable.Range(0,500).Select(i => $"..{i}..").ToArray();
        var arrayContainsDocs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.WhereString("ArrayIndex", Condition.ALLSET, arrayItems));
        Assert.AreEqual(1000-arrayItems.Length, arrayContainsDocs.QueryTotalItems);

        var rangeQueryDocs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.WhereDouble("RangeIndex", Condition.RANGE, 5.1d, 6d));
        Assert.AreEqual(5.125, rangeQueryDocs.Items.FirstOrDefault()?.RangeIndex);

        var props = Enumerable.Range(0,1000).Select(i => "ÇŞĞÜÖİöçşğüı" + i).ToArray();
        var utf8SearrchResult = await Client.ExecuteAsync<TestDocument>(NsName, q => q.WhereString(nameof(TestDocument.Utf8String), Condition.SET, props));
        CollectionAssert.AreEqual(props, utf8SearrchResult.Items.Select(i => i.Utf8String).ToList());

        var deletedCount = await Client.DeleteAsync(NsName, new[] { new TestDocument { Id = 500 } });
        Assert.AreEqual(1, deletedCount);
        var deletedQ = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.WhereInt("Id", Condition.EQ, 500));
        Assert.AreEqual(0, deletedQ.QueryTotalItems);
    }
}