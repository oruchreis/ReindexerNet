using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReindexerNet
{
    /// <summary>
    /// Action invoker interface in a Reindexer transaction.
    /// </summary>
    public interface ITransactionInvoker
    {
        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <returns>Number of items to be affected.</returns>
        int Commit();
        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <returns>Number of items to be affected.</returns>
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Cancels and rolls back the transaction.
        /// </summary>
        void Rollback();
        /// <summary>
        /// Cancels and rolls back the transaction.
        /// </summary>
        /// <returns></returns>
        Task RollbackAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) an item.
        /// </summary>
        /// <param name="mode">Action to perform on item</param>
        /// <param name="items">Items</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        int ModifyItems<TItem>(ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) an item.
        /// </summary>
        /// <param name="mode">Action to perform on item</param>
        /// <param name="items">Items</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> ModifyItemsAsync<TItem>(ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) an item with preserialized data
        /// </summary>
        /// <param name="mode">Action to perform on item</param>
        /// <param name="itemDatas">Items</param>
        /// <param name="dataEncoding"></param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        int ModifyItems(ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) an item with preserialized data
        /// </summary>
        /// <param name="mode">Action to perform on item</param>
        /// <param name="itemDatas">Items</param>
        /// <param name="dataEncoding"></param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> ModifyItemsAsync(ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null, CancellationToken cancellationToken = default);
    }
}
