using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReindexerNet
{
    /// <summary>
    /// Common interface for Reindexer async operations.
    /// </summary>
    public interface IAsyncReindexerClient : IAsyncDisposable
    {
        /// <summary>
        /// Connects to a reindexer implementation. This is first method to call before starting to use Reindexer.
        /// </summary>
        /// <param name="options">Reindexer connection options.</param>
        Task ConnectAsync(ConnectionOptions options = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Pings the server. Does nothing on embedded mode.
        /// </summary>
        Task PingAsync(CancellationToken cancellationToken = default);
        Task CreateDatabaseAsync(string dbName, CancellationToken cancellationToken = default);
        Task<IEnumerable<Database>> EnumDatabasesAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Opens a namespace. If it is missing, it wll be created.
        /// </summary>
        /// <param name="nsName">Namespace name</param>
        /// <param name="options">Reindexer namespace options.</param>
        Task OpenNamespaceAsync(string nsName, NamespaceOptions options = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Drops namaspace
        /// </summary>
        /// <param name="nsName">Namespace to dtop</param>
        Task DropNamespaceAsync(string nsName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Closes namespace and unallocate the memory that it used by.
        /// </summary>
        /// <param name="nsName"></param>
        Task CloseNamespaceAsync(string nsName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deletes all items in the namespace.
        /// </summary>
        /// <param name="nsName"></param>
        Task TruncateNamespaceAsync(string nsName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Renames namespace.
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        Task RenameNamespaceAsync(string oldName, string newName, CancellationToken cancellationToken = default);
        Task<IEnumerable<Namespace>> EnumNamespacesAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Creates new index definitions.
        /// </summary>
        /// <param name="nsName">Namespace to add indexes</param>
        /// <param name="indexDefinitions">Index definitions to create</param>
        Task AddIndexAsync(string nsName, Index[] indexDefinitions, CancellationToken cancellationToken = default);
        /// <summary>
        /// Updates current index definitions in the namespace.
        /// </summary>
        /// <param name="nsName">Namespace that have the indexes.</param>
        /// <param name="indexDefinitions">Index definitions to update</param>
        Task UpdateIndexAsync(string nsName, Index[] indexDefinitions, CancellationToken cancellationToken = default);
        /// <summary>
        /// Drops index definitions by name of index.
        /// </summary>
        /// <param name="nsName">Namespace that have the indexes.</param>
        /// <param name="indexName">Index names to drop.</param>
        Task DropIndexAsync(string nsName, string[] indexName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Starts a Reindexer transaction. Use it with <c>using</c> or don't forget to dispose.
        /// </summary>
        /// <param name="nsName"></param>
        /// <returns></returns>
        Task<ReindexerTransaction> StartTransactionAsync(string nsName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) on multiple items.
        /// </summary>
        /// <param name="nsName">Namespace name</param>
        /// <param name="mode">Action to perform on item</param>
        /// <param name="items">Items</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        Task<int> ModifyItemsAsync<TItem>(string nsName, ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Serialze and Insert an item to the namespace.
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="items">Items to be inserted</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        Task<int> InsertAsync<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Serialize and Update an item to the namespace.PK indexed field will be used from <typeparamref name="TItem"/> when searching the item
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="items">Items to be updated</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        Task<int> UpdateAsync<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Serialize and Upsert an item to the namespace. PK indexed field will be used from <typeparamref name="TItem"/> when searching the item.
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="items">Items to be upserted</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        Task<int> UpsertAsync<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deletes an item from namespace. Only PK indexed field will be used from <typeparamref name="TItem"/> when deleting.
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="items">Items to be deleted</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        Task<int> DeleteAsync<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Executes an sql query.
        /// </summary>
        /// <typeparam name="TItem">Item type to return</typeparam>
        /// <param name="sql">Sql query to perform.</param>
        /// <returns></returns>
        Task<QueryItemsOf<TItem>> ExecuteSqlAsync<TItem>(string sql, CancellationToken cancellationToken = default);
        /// <summary>
        /// Executes an sql query.
        /// </summary>
        /// <param name="sql">Sql query to perform.</param>
        /// <returns></returns>
        Task<QueryItemsOf<object>> ExecuteSqlAsync(string sql, CancellationToken cancellationToken = default);
        Task SetSchemaAsync(string nsName, string jsonSchema, CancellationToken cancellationToken = default);
        Task<string> GetMetaAsync(string nsName, MetaInfo metadata, CancellationToken cancellationToken = default);
        Task PutMetaAsync(string nsName, MetaInfo metadata, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> EnumMetaAsync(string nsName, CancellationToken cancellationToken = default);
    }
}
