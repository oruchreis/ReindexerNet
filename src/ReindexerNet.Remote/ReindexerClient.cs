using System;
using System.Threading.Tasks;

namespace ReindexerNet.Remote
{
    public class ReindexerClient : IReindexerClient
    {
        public void AddIndex(string nsName, params Index[] indexDefinitions)
        {
            throw new NotImplementedException();
        }

        public Task AddIndexAsync(string nsName, params Index[] indexDefinitions)
        {
            throw new NotImplementedException();
        }

        public void CloseNamespace(string nsName)
        {
            throw new NotImplementedException();
        }

        public Task CloseNamespaceAsync(string nsName)
        {
            throw new NotImplementedException();
        }

        public void Connect(string connectionString, ConnectionOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task ConnectAsync(string connectionString, ConnectionOptions options = null)
        {
            throw new NotImplementedException();
        }

        public int Delete<TItem>(string nsName, TItem item, params string[] precepts)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteAsync<TItem>(string nsName, TItem item, params string[] precepts)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void DropIndex(string nsName, params string[] indexName)
        {
            throw new NotImplementedException();
        }

        public Task DropIndexAsync(string nsName, params string[] indexName)
        {
            throw new NotImplementedException();
        }

        public void DropNamespace(string nsName)
        {
            throw new NotImplementedException();
        }

        public Task DropNamespaceAsync(string nsName)
        {
            throw new NotImplementedException();
        }

        public QueryItemsOf<TItem> ExecuteSql<TItem>(string sql, Func<byte[], TItem> deserializeItem)
        {
            throw new NotImplementedException();
        }

        public QueryItemsOf<TItem> ExecuteSql<TItem>(string sql)
        {
            throw new NotImplementedException();
        }

        public QueryItemsOf<byte[]> ExecuteSql(string sql)
        {
            throw new NotImplementedException();
        }

        public Task<QueryItemsOf<TItem>> ExecuteSqlAsync<TItem>(string sql, Func<byte[], TItem> deserializeItem)
        {
            throw new NotImplementedException();
        }

        public Task<QueryItemsOf<TItem>> ExecuteSqlAsync<TItem>(string sql)
        {
            throw new NotImplementedException();
        }

        public Task<QueryItemsOf<byte[]>> ExecuteSqlAsync(string sql)
        {
            throw new NotImplementedException();
        }

        public int Insert<TItem>(string nsName, TItem item, params string[] precepts)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertAsync<TItem>(string nsName, TItem item, params string[] precepts)
        {
            throw new NotImplementedException();
        }

        public int ModifyItem(string nsName, ItemModifyMode mode, string itemJson, params string[] precepts)
        {
            throw new NotImplementedException();
        }

        public Task<int> ModifyItemAsync(string nsName, ItemModifyMode mode, string itemJson, params string[] precepts)
        {
            throw new NotImplementedException();
        }

        public void OpenNamespace(string nsName, NamespaceOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task OpenNamespaceAsync(string nsName, NamespaceOptions options = null)
        {
            throw new NotImplementedException();
        }

        public void Ping()
        {
            throw new NotImplementedException();
        }

        public Task PingAsync()
        {
            throw new NotImplementedException();
        }

        public void RenameNamespace(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public Task RenameNamespaceAsync(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public ReindexerTransaction StartTransaction(string nsName)
        {
            throw new NotImplementedException();
        }

        public Task<ReindexerTransaction> StartTransactionAsync(string nsName)
        {
            throw new NotImplementedException();
        }

        public void TruncateNamespace(string nsName)
        {
            throw new NotImplementedException();
        }

        public Task TruncateNamespaceAsync(string nsName)
        {
            throw new NotImplementedException();
        }

        public int Update<TItem>(string nsName, TItem item, params string[] precepts)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync<TItem>(string nsName, TItem item, params string[] precepts)
        {
            throw new NotImplementedException();
        }

        public void UpdateIndex(string nsName, params Index[] indexDefinitions)
        {
            throw new NotImplementedException();
        }

        public Task UpdateIndexAsync(string nsName, params Index[] indexDefinitions)
        {
            throw new NotImplementedException();
        }

        public int Upsert<TItem>(string nsName, TItem item, params string[] precepts)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpsertAsync<TItem>(string nsName, TItem item, params string[] precepts)
        {
            throw new NotImplementedException();
        }
    }
}