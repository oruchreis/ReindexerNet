using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet
{
    public interface ITransactionInvoker
    {
        void Commit();
        Task CommitAsync();
        void Rollback();
        Task RollbackAsync();        

        int ModifyItem(ItemModifyMode mode, string itemJson, params string[] precepts);
        Task<int> ModifyItemAsync(ItemModifyMode mode, string itemJson, params string[] precepts);
    }
}
