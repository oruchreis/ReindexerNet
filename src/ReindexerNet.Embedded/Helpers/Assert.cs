using ReindexerNet.Embedded.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReindexerNet.Embedded.Helpers
{
    static class Assert
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
				throw new ReindexerException(error.code, error.what);
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
				throw new ReindexerException(rsp.err_code, rsp.@out);

			return rsp;
        }

		//public static bool FalseIfError(Func<reindexer_error> action)
  //      {
		//	reindexer_error error;
		//	try
		//	{
		//		error = action();
		//	}
		//	catch (Exception e)
		//	{
		//		throw new ReindexerInternalException(e);
		//	}

		//	return error.code == 0;
  //      }
    }
}
