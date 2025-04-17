using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            },
            new Index
            {
                Name = "Group",
                FieldType = FieldType.Int64,
                IndexType = IndexType.Hash
            },
            new Index
            {
                Name = "NullableInt64",
                FieldType = FieldType.Int64,
                IndexType = IndexType.Hash,
                IsSparse = true
            },
            new Index
            {
                Name = "NullableInt32",
                FieldType = FieldType.Int,
                IndexType = IndexType.Hash,
                IsSparse = true,
                IsDense = true
            },
            new Index
            {
                Name = "NullableArray",
                FieldType = FieldType.String,
                IndexType = IndexType.Hash,
                IsSparse = true,
                IsArray = true
            },
            new Index
            {
                Name = "NullableSortable",
                FieldType = FieldType.Double,
                IndexType = IndexType.Tree,
                IsSparse = true
            },
            new Index
            {
                Name = "NullableColumnIndex",
                FieldType = FieldType.Bool,
                IndexType = IndexType.ColumnIndex,
                IsSparse = true,
                IsDense = true
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

    [DataContract]
    public class TestDocument
    {
        [DataMember(Order = 0)]
        public long Id { get; set; }
        [DataMember(Order = 1)]
        public string Name { get; set; }
        [DataMember(Order = 2)]
        public string[] ArrayIndex { get; set; }
        [DataMember(Order = 3)]
        public double RangeIndex { get; set; }
        [DataMember(Order = 4)]
        public byte[] Payload { get; set; }
        [DataMember(Order = 5)]
        public int SerialPrecept { get; set; } //precepts cant be null
        [DataMember(Order = 6)]
        public long UpdateTime { get; set; } //precepts cant be null
        [DataMember(Order = 7)]
        public string NullablePayload { get; set; }
        [DataMember(Order = 8)]
        public int? NullableIntPayload { get; set; }
        [DataMember(Order = 9)]
        public string? Utf8String { get; set; }
        [DataMember(Order = 10)]
        public int Group { get; set; }
        [DataMember(Order = 11)]
        public long? NullableInt64 { get; set; }
        [DataMember(Order = 12)]
        public string?[]? NullableArray { get; set; }
        [DataMember(Order = 13)]
        public double? NullableSortable { get; set; }
        [DataMember(Order = 14)]
        public bool? NullableColumnIndex { get; set; }
        [DataMember(Order = 15)]
        public int? NullableInt32 { get; set; }
    }

    private async Task<List<TestDocument>> AddItemsAsync(int idStart, int idEnd)
    {
        var entities = Enumerable.Range(idStart, idEnd - idStart).Select(i =>
                  new TestDocument
                  {
                      Id = i,
                      Name = $"Name of {i}",
                      ArrayIndex = Enumerable.Range(0, i).Select(r => $"..{r}..").ToArray(),
                      RangeIndex = i * 0.125,
                      Payload = Enumerable.Range(0, i).Select(r => (byte)(r % 255)).ToArray(),
                      NullablePayload = i % 2 == 0 ? i.ToString() : null,
                      NullableIntPayload = i % 2 == 0 ? i : (int?)null,
                      Utf8String = "ÇŞĞÜÖİöçşğüı" + i,
                      Group = i % 3,
                      NullableInt64 = i % 3 == 0 ? i : (long?)null,
                      NullableInt32 = i % 3 == 0 ? i : (int?)null,
                      NullableArray = i % 3 == 0 ? Enumerable.Range(0, i).Select(r => i % 2 == 0 && r == 0 ? null : $"..{r}..").ToArray() : null,
                      NullableSortable = i % 3 == 0 ? i * 0.125 : (double?)null,
                      NullableColumnIndex = i % 3 == 0 ? i % 2 == 0 : (bool?)null
                  }).ToList();
        var insertedItemCount = await Client.UpsertAsync(NsName, entities);


        Assert.AreEqual(idEnd - idStart, insertedItemCount);

        return entities;
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
            await tran.ModifyItemsAsync(ItemModifyMode.Insert, new[] { new TestDocument { Id = 10500 } });
            Assert.AreEqual(0, (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=10500")).QueryTotalItems);
            tran.Commit();
        }
        Assert.AreEqual(1, (await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id=10500")).QueryTotalItems);

        using (var tran = await Client.StartTransactionAsync(NsName))
        {
            await tran.ModifyItemsAsync(ItemModifyMode.Insert, new[] { new TestDocument { Id = 10501 } });
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
    public async Task QueryBuilderTest_InsertAndQuery()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var docs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Where("Id", Condition.LT, 1000));
        Assert.AreEqual(1000, docs.QueryTotalItems);
        var item = docs.Items.FirstOrDefault(i => i.Id == 2);
        Assert.AreEqual($"Name of 2", item.Name);
        CollectionAssert.AreEqual(new[] { "..0..", "..1.." }, item.ArrayIndex);
        Assert.AreEqual(2 * 0.125, item.RangeIndex);
        CollectionAssert.AreEqual(Enumerable.Range(0, 2).Select(r => (byte)(r % 255)).ToArray(), item.Payload);
    }

    [TestMethod]
    public async Task QueryBuilderTest_NullableSearch()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);
        var nullableInt64s = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.WhereInt64("NullableInt64", Condition.EMPTY));
        CollectionAssert.AreEqual(insertedItems.Where(i => i.NullableInt64 == null).Select(i => i.Id).ToList(), nullableInt64s.Items.Select(i => i.Id).ToList());
        var nullableInt32s = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.WhereInt32("NullableInt32", Condition.EMPTY));
        CollectionAssert.AreEqual(insertedItems.Where(i => i.NullableInt32 == null).Select(i => i.Id).ToList(), nullableInt32s.Items.Select(i => i.Id).ToList());

        var nullableArrays = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Not().WhereString("NullableArray", Condition.ANY));
        CollectionAssert.AreEqual(insertedItems.Where(i => i.NullableArray == null || i.NullableArray.Length == 0).Select(i => i.Id).ToList(), nullableArrays.Items.Select(i => i.Id).ToList());
        //var all = await Client.ExecuteAsync<TestDocument>(NsName,q => { });

        var nullableArrayContainsNulls = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.WhereString("NullableArray", Condition.EMPTY).And().WhereString("NullableArray", Condition.ANY));//WHERE NullableArray IS NULL AND NullableArray IS NOT EMPTY
        CollectionAssert.AreEqual(insertedItems.Where(i => i.NullableArray != null && i.NullableArray.Contains(null)).Select(i => i.Id).ToList(), nullableArrayContainsNulls.Items.Select(i => i.Id).ToList());

        var nullableArrayContainsNullsAndOtherValues = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Where(qq => qq.WhereString("NullableArray", Condition.EMPTY).Or().WhereString("NullableArray", Condition.SET, "..3..", "..4..")).And().WhereString("NullableArray", Condition.ANY));//WHERE (NullableArray IS NULL OR NullableArray IN (3,4)) AND NullableArray IS NOT EMPTY
        HashSet<string> expectedValuesSet = [null, "..3..", "..4.."];
        CollectionAssert.AreEqual(insertedItems.Where(i => i.NullableArray != null && i.NullableArray.Any(a => expectedValuesSet.Contains(a))).Select(i => i.Id).ToList(), nullableArrayContainsNullsAndOtherValues.Items.Select(i => i.Id).ToList());

        
        //var nullableArrayContainsNullsAndAllOtherValues = await Client.ExecuteAsync<TestDocument>(NsName,
        //    q => q.Where(qq => qq.WhereString("NullableArray", Condition.EMPTY).And().WhereString("NullableArray", Condition.ALLSET, "..1..", "..2..")).And().WhereString("NullableArray", Condition.ANY));//WHERE (NullableArray IS NULL OR NullableArray ALLSET (1,2)) AND NullableArray IS NOT EMPTY
        //HashSet<string> expectedValuesAllSet = [null, "..1..", "..2.."];
        //CollectionAssert.AreEqual(insertedItems.Where(i => i.NullableArray != null && i.NullableArray.All(a => expectedValuesAllSet.Contains(a))).Select(i => i.Id).ToList(), nullableArrayContainsNullsAndAllOtherValues.Items.Select(i => i.Id).ToList());

        var nullableSortable = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.WhereDouble("NullableSortable", Condition.EMPTY));
        CollectionAssert.AreEqual(insertedItems.Where(i => i.NullableSortable == null).Select(i => i.Id).ToList(), nullableSortable.Items.Select(i => i.Id).ToList());

        var nullableColumnIndex = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.WhereBool("NullableColumnIndex", Condition.EMPTY));
        CollectionAssert.AreEqual(insertedItems.Where(i => i.NullableColumnIndex == null).Select(i => i.Id).ToList(), nullableColumnIndex.Items.Select(i => i.Id).ToList());
    }

    [TestMethod]
    public async Task QueryBuilderTest_SingleSort()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var singleSortedDocs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Where("Id", Condition.LT, 1000).Sort("Id", true));
        CollectionAssert.AreEqual(insertedItems.Where(i => i.Id < 1000).OrderByDescending(i => i.Id).Select(i => i.Id).ToList(), singleSortedDocs.Items.Select(i => i.Id).ToList());
    }

    [TestMethod]
    public async Task QueryBuilderTest_MultiSort()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var multiSortedDocs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Where("Id", Condition.LT, 1000).Sort("Group", true).Sort("Id", true));
        CollectionAssert.AreEqual(insertedItems.Where(i => i.Id < 1000).OrderByDescending(i => i.Group).ThenByDescending(i => i.Id).Select(i => i.Id).ToList(), multiSortedDocs.Items.Select(i => i.Id).ToList());
    }

    [TestMethod]
    public async Task QueryBuilderTest_ArrayContains()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var arrayItems = Enumerable.Range(0, 500).Select(i => $"..{i}..").ToArray();
        var arrayContainsDocs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.WhereString("ArrayIndex", Condition.ALLSET, arrayItems));
        Assert.AreEqual(1000 - arrayItems.Length, arrayContainsDocs.QueryTotalItems);
    }

    [TestMethod]
    public async Task QueryBuilderTest_RangeQuery()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var rangeQueryDocs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.WhereDouble("RangeIndex", Condition.RANGE, 5.1d, 6d));
        Assert.AreEqual(5.125, rangeQueryDocs.Items.FirstOrDefault()?.RangeIndex);
    }

    [TestMethod]
    public async Task QueryBuilderTest_Utf8Search()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var props = Enumerable.Range(0, 1000).Select(i => "ÇŞĞÜÖİöçşğüı" + i).ToArray();
        var utf8SearrchResult = await Client.ExecuteAsync<TestDocument>(NsName, q => q.WhereString(nameof(TestDocument.Utf8String), Condition.SET, props));
        CollectionAssert.AreEqual(props, utf8SearrchResult.Items.Select(i => i.Utf8String).ToList());
    }

    [TestMethod]
    public async Task QueryBuilderTest_Delete()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var deletedCount = await Client.DeleteAsync(NsName, new[] { new TestDocument { Id = 500 } });
        Assert.AreEqual(1, deletedCount);
        var deletedQ = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.WhereInt("Id", Condition.EQ, 500));
        Assert.AreEqual(0, deletedQ.QueryTotalItems);
    }

    [TestMethod]
    public async Task QueryBuilderTest_SelectMultipleItemsContains()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var selectMultipleItemsContains = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q
                .Explain()
                .Select("Id", "Group")
                .Where("Group", Condition.SET, new object[] { 1 }));
        Assert.AreEqual(333, selectMultipleItemsContains.QueryTotalItems);
    }

    [TestMethod]
    public async Task QueryBuilderTest_AggregateMin()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var aggregateMinQuery = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.AggregateMin("Group"));
        Assert.AreEqual(1, aggregateMinQuery.Aggregations.Count(ag => ag.Type == "min"));
        Assert.AreEqual(0, aggregateMinQuery.Aggregations.First(ag => ag.Type == "min").Value);
    }

    [TestMethod]
    public async Task QueryBuilderTest_AggregateMax()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var aggregateMaxQuery = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.AggregateMax("Group"));
        Assert.AreEqual(1, aggregateMaxQuery.Aggregations.Count(ag => ag.Type == "max"));
        Assert.AreEqual(2, aggregateMaxQuery.Aggregations.First(ag => ag.Type == "max").Value);
    }

    [TestMethod]
    public async Task QueryBuilderTest_AggregateSumAvg()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var docs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Where("Id", Condition.LT, 1000));

        var aggregateSumAvgQuery = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.AggregateSum("Group").AggregateAvg("Group"));
        Assert.AreEqual(1, aggregateSumAvgQuery.Aggregations.Count(ag => ag.Type == "sum"));
        Assert.AreEqual(1, aggregateSumAvgQuery.Aggregations.Count(ag => ag.Type == "avg"));
        Assert.AreEqual(docs.Items.Select(e => e.Group).Sum(), aggregateSumAvgQuery.Aggregations.First(ag => ag.Type == "sum").Value);
    }

    [TestMethod]
    public async Task QueryBuilderTest_AggregateDistinct()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var docs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Where("Id", Condition.LT, 1000));

        var aggregateDistinctQuery = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Distinct("Group"));
        Assert.AreEqual(1, aggregateDistinctQuery.Aggregations.Count(ag => ag.Type == "distinct"));
        CollectionAssert.AreEquivalent(new string[] { "0", "1", "2" }, aggregateDistinctQuery.Aggregations.First(ag => ag.Type == "distinct").Distincts);
    }

    [TestMethod]
    public async Task QueryBuilderTest_AggregateFacet()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var docs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Where("Id", Condition.LT, 1000));

        var aggregateFacetQuery = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q
                .WhereString("ArrayIndex", Condition.SET, Enumerable.Range(1, 100).Select(i => $"..{i}..").ToArray())
                .AggregateFacet(fq =>
                fq.Sort("Group", true).Limit(2),
                "Group"));
        Assert.AreEqual(1, aggregateFacetQuery.Aggregations.Count(ag => ag.Type == "facet" && ag.Fields.SequenceEqual(["Group"])));
        Assert.AreEqual(2, aggregateFacetQuery.Aggregations.First(ag => ag.Type == "facet" && ag.Fields.SequenceEqual(["Group"])).Facets.Count);
        CollectionAssert.AreEquivalent(new string[] { "2" }, aggregateFacetQuery.Aggregations.First(ag => ag.Type == "facet" && ag.Fields.SequenceEqual(["Group"])).Facets.First().Values);
    }

    [TestMethod]
    public async Task QueryBuilderTest_Join()
    {
        await AddIndexesAsync();
        var insertedItems = await AddItemsAsync(0, 1000);

        var docs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Where("Id", Condition.LT, 1000));

        var joinQuery = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Limit(10).LeftJoin(NsName, j => j.Limit(1), "JoinedByGroup").On(nameof(TestDocument.Group), Condition.EQ, nameof(TestDocument.Group))
            );
        Assert.AreEqual(10, joinQuery.QueryTotalItems);
    }

    [TestMethod]
    public async Task QueryBuilderTest_AggregateMultipleWhere()
    {
        await AddIndexesAsync();
        await AddItemsAsync(0, 1000);

        var docs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.Where("Id", Condition.LT, 1000));

        var arrayLookup = Enumerable.Range(1, 100).Select(i => $"..{i}..").ToArray();

        var aggregateMultipleWhereQuery = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q
                .Where(q1 => q1.WhereString("ArrayIndex", Condition.SET, arrayLookup))
                .Where(q2 => q2.WhereDouble("RangeIndex", Condition.GT, 50))                
                .AggregateFacet(fq =>
                    fq.Sort("Group", true).Limit(10),
                    "Group", "Id"));

        var expectedDocs = docs.Items
            .Where(d => d.ArrayIndex.Any(ai => arrayLookup.Contains(ai)) &&
                        d.RangeIndex > 50)
            .GroupBy(d => d.Group)            
            .OrderByDescending(g => g.Key)
            .SelectMany(g => g.Select(f => new long[]{f.Group, f.Id }))
            .Take(10)
            .ToList();

        CollectionAssert.AreEqual(expectedDocs.SelectMany(a => a.Select(i => i.ToString())).ToList(), aggregateMultipleWhereQuery.Aggregations.SelectMany(ag => ag.Facets.SelectMany(f => f.Values)).ToList());
    }

    [TestMethod]
    public async Task LargeNumericTypes()
    {
        await AddIndexesAsync();
        await Client.UpsertAsync(NsName, [
            new TestDocument
            {
                Id = 1,
                Name = "1",
                ArrayIndex = new[] { "1" },
                RangeIndex = double.MaxValue,
                Payload = new byte[] { 1 },
                SerialPrecept = 1,
                UpdateTime = 1,
                Utf8String = "1",
                Group = 1,
                NullableInt64 = long.MaxValue,
                NullableInt32 = int.MaxValue,
                NullableArray = new[] { "1" },
                NullableSortable = 1,
                NullableColumnIndex = true
            },
            new TestDocument
            {
                Id = 2,
                Name = "1",
                ArrayIndex = new[] { "1" },
                RangeIndex = 18014398499999998d,
                Payload = new byte[] { 1 },
                SerialPrecept = 1,
                UpdateTime = 1,
                Utf8String = "1",
                Group = 1,
                NullableInt64 = long.MaxValue - 1,
                NullableInt32 = int.MaxValue - 1,
                NullableArray = new[] { "1" },
                NullableSortable = 1,
                NullableColumnIndex = true
            },
            ]);
        
        var docs = await Client.ExecuteSqlAsync<TestDocument>($"SELECT * FROM {NsName} WHERE Id IN (1,2)");
        Assert.AreEqual(double.MaxValue.ToString("F"), docs.Items.First(i => i.Id == 1).RangeIndex.ToString("F"));//delta didin't work so we need to compare as string
        Assert.AreEqual(long.MaxValue, docs.Items.First(i => i.Id == 1).NullableInt64);
        Assert.AreEqual(int.MaxValue, docs.Items.First(i => i.Id == 1).NullableInt32);
        Assert.AreEqual(18014398499999998d.ToString("F"), docs.Items.First(i => i.Id == 2).RangeIndex.ToString("F"));
        Assert.AreEqual(long.MaxValue-1, docs.Items.First(i => i.Id == 2).NullableInt64);
        Assert.AreEqual(int.MaxValue-1, docs.Items.First(i => i.Id == 2).NullableInt32);

        docs = await Client.ExecuteAsync<TestDocument>(NsName,
            q => q.WhereInt64("Id", Condition.SET, 1,2));
        Assert.AreEqual(double.MaxValue.ToString("F"), docs.Items.First(i => i.Id == 1).RangeIndex.ToString("F"));
        Assert.AreEqual(long.MaxValue, docs.Items.First(i => i.Id == 1).NullableInt64);
        Assert.AreEqual(int.MaxValue, docs.Items.First(i => i.Id == 1).NullableInt32);
        Assert.AreEqual(18014398499999998d.ToString("F"), docs.Items.First(i => i.Id == 2).RangeIndex.ToString("F"));
        Assert.AreEqual(long.MaxValue-1, docs.Items.First(i => i.Id == 2).NullableInt64);
        Assert.AreEqual(int.MaxValue-1, docs.Items.First(i => i.Id == 2).NullableInt32);
    }
}