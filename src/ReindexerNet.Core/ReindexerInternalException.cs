using System;
using System.Collections.Generic;
using System.Text;

namespace ReindexerNet
{
    public class ReindexerInternalException: Exception
    {
        const string _message = "Reindexer internal error ocurred. See for the inner exception.";
        public ReindexerInternalException(Exception innerException): base(_message, innerException)
        {

        }
    }
}
