using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet
{
    public interface ITransactionInvoker
    {
        int Commit();
        Task<int> CommitAsync();
        void Rollback();
        Task RollbackAsync();        

        void ModifyItem(ItemModifyMode mode, byte[] itemJson, params string[] precepts);
        Task ModifyItemAsync(ItemModifyMode mode, byte[] itemJson, params string[] precepts);
    }
}
