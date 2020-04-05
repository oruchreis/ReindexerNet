using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReindexerNet.Embedded;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ReindexerNet.EmbeddedTest
{
    [TestClass]
    public class ClientTest
    {
        private IReindexerClient _client;
        private readonly string _nsName = nameof(ClientTest);

        [TestInitialize]
        public void Init()
        {
            _client = new ReindexerEmbedded();
            _client.Connect("ReindexerEmbeddedClientTest");
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client.Dispose();
        }

        private void AddIndexes()
        {
            _client.AddIndex(_nsName,
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

            var nsInfo = _client.ExecuteSql<Namespace>($"SELECT * FROM #namespaces WHERE name=\"{_nsName}\" LIMIT 1").Items.FirstOrDefault();
            Assert.IsNotNull(nsInfo);
            Assert.AreEqual(_nsName, nsInfo.Name);
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

        private void AddItems(int idStart, int idEnd)
        {
            var insertedItemCount = 0;
            for (int i = idStart; i < idEnd; i++)
            {
                insertedItemCount += _client.Upsert(_nsName,
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
        public void ExecuteSql()
        {
            _client.OpenNamespace(_nsName);
            _client.TruncateNamespace(_nsName);
            AddIndexes();

            AddItems(0, 1000);

            var docs = _client.ExecuteSql<TestDocument>($"SELECT * FROM {_nsName} WHERE Id < 1000");
            Assert.AreEqual(1000, docs.QueryTotalItems);
            var item = docs.Items.FirstOrDefault(i => i.Id == 2);
            Assert.AreEqual($"Name of 2", item.Name);
            CollectionAssert.AreEqual(new[] { "..0..", "..1.." }, item.ArrayIndex);
            Assert.AreEqual(2 * 0.125, item.RangeIndex);
            CollectionAssert.AreEqual(Enumerable.Range(0, 2).Select(r => (byte)(r % 255)).ToArray(), item.Payload);

            var arrayContainsDocs = _client.ExecuteSql<TestDocument>($"SELECT * FROM {_nsName} WHERE ArrayIndex IN (\"..997..\",\"..998..\")");
            Assert.AreEqual(2, arrayContainsDocs.QueryTotalItems);

            var rangeQueryDocs = _client.ExecuteSql<TestDocument>($"SELECT * FROM {_nsName} WHERE RangeIndex > 5.1 AND RangeIndex < 6");
            Assert.AreEqual(5.125, rangeQueryDocs.Items.FirstOrDefault()?.RangeIndex);

            var deletedCount = _client.Delete(_nsName, new TestDocument { Id = 500 });
            Assert.AreEqual(1, deletedCount);
            var deletedQ = _client.ExecuteSql<TestDocument>($"SELECT * FROM {_nsName} WHERE Id=500");
            Assert.AreEqual(0, deletedQ.QueryTotalItems);
            var notExistDeletedCount = _client.Delete(_nsName, new TestDocument { Id = 500 });
            Assert.AreEqual(0, notExistDeletedCount);

            var multipleDeletedCount = _client.ExecuteSql($"DELETE FROM {_nsName} WHERE Id > 501");
            Assert.AreEqual(498, multipleDeletedCount.QueryTotalItems);

            var preceptQueryCount = _client.Update(_nsName, new TestDocument { Id = 1, Name = "Updated" }, "SerialPrecept=SERIAL()", "UpdateTime=NOW(msec)");
            Assert.AreEqual(1, preceptQueryCount);
            var preceptQ = _client.ExecuteSql<TestDocument>($"SELECT * FROM {_nsName} WHERE Id=1").Items.First();
            Assert.AreEqual("Updated", preceptQ.Name);
            Assert.AreNotEqual(0, preceptQ.SerialPrecept);
            Assert.AreNotEqual(0, preceptQ.UpdateTime);

            var nullableStringQ = _client.ExecuteSql<TestDocument>($"SELECT * FROM {_nsName} WHERE Id=100").Items.First();
            Assert.AreEqual("100", nullableStringQ.NullablePayload);
            var nullableStringQ2 = _client.ExecuteSql<TestDocument>($"SELECT * FROM {_nsName} WHERE Id=101").Items.First();
            Assert.AreEqual(null, nullableStringQ2.NullablePayload);

            var nullableIntQ = _client.ExecuteSql<TestDocument>($"SELECT * FROM {_nsName} WHERE Id=100").Items.First();
            Assert.AreEqual(100, nullableIntQ.NullableIntPayload);
            var nullableIntQ2 = _client.ExecuteSql<TestDocument>($"SELECT * FROM {_nsName} WHERE Id=101").Items.First();
            Assert.AreEqual(null, nullableIntQ2.NullableIntPayload);
        }
    }
}
