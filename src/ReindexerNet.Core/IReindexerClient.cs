using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReindexerNet
{
    /// <summary>
    /// Common interface for Reindexer async/sync operations.
    /// </summary>
    public interface IReindexerClient : IAsyncReindexerClient, IDisposable
    {
        /// <summary>
        /// Connects to a reindexer implementation. This is first method to call before starting to use Reindexer.
        /// </summary>
        /// <param name="options">Reindexer connection options.</param>
        void Connect(ConnectionOptions options = null);
        /// <summary>
        /// Pings the server. Does nothing on embedded mode.
        /// </summary>
        void Ping();
        /// <summary>
        /// Creates a database
        /// </summary>
        /// <param name="dbName"></param>
        void CreateDatabase(string dbName);
        IEnumerable<Database> EnumDatabases();
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
        /// Enumerates all active namespaces
        /// </summary>
        /// <returns></returns>
        IEnumerable<Namespace> EnumNamespaces();
        /// <summary>
        /// Creates new index definitions.
        /// </summary>
        /// <param name="nsName">Namespace to add indexes</param>
        /// <param name="indexDefinition">Index definition to create</param>
        void AddIndex(string nsName, Index indexDefinition);
        /// <summary>
        /// Updates current index definitions in the namespace.
        /// </summary>
        /// <param name="nsName">Namespace that have the indexes.</param>
        /// <param name="indexDefinition">Index definition to update</param>
        void UpdateIndex(string nsName, Index indexDefinition);
        /// <summary>
        /// Drops index definitions by name of index.
        /// </summary>
        /// <param name="nsName">Namespace that have the indexes.</param>
        /// <param name="indexName">Index name to drop.</param>
        void DropIndex(string nsName, string indexName);
        /// <summary>
        /// Starts a Reindexer transaction. Use it with <c>using</c> or don't forget to dispose.
        /// </summary>
        /// <param name="nsName"></param>
        /// <returns></returns>
        ReindexerTransaction StartTransaction(string nsName);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) on multiple items.
        /// </summary>
        /// <param name="nsName">Namespace name</param>
        /// <param name="mode">Action to perform on item</param>
        /// <param name="items">Items</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        int ModifyItems<TItem>(string nsName, ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) on multiple items with preserialized item data.
        /// </summary>
        /// <param name="nsName"></param>
        /// <param name="mode"></param>
        /// <param name="itemDatas"></param>
        /// <param name="dataEncoding"></param>
        /// <param name="precepts"></param>
        /// <returns></returns>
        int ModifyItems(string nsName, ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null);
        /// <summary>
        /// Serialze and Insert an item to the namespace.
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="items">Items to be inserted</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        int Insert<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null);
        /// <summary>
        /// Serialize and Update an item to the namespace.PK indexed field will be used from <typeparamref name="TItem"/> when searching the item
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="items">Items to be updated</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        int Update<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null);
        /// <summary>
        /// Serialize and Upsert an item to the namespace. PK indexed field will be used from <typeparamref name="TItem"/> when searching the item.
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="items">Items to be upserted</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        int Upsert<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null);
        /// <summary>
        /// Deletes an item from namespace. Only PK indexed field will be used from <typeparamref name="TItem"/> when deleting.
        /// </summary>
        /// <typeparam name="TItem">Item type</typeparam>
        /// <param name="nsName">Namespace name</param>
        /// <param name="items">Items to be deleted</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        int Delete<TItem>(string nsName, IEnumerable<TItem> items, string[] precepts = null);
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
        QueryItemsOf<object> ExecuteSql(string sql);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nsName"></param>
        /// <param name="jsonSchema"></param>
        void SetSchema(string nsName, string jsonSchema);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nsName"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        string GetMeta(string nsName, MetaInfo metadata);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nsName"></param>
        /// <param name="metadata"></param>
        void PutMeta(string nsName, MetaInfo metadata);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nsName"></param>
        /// <returns></returns>
        IEnumerable<string> EnumMeta(string nsName);
    }
}
