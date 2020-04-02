using System;
using System.Collections.Generic;
using System.Text;

namespace ReindexerNet
{
    public class ReindexerException: Exception
    {
        public ReindexerException(int code, string message): base($"Reindexer returned an error response, ErrCode: {code}, Msg:{message}")
        {

        }
    }
}
