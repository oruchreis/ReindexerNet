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
        Task<int> CommitAsync();
        /// <summary>
        /// Cancels and rolls back the transaction.
        /// </summary>
        void Rollback();
        /// <summary>
        /// Cancels and rolls back the transaction.
        /// </summary>
        /// <returns></returns>
        Task RollbackAsync();
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) an item.
        /// </summary>
        /// <param name="mode">Action to perform on item</param>
        /// <param name="itemJson">Item's json</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        void ModifyItem(ItemModifyMode mode, byte[] itemJson, params string[] precepts);
        /// <summary>
        /// Performs one of these actions: Insert, Update, Delete or Upsert(Insert or Update) an item.
        /// </summary>
        /// <param name="mode">Action to perform on item</param>
        /// <param name="itemJson">Item's json</param>
        /// <param name="precepts">Precepts to be done after modify action. For example, you can update time by <c>UpdateTime=now()</c> or you can increase id by <c>Id=serial()</c></param>
        /// <returns></returns>
        Task ModifyItemAsync(ItemModifyMode mode, byte[] itemJson, params string[] precepts);
    }
}
