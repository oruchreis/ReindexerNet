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
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The number of items affected by the transaction.</returns>
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Cancels and rolls back the transaction.
        /// </summary>
        void Rollback();
        /// <summary>
        /// Cancels and rolls back the transaction.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the rollback operation.</returns>
        Task RollbackAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) an item.
        /// </summary>
        /// <param name="mode">Action to perform for each item.</param>
        /// <param name="items">Items to modify.</param>
        /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
        /// <returns>The number of items affected by the operation.</returns>
        int ModifyItems<TItem>(ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) an item.
        /// </summary>
        /// <param name="mode">Action to perform for each item.</param>
        /// <param name="items">Items to modify.</param>
        /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The number of items affected by the operation.</returns>
        Task<int> ModifyItemsAsync<TItem>(ItemModifyMode mode, IEnumerable<TItem> items, string[] precepts = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) with preserialized item data.
        /// </summary>
        /// <param name="mode">Action to perform for each item.</param>
        /// <param name="itemDatas">Serialized item payloads.</param>
        /// <param name="dataEncoding">Encoding used by the serialized item payloads.</param>
        /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
        /// <returns>The number of items affected by the operation.</returns>
        int ModifyItems(ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) with preserialized item data.
        /// </summary>
        /// <param name="mode">Action to perform for each item.</param>
        /// <param name="itemDatas">Serialized item payloads.</param>
        /// <param name="dataEncoding">Encoding used by the serialized item payloads.</param>
        /// <param name="precepts">Precepts to apply after the modify action, such as <c>UpdateTime=now()</c> or <c>Id=serial()</c>.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The number of items affected by the operation.</returns>
        Task<int> ModifyItemsAsync(ItemModifyMode mode, IEnumerable<byte[]> itemDatas, SerializerType dataEncoding, string[] precepts = null, CancellationToken cancellationToken = default);
    }
}
