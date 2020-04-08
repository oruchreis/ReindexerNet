using ReindexerNet.Embedded.Internal;
using System;
using System.Runtime.InteropServices;

namespace ReindexerNet.Embedded.Helpers
{
    internal static class Assert
    {
        public static void ThrowIfError(Func<reindexer_error> action)
        {
            reindexer_error error;
            try
            {
                error = action();
            }
            catch (Exception e)
            {
                throw new ReindexerInternalException(e);
            }

            if (error.code != 0)
            {
                var errorStr = Marshal.PtrToStringAnsi(error.what);
                ReindexerBinding.malloc_free(error.what);
                throw new ReindexerException(error.code, errorStr);
            }
        }

        public static reindexer_ret ThrowIfError(Func<reindexer_ret> action)
        {
            reindexer_ret rsp;
            try
            {
                rsp = action();
            }
            catch (Exception e)
            {
                throw new ReindexerInternalException(e);
            }

            if (rsp.err_code != 0)
            {
                if (rsp.@out.results_ptr != default)
                    rsp.@out.Free();
                throw new ReindexerException(rsp.err_code, rsp.@out);
            }

            return rsp;
        }
    }
}
