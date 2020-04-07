using System;
using System.Threading.Tasks;

namespace ReindexerNet
{
    /// <summary>
    /// Common interface for Reindexer operations.
    /// </summary>
    public interface IReindexerClient : IDisposable
    {
        /// <summary>
        /// Connects to a reindexer implementation. This is first method to call before starting to use Reindexer.
        /// </summary>
        /// <param name="connectionString">Connection string for the implementation.</param>
        /// <param name="options">Reindexer connection options.</param>
        void Connect(string connectionString, ConnectionOptions options = null);
        /// <summary>
        /// Pings the server. Does nothing on embedded mode.
        /// </summary>
        void Ping();
        /// <summary>
        /// Opens a namespace. If it is missing, it wll be created.
        /// </summary>
        /// <param name="nsName">Namespace name</param>
        /// <param name="options">Reindexer namespace options.</param>
        void OpenNamespace(string nsName, NamespaceOptions options = null);
        /// <summary>
        /// Drops namaspace
        /// </summary>
        /// <param name="nsName">Namespace to dtop</param>
        void DropNamespace(string nsName);
        /// <summary>
        /// Closes namespace and unallocate the memory that it used by.
        /// </summary>
        /// <param name="nsName"></param>
        void CloseNamespace(string nsName);
        /// <summary>
        /// Deletes all items in the namespace.
        /// </summary>
        /// <param name="nsName"></param>
        void TruncateNamespace(string nsName);
        /// <summary>
        /// Renames namespace.
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        void RenameNamespace(string oldName, string newName);
        /// <summary>
        /// Creates new index definitions.
        /// </summary>
        /// <param name="nsName">Namespace to add indexes</param>
        /// <param name="indexDefinitions">Index definitions to create</param>
        void AddIndex(string nsName, params Index[] indexDefinitions);
        /// <summary>
        /// Updates current index definitions in the namespace.
        /// </summary>
        /// <param name="nsName">Namespace that have the indexes.</param>
        /// <param name="indexDefinitions">Index definitions to update</param>
        void UpdateIndex(string nsName, params Index[] indexDefinitions);
        /// <summary>
        /// Drops index definitions by name of index.
        /// </summary>
        /// <param name="nsName">Namespace that have the indexes.</param>
        /// <param name="indexName">Index names to drop.</param>
        void DropIndex(string nsName, params string[] indexName);
        /// <summary>
        /// Starts a Reindexer transaction. Use it with <c>using</c> or don't forget to dispose.
        /// </summary>
        /// <param name="nsName"></param>
        /// <returns></returns>
        ReindexerTransaction StartTransaction(string nsName);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) an item.
        /// </summary>
        /// <param name="nsName">Namespace name</param>
        /// <param name="mode">Action to perform on item</param>
        /// <param name="itemJson">Item's json</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        int ModifyItem(string nsName, ItemModifyMode mode, string itemJson, params string[] precepts);
        /// <summary>
        /// Serialze and Insert an item to the namespace.
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="item">Item to be inserted</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        int Insert<TItem>(string nsName, TItem item, params string[] precepts);
        /// <summary>
        /// Serialize and Update an item to the namespace.PK indexed field will be used from <typeparamref name="TItem"/> when searching the item
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="item">Item to be updated</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        int Update<TItem>(string nsName, TItem item, params string[] precepts);
        /// <summary>
        /// Serialize and Upsert an item to the namespace. PK indexed field will be used from <typeparamref name="TItem"/> when searching the item.
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="item">Item to be upserted</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        int Upsert<TItem>(string nsName, TItem item, params string[] precepts);
        /// <summary>
        /// Deletes an item from namespace. Only PK indexed field will be used from <typeparamref name="TItem"/> when deleting.
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="item">Item to be deleted</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        int Delete<TItem>(string nsName, TItem item, params string[] precepts);
        /// <summary>
        /// Executes an sql query.
        /// </summary>
        /// <typeparam name="TItem">Item type to return</typeparam>
        /// <param name="sql">Sql query to perform.</param>
        /// <param name="deserializeItem">Item deserialization function from byte array.</param>
        /// <returns></returns>
        QueryItemsOf<TItem> ExecuteSql<TItem>(string sql, Func<byte[], TItem> deserializeItem);
        /// <summary>
        /// Executes an sql query.
        /// </summary>
        /// <typeparam name="TItem">Item type to return</typeparam>
        /// <param name="sql">Sql query to perform.</param>
        /// <returns></returns>
        QueryItemsOf<TItem> ExecuteSql<TItem>(string sql);
        /// <summary>
        /// Executes an sql query.
        /// </summary>
        /// <param name="sql">Sql query to perform.</param>
        /// <returns></returns>
        QueryItemsOf<byte[]> ExecuteSql(string sql);

        /// <summary>
        /// Connects to a reindexer implementation. This is first method to call before starting to use Reindexer.
        /// </summary>
        /// <param name="connectionString">Connection string for the implementation.</param>
        /// <param name="options">Reindexer connection options.</param>
        Task ConnectAsync(string connectionString, ConnectionOptions options = null);
        /// <summary>
        /// Pings the server. Does nothing on embedded mode.
        /// </summary>
        Task PingAsync();
        /// <summary>
        /// Opens a namespace. If it is missing, it wll be created.
        /// </summary>
        /// <param name="nsName">Namespace name</param>
        /// <param name="options">Reindexer namespace options.</param>
        Task OpenNamespaceAsync(string nsName, NamespaceOptions options = null);
        /// <summary>
        /// Drops namaspace
        /// </summary>
        /// <param name="nsName">Namespace to dtop</param>
        Task DropNamespaceAsync(string nsName);
        /// <summary>
        /// Closes namespace and unallocate the memory that it used by.
        /// </summary>
        /// <param name="nsName"></param>
        Task CloseNamespaceAsync(string nsName);
        /// <summary>
        /// Deletes all items in the namespace.
        /// </summary>
        /// <param name="nsName"></param>
        Task TruncateNamespaceAsync(string nsName);
        /// <summary>
        /// Renames namespace.
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        Task RenameNamespaceAsync(string oldName, string newName);
        /// <summary>
        /// Creates new index definitions.
        /// </summary>
        /// <param name="nsName">Namespace to add indexes</param>
        /// <param name="indexDefinitions">Index definitions to create</param>
        Task AddIndexAsync(string nsName, params Index[] indexDefinitions);
        /// <summary>
        /// Updates current index definitions in the namespace.
        /// </summary>
        /// <param name="nsName">Namespace that have the indexes.</param>
        /// <param name="indexDefinitions">Index definitions to update</param>
        Task UpdateIndexAsync(string nsName, params Index[] indexDefinitions);
        /// <summary>
        /// Drops index definitions by name of index.
        /// </summary>
        /// <param name="nsName">Namespace that have the indexes.</param>
        /// <param name="indexName">Index names to drop.</param>
        Task DropIndexAsync(string nsName, params string[] indexName);
        /// <summary>
        /// Starts a Reindexer transaction. Use it with <c>using</c> or don't forget to dispose.
        /// </summary>
        /// <param name="nsName"></param>
        /// <returns></returns>
        Task<ReindexerTransaction> StartTransactionAsync(string nsName);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) an item.
        /// </summary>
        /// <param name="nsName">Namespace name</param>
        /// <param name="mode">Action to perform on item</param>
        /// <param name="itemJson">Item's json</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        Task<int> ModifyItemAsync(string nsName, ItemModifyMode mode, string itemJson, params string[] precepts);
        /// <summary>
        /// Serialze and Insert an item to the namespace.
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="item">Item to be inserted</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        Task<int> InsertAsync<TItem>(string nsName, TItem item, params string[] precepts);
        /// <summary>
        /// Serialize and Update an item to the namespace.PK indexed field will be used from <typeparamref name="TItem"/> when searching the item
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="item">Item to be updated</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        Task<int> UpdateAsync<TItem>(string nsName, TItem item, params string[] precepts);
        /// <summary>
        /// Serialize and Upsert an item to the namespace. PK indexed field will be used from <typeparamref name="TItem"/> when searching the item.
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="item">Item to be upserted</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        Task<int> UpsertAsync<TItem>(string nsName, TItem item, params string[] precepts);
        /// <summary>
        /// Deletes an item from namespace. Only PK indexed field will be used from <typeparamref name="TItem"/> when deleting.
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="item">Item to be deleted</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        Task<int> DeleteAsync<TItem>(string nsName, TItem item, params string[] precepts);
        /// <summary>
        /// Executes an sql query.
        /// </summary>
        /// <typeparam name="TItem">Item type to return</typeparam>
        /// <param name="sql">Sql query to perform.</param>
        /// <param name="deserializeItem">Item deserialization function from byte array.</param>
        /// <returns></returns>
        Task<QueryItemsOf<TItem>> ExecuteSqlAsync<TItem>(string sql, Func<byte[], TItem> deserializeItem);
        /// <summary>
        /// Executes an sql query.
        /// </summary>
        /// <typeparam name="TItem">Item type to return</typeparam>
        /// <param name="sql">Sql query to perform.</param>
        /// <returns></returns>
        Task<QueryItemsOf<TItem>> ExecuteSqlAsync<TItem>(string sql);
        /// <summary>
        /// Executes an sql query.
        /// </summary>
        /// <param name="sql">Sql query to perform.</param>
        /// <returns></returns>
        Task<QueryItemsOf<byte[]>> ExecuteSqlAsync(string sql);
    }
}
