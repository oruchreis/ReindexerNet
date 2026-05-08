using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// Connects to a Reindexer implementation. This is the first method to call before using Reindexer.
        /// </summary>
        /// <param name="options">Reindexer connection options.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task ConnectAsync(ConnectionOptions options = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Pings the server. Does nothing on embedded mode.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task PingAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Creates a database.
        /// </summary>
        /// <param name="dbName">Database name to create.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the create operation.</returns>
        Task CreateDatabaseAsync(string dbName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Enumerates all active databases.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>Database descriptors returned by Reindexer.</returns>
        Task<IEnumerable<Database>> EnumDatabasesAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Opens a namespace. If it is missing and options allow creation, it will be created.
        /// </summary>
        /// <param name="nsName">Namespace name.</param>
        /// <param name="options">Reindexer namespace options.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task OpenNamespaceAsync(string nsName, NamespaceOptions options = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Drops a namespace and its stored data.
        /// </summary>
        /// <param name="nsName">Namespace name to drop.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task DropNamespaceAsync(string nsName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Closes a namespace and releases memory used by it.
        /// </summary>
        /// <param name="nsName">Namespace name to close.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task CloseNamespaceAsync(string nsName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deletes all items in the namespace.
        /// </summary>
        /// <param name="nsName">Namespace name to truncate.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task TruncateNamespaceAsync(string nsName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Renames namespace.
        /// </summary>
        /// <param name="oldName">Current namespace name.</param>
        /// <param name="newName">New namespace name.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task RenameNamespaceAsync(string oldName, string newName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Enumerates namespaces.
        /// </summary>
        /// <param name="name">Optional namespace name filter.</param>
        /// <param name="onlyNames">Whether to return only namespace names without index and storage metadata.</param>
        /// <param name="hideSystems">Whether to hide system namespaces.</param>
        /// <param name="withClosed">Whether to include closed namespaces.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>Namespace descriptors returned by Reindexer.</returns>
        Task<IEnumerable<Namespace>> EnumNamespacesAsync(string name = null, bool onlyNames = false, 
            bool hideSystems = true, bool withClosed = false,
            CancellationToken cancellationToken = default);
        /// <summary>
        /// Creates new index definitions.
        /// </summary>
        /// <param name="nsName">Namespace name that will receive the index.</param>
        /// <param name="indexDefinition">Index definition to create.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task AddIndexAsync(string nsName, Index indexDefinition, CancellationToken cancellationToken = default);
        /// <summary>
        /// Updates current index definitions in the namespace.
        /// </summary>
        /// <param name="nsName">Namespace name that owns the index.</param>
        /// <param name="indexDefinition">Index definition to update.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task UpdateIndexAsync(string nsName, Index indexDefinition, CancellationToken cancellationToken = default);
        /// <summary>
        /// Drops index definitions by name of index.
        /// </summary>
        /// <param name="nsName">Namespace name that owns the index.</param>
        /// <param name="indexName">Index name to drop.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task DropIndexAsync(string nsName, string indexName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Starts a Reindexer transaction. Use it with <c>using</c> or don't forget to dispose.
        /// </summary>
        /// <param name="nsName">Namespace name for the transaction.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A transaction wrapper that must be committed or disposed.</returns>
        Task<ReindexerTransaction> StartTransactionAsync(string nsName, CancellationToken cancellationToken = default);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) on multiple items.
        /// </summary>
        /// <param name="nsName">Namespace name.</param>
        /// <param name="mode">Action to perform for each item.</param>
        /// <param name="items">Items to modify.</param>
        /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The number of items affected by the operation.</returns>
        Task<int> ModifyItemsAsync<TItem>(string nsName, ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) on multiple items with preserialized item data.
        /// </summary>
        /// <param name="nsName">Namespace name.</param>
        /// <param name="mode">Action to perform for each item.</param>
        /// <param name="itemDatas">Serialized item payloads.</param>
        /// <param name="dataEncoding">Encoding used by the serialized item payloads.</param>
        /// <param name="precepts">Precepts to apply after the modify action.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The number of items affected by the operation.</returns>
        Task<int> ModifyItemsAsync(string nsName, ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Serializes and inserts items into the namespace.
        /// </summary>
        /// <typeparam name="TItem">Item type.</typeparam>
        /// <param name="nsName">Namespace name.</param>
        /// <param name="items">Items to insert.</param>
        /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The number of inserted items.</returns>
        Task<int> InsertAsync<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Serializes and updates items in the namespace. The primary-key indexed field is used to find each item.
        /// </summary>
        /// <typeparam name="TItem">Item type.</typeparam>
        /// <param name="nsName">Namespace name.</param>
        /// <param name="items">Items to update.</param>
        /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The number of updated items.</returns>
        Task<int> UpdateAsync<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Serializes and upserts items in the namespace. The primary-key indexed field is used to find existing items.
        /// </summary>
        /// <typeparam name="TItem">Item type.</typeparam>
        /// <param name="nsName">Namespace name.</param>
        /// <param name="items">Items to upsert.</param>
        /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The number of upserted items.</returns>
        Task<int> UpsertAsync<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deletes items from the namespace. Only the primary-key indexed field is used from each item.
        /// </summary>
        /// <typeparam name="TItem">Item type.</typeparam>
        /// <param name="nsName">Namespace name.</param>
        /// <param name="items">Items to delete.</param>
        /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The number of deleted items.</returns>
        Task<int> DeleteAsync<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Executes a reindexer nosql query
        /// </summary>
        /// <typeparam name="TItem">Item type to deserialize from the query result.</typeparam>
        /// <param name="namespace">Namespace to query.</param>
        /// <param name="query">Callback used to build the query.</param>
        /// <param name="cancellationToken">Token used to cancel the query.</param>
        /// <returns>Query items, aggregation data, and query metadata returned by Reindexer.</returns>
        Task<QueryItemsOf<TItem>> ExecuteAsync<TItem>(string @namespace, Action<IQueryBuilder> query, CancellationToken cancellationToken = default);
        /// <summary>
        /// Executes a reindexer nosql query
        /// </summary>
        /// <typeparam name="TItem">Item type to deserialize from the query result.</typeparam>
        /// <param name="query">Serialized query payload.</param>
        /// <param name="queryEncoding">Encoding used by the serialized query payload.</param>
        /// <param name="cancellationToken">Token used to cancel the query.</param>
        /// <returns>Query items, aggregation data, and query metadata returned by Reindexer.</returns>
        Task<QueryItemsOf<TItem>> ExecuteAsync<TItem>(byte[] query, SerializerType queryEncoding, CancellationToken cancellationToken = default);
        /// <summary>
        /// Executes an sql query.
        /// </summary>
        /// <typeparam name="TItem">Item type to deserialize from the query result.</typeparam>
        /// <param name="sql">Sql query to perform.</param>
        /// <param name="cancellationToken">Token used to cancel the query.</param>
        /// <returns>Query items, aggregation data, and query metadata returned by Reindexer.</returns>
        Task<QueryItemsOf<TItem>> ExecuteSqlAsync<TItem>(string sql, CancellationToken cancellationToken = default);
        /// <summary>
        /// Executes a reindexer nosql query
        /// </summary>
        /// <param name="query">Serialized query payload.</param>
        /// <param name="queryEncoding">Encoding used by the serialized query payload.</param>
        /// <param name="cancellationToken">Token used to cancel the query.</param>
        /// <returns>Query items, aggregation data, and query metadata returned by Reindexer.</returns>
        Task<QueryItemsOf<object>> ExecuteAsync(byte[] query, SerializerType queryEncoding, CancellationToken cancellationToken = default);
        /// <summary>
        /// Executes an sql query.
        /// </summary>
        /// <param name="sql">Sql query to perform.</param>
        /// <param name="cancellationToken">Token used to cancel the query.</param>
        /// <returns>Query items, aggregation data, and query metadata returned by Reindexer.</returns>
        Task<QueryItemsOf<object>> ExecuteSqlAsync(string sql, CancellationToken cancellationToken = default);
        /// <summary>
        /// Sets the JSON schema for a namespace.
        /// </summary>
        /// <param name="nsName">Namespace name.</param>
        /// <param name="jsonSchema">JSON schema payload.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the schema update operation.</returns>
        Task SetSchemaAsync(string nsName, string jsonSchema, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets metadata value by key.
        /// </summary>
        /// <param name="nsName">Namespace name.</param>
        /// <param name="metadata">Metadata descriptor containing the key to read.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The metadata value.</returns>
        Task<string> GetMetaAsync(string nsName, MetaInfo metadata, CancellationToken cancellationToken = default);
        /// <summary>
        /// Sets metadata value by key.
        /// </summary>
        /// <param name="nsName">Namespace name.</param>
        /// <param name="metadata">Metadata descriptor containing the key and value to write.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the metadata update operation.</returns>
        Task PutMetaAsync(string nsName, MetaInfo metadata, CancellationToken cancellationToken = default);
        /// <summary>
        /// Enumerates metadata keys for a namespace.
        /// </summary>
        /// <param name="nsName">Namespace name.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>Metadata keys available in the namespace.</returns>
        Task<IEnumerable<string>> EnumMetaAsync(string nsName, CancellationToken cancellationToken = default);
    }
}
