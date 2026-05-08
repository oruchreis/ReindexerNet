using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ReindexerNet;

/// <summary>
/// Common interface for Reindexer async/sync operations.
/// </summary>
public interface IReindexerClient : IAsyncReindexerClient, IDisposable
{
    /// <summary>
    /// Connects to a Reindexer implementation. This is the first method to call before using Reindexer.
    /// </summary>
    /// <param name="options">Reindexer connection options.</param>
    void Connect(ConnectionOptions options = null);
    /// <summary>
    /// Pings the server. Does nothing on embedded mode.
    /// </summary>
    void Ping();
    /// <summary>
    /// Creates a database.
    /// </summary>
    /// <param name="dbName">Database name to create.</param>
    void CreateDatabase(string dbName);
    /// <summary>
    /// Enumerates databases.
    /// </summary>
    /// <returns>Database descriptors returned by Reindexer.</returns>
    IEnumerable<Database> EnumDatabases();
    /// <summary>
    /// Opens a namespace. If it is missing and options allow creation, it will be created.
    /// </summary>
    /// <param name="nsName">Namespace name</param>
    /// <param name="options">Reindexer namespace options.</param>
    void OpenNamespace(string nsName, NamespaceOptions options = null);
    /// <summary>
    /// Drops a namespace and its stored data.
    /// </summary>
    /// <param name="nsName">Namespace name to drop.</param>
    void DropNamespace(string nsName);
    /// <summary>
    /// Closes a namespace and releases memory used by it.
    /// </summary>
    /// <param name="nsName">Namespace name to close.</param>
    void CloseNamespace(string nsName);
    /// <summary>
    /// Deletes all items in the namespace.
    /// </summary>
    /// <param name="nsName">Namespace name to truncate.</param>
    void TruncateNamespace(string nsName);
    /// <summary>
    /// Renames namespace.
    /// </summary>
    /// <param name="oldName">Current namespace name.</param>
    /// <param name="newName">New namespace name.</param>
    void RenameNamespace(string oldName, string newName);
    /// <summary>
    /// Enumerates namespaces.
    /// </summary>
    /// <param name="name">Optional namespace name filter.</param>
    /// <param name="onlyNames">Whether to return only namespace names without index and storage metadata.</param>
    /// <param name="hideSystems">Whether to hide system namespaces.</param>
    /// <param name="withClosed">Whether to include closed namespaces.</param>
    /// <returns>Namespace descriptors returned by Reindexer.</returns>
    IEnumerable<Namespace> EnumNamespaces(string name = null, bool onlyNames = false, 
        bool hideSystems = true, bool withClosed = false);
    /// <summary>
    /// Creates new index definitions.
    /// </summary>
    /// <param name="nsName">Namespace name that will receive the index.</param>
    /// <param name="indexDefinition">Index definition to create.</param>
    void AddIndex(string nsName, Index indexDefinition);
    /// <summary>
    /// Updates current index definitions in the namespace.
    /// </summary>
    /// <param name="nsName">Namespace name that owns the index.</param>
    /// <param name="indexDefinition">Index definition to update.</param>
    void UpdateIndex(string nsName, Index indexDefinition);
    /// <summary>
    /// Drops index definitions by name of index.
    /// </summary>
    /// <param name="nsName">Namespace name that owns the index.</param>
    /// <param name="indexName">Index name to drop.</param>
    void DropIndex(string nsName, string indexName);
    /// <summary>
    /// Starts a Reindexer transaction. Use it with <c>using</c> or don't forget to dispose.
    /// </summary>
    /// <param name="nsName">Namespace name for the transaction.</param>
    /// <returns>A transaction wrapper that must be committed or disposed.</returns>
    ReindexerTransaction StartTransaction(string nsName);
    /// <summary>
    /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) on multiple items.
    /// </summary>
    /// <param name="nsName">Namespace name.</param>
    /// <param name="mode">Action to perform for each item.</param>
    /// <param name="items">Items to modify.</param>
    /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
    /// <returns>The number of items affected by the operation.</returns>
    int ModifyItems<TItem>(string nsName, ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null);
    /// <summary>
    /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) on multiple items with preserialized item data.
    /// </summary>
    /// <param name="nsName">Namespace name.</param>
    /// <param name="mode">Action to perform for each item.</param>
    /// <param name="itemDatas">Serialized item payloads.</param>
    /// <param name="dataEncoding">Encoding used by the serialized item payloads.</param>
    /// <param name="precepts">Precepts to apply after the modify action.</param>
    /// <returns>The number of items affected by the operation.</returns>
    int ModifyItems(string nsName, ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null);
    /// <summary>
    /// Serializes and inserts items into the namespace.
    /// </summary>
    /// <typeparam name="TItem">Item type.</typeparam>
    /// <param name="nsName">Namespace name.</param>
    /// <param name="items">Items to insert.</param>
    /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
    /// <returns>The number of inserted items.</returns>
    int Insert<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null);
    /// <summary>
    /// Serializes and updates items in the namespace. The primary-key indexed field is used to find each item.
    /// </summary>
    /// <typeparam name="TItem">Item type.</typeparam>
    /// <param name="nsName">Namespace name.</param>
    /// <param name="items">Items to update.</param>
    /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
    /// <returns>The number of updated items.</returns>
    int Update<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null);
    /// <summary>
    /// Serializes and upserts items in the namespace. The primary-key indexed field is used to find existing items.
    /// </summary>
    /// <typeparam name="TItem">Item type.</typeparam>
    /// <param name="nsName">Namespace name.</param>
    /// <param name="items">Items to upsert.</param>
    /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
    /// <returns>The number of upserted items.</returns>
    int Upsert<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null);
    /// <summary>
    /// Deletes items from the namespace. Only the primary-key indexed field is used from each item.
    /// </summary>
    /// <typeparam name="TItem">Item type.</typeparam>
    /// <param name="nsName">Namespace name.</param>
    /// <param name="items">Items to delete.</param>
    /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
    /// <returns>The number of deleted items.</returns>
    int Delete<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null);
    /// <summary>
    /// Executes a reindexer nosql query
    /// </summary>
    /// <typeparam name="T">Item type to deserialize from the query result.</typeparam>
    /// <param name="namespace">Namespace to query.</param>
    /// <param name="query">Callback used to build the query.</param>
    /// <returns>Query items, aggregation data, and query metadata returned by Reindexer.</returns>
    QueryItemsOf<T> Execute<T>(string @namespace, Action<IQueryBuilder> query);
    /// <summary>
    /// Executes a reindexer nosql query
    /// </summary>
    /// <typeparam name="T">Item type to deserialize from the query result.</typeparam>
    /// <param name="query">Serialized query payload.</param>
    /// <param name="queryEncoding">Encoding used by the serialized query payload.</param>
    /// <returns>Query items, aggregation data, and query metadata returned by Reindexer.</returns>
    QueryItemsOf<T> Execute<T>(byte[] query, SerializerType queryEncoding);
    /// <summary>
    /// Executes an sql query.
    /// </summary>
    /// <typeparam name="TItem">Item type to deserialize from the query result.</typeparam>
    /// <param name="sql">Sql query to perform.</param>
    /// <returns>Query items, aggregation data, and query metadata returned by Reindexer.</returns>
    QueryItemsOf<TItem> ExecuteSql<TItem>(string sql);
    /// <summary>
    /// Executes a reindexer nosql query
    /// </summary>
    /// <param name="query">Serialized query payload.</param>
    /// <param name="queryEncoding">Encoding used by the serialized query payload.</param>
    /// <returns>Query items, aggregation data, and query metadata returned by Reindexer.</returns>
    QueryItemsOf<object> Execute(byte[] query, SerializerType queryEncoding);
    /// <summary>
    /// Executes an sql query.
    /// </summary>
    /// <param name="sql">Sql query to perform.</param>
    /// <returns>Query items, aggregation data, and query metadata returned by Reindexer.</returns>
    QueryItemsOf<object> ExecuteSql(string sql);
    /// <summary>
    /// Sets the JSON schema for a namespace.
    /// </summary>
    /// <param name="nsName">Namespace name.</param>
    /// <param name="jsonSchema">JSON schema payload.</param>
    void SetSchema(string nsName, string jsonSchema);
    /// <summary>
    /// Gets metadata value by key.
    /// </summary>
    /// <param name="nsName">Namespace name.</param>
    /// <param name="metadata">Metadata descriptor containing the key to read.</param>
    /// <returns>The metadata value.</returns>
    string GetMeta(string nsName, MetaInfo metadata);
    /// <summary>
    /// Sets metadata value by key.
    /// </summary>
    /// <param name="nsName">Namespace name.</param>
    /// <param name="metadata">Metadata descriptor containing the key and value to write.</param>
    void PutMeta(string nsName, MetaInfo metadata);
    /// <summary>
    /// Enumerates metadata keys for a namespace.
    /// </summary>
    /// <param name="nsName">Namespace name.</param>
    /// <returns>Metadata keys available in the namespace.</returns>
    IEnumerable<string> EnumMeta(string nsName);    
}
