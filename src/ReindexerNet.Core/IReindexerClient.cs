using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet
{
    public interface IReindexerClient
    {
        void Connect(string connectionString, ConnectionOptions options);
        void Ping();
        void OpenNamespace(string nsName, NamespaceOptions options);
        void DropNamespace(string nsName);
        void CloseNamespace(string nsName);
        void TruncateNamespace(string nsName);
        void RenameNamespace(string oldName, string newName);
        void AddIndex(string nsName, params Index[] indexDefinitions);
        void UpdateIndex(string nsName, params Index[] indexDefinitions);
        void DropIndex(string nsName, params string[] indexName);
        ReindexerTransaction StartTransaction(string nsName);
        int ModifyItem(string nsName, ItemModifyMode mode, string itemJson, params string[] precepts);
        QueryItemsOf<T> ExecuteSql<T>(string sql);
        QueryItemsOf<byte[]> ExecuteSql(string sql);

        Task ConnectAsync(string connectionString, ConnectionOptions options);
        Task PingAsync();
        Task OpenNamespaceAsync(string nsName, NamespaceOptions options);
        Task DropNamespaceAsync(string nsName);
        Task CloseNamespaceAsync(string nsName);
        Task TruncateNamespaceAsync(string nsName);
        Task RenameNamespaceAsync(string oldName, string newName);
        Task AddIndexAsync(string nsName, params Index[] indexDefinitions);
        Task UpdateIndexAsync(string nsName, params Index[] indexDefinitions);
        Task DropIndexAsync(string nsName, params string[] indexName);
        Task<ReindexerTransaction> StartTransactionAsync(string nsName);
        Task<int> ModifyItemAsync(string nsName, ItemModifyMode mode, string itemJson, params string[] precepts);
        Task<QueryItemsOf<T>> ExecuteSqlAsync<T>(string sql);
        Task<QueryItemsOf<byte[]>> ExecuteSqlAsync(string sql);
    }
}
