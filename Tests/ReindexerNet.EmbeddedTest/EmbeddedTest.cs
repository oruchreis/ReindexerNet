using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReindexerNet.Embedded;
using System;
using System.Linq;
using System.Threading.Tasks;
using Utf8Json;

namespace ReindexerNet.EmbeddedTest
{
    [TestClass]
    public class EmbeddedTest
    {
        protected virtual IReindexerClient Client { get; set; }
        protected virtual string NsName { get; set; } = nameof(EmbeddedTest);

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public virtual async Task InitAsync()
        {
            Client = new ReindexerEmbedded();
            ReindexerEmbedded.EnableLogger(Log);
            await Client.ConnectAsync("ReindexerEmbeddedClientTest");
            await Client.OpenNamespaceAsync(NsName);
            await Client.TruncateNamespaceAsync(NsName);
        }

        void Log(LogLevel level, string msg)
        {
            if (level <= LogLevel.Info)
                TestContext.WriteLine("{0}: {1}", level, msg);
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            Client.Dispose();
        }

        private async Task AddIndexesAsync()
        {
            await Client.AddIndexAsync(NsName,
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
                }
                );

            var nsInfo = (await Client.ExecuteSqlAsync<Namespace>($"SELECT * FROM #namespaces WHERE name=\"{NsName}\" LIMIT 1")).Items.FirstOrDefault();
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
        }

        private async Task AddItemsAsync(int idStart, int idEnd)
        {
            var insertedItemCount = 0;
            for (int i = idStart; i < idEnd; i++)
            {
                insertedItemCount += await Client.UpsertAsync(NsName,
                    new TestDocument
                    {
                        Id = i,
                        Name = $"Name of {i}",
                        ArrayIndex = Enumerable.Range(0, i).Select(r => $"..{r}..").ToArray(),
                        RangeIndex = i * 0.125,
                        Payload = Enumerable.Range(0, i).Select(r => (byte)(r % 255)).ToArray(),
                        NullablePayload = i % 2 == 0 ? i.ToString() : null,
                        NullableIntPayload = i % 2 == 0 ? i : (int?)null
                    });
            }

            Assert.AreEqual(idEnd - idStart, insertedItemCount);
        }

        [TestMethod]
        public void ErrorTest()
        {
            var excp = Assert.ThrowsException<ReindexerException>(() =>
            {
                Client.DropNamespace(Guid.NewGuid().ToString());
            });

            Assert.AreNotEqual(ReindexerErrorCode.OK, excp?.ErrorCode);
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

            var arrayContainsDocs = Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE ArrayIndex IN (\"..997..\",\"..998..\")");
            Assert.AreEqual(2, arrayContainsDocs.QueryTotalItems);

            var rangeQueryDocs = Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE RangeIndex > 5.1 AND RangeIndex < 6");
            Assert.AreEqual(5.125, rangeQueryDocs.Items.FirstOrDefault()?.RangeIndex);

            var deletedCount = await Client.DeleteAsync(NsName, new TestDocument { Id = 500 });
            Assert.AreEqual(1, deletedCount);
            var deletedQ = Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE Id=500");
            Assert.AreEqual(0, deletedQ.QueryTotalItems);
            var notExistDeletedCount = Client.Delete(NsName, new TestDocument { Id = 500 });
            Assert.AreEqual(0, notExistDeletedCount);

            var multipleDeletedCount = Client.ExecuteSql($"DELETE FROM {NsName} WHERE Id > 501");
            Assert.AreEqual(498, multipleDeletedCount.QueryTotalItems);

            var preceptQueryCount = await Client.UpdateAsync(NsName, new TestDocument { Id = 1, Name = "Updated" }, "SerialPrecept=SERIAL()", "UpdateTime=NOW(msec)");
            Assert.AreEqual(1, preceptQueryCount);
            var preceptQ = Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE Id=1").Items.First();
            Assert.AreEqual("Updated", preceptQ.Name);
            Assert.AreNotEqual(0, preceptQ.SerialPrecept);
            Assert.AreNotEqual(0, preceptQ.UpdateTime);

            var nullableStringQ = Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE Id=100").Items.First();
            Assert.AreEqual("100", nullableStringQ.NullablePayload);
            var nullableStringQ2 = Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE Id=101").Items.First();
            Assert.AreEqual(null, nullableStringQ2.NullablePayload);

            var nullableIntQ = Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE Id=100").Items.First();
            Assert.AreEqual(100, nullableIntQ.NullableIntPayload);
            var nullableIntQ2 = Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE Id=101").Items.First();
            Assert.AreEqual(null, nullableIntQ2.NullableIntPayload);
        }

        [TestMethod]
        public async Task Transaction()
        {
            await AddIndexesAsync();

            using (var tran = await Client.StartTransactionAsync(NsName))
            {
                tran.ModifyItem(ItemModifyMode.Insert, JsonSerializer.Serialize(new TestDocument { Id = 10500 }));
                Assert.AreEqual(0, Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE Id=10500").QueryTotalItems);
                tran.Commit();
            }
            Assert.AreEqual(1, Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE Id=10500").QueryTotalItems);

            using (var tran = Client.StartTransaction(NsName))
            {
                await tran.ModifyItemAsync(ItemModifyMode.Insert, JsonSerializer.Serialize(new TestDocument { Id = 10501 }));
                await tran.RollbackAsync();
            }
            Assert.AreEqual(0, Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE Id=10501").QueryTotalItems);

            try
            {
                using (var tran = Client.StartTransaction(NsName))
                {
                    tran.ModifyItem(ItemModifyMode.Insert, JsonSerializer.Serialize(new TestDocument { Id = 10502 }));
                    throw new Exception();
                }
            }
            catch
            {
                //ignored because testing rollback
            }
            Assert.AreEqual(0, Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE Id=10502").QueryTotalItems);
        }

        [TestMethod]
        public async Task Utf8Test()
        {
            var utf8Str = "İŞĞÜÇÖışğüöç   بِسْــــــــــــــــــــــمِ اﷲِارَّحْمَنِ ارَّحِيم";
            await Client.UpsertAsync(NsName,
                    new TestDocument
                    {
                        Id = 10001,
                        Name = utf8Str
                    });

            var itemWithPayloadUtf8 = Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE Id=10001").Items.FirstOrDefault();
            Assert.AreEqual(utf8Str, itemWithPayloadUtf8?.Name);

            var itemWithQueryUtf8 = Client.ExecuteSql<TestDocument>($"SELECT * FROM {NsName} WHERE Name=\"{utf8Str}\"").Items.FirstOrDefault();
            Assert.AreEqual(utf8Str, itemWithQueryUtf8?.Name);
        }
    }
}
