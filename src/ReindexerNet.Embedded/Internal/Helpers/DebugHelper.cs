using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ReindexerNet.Embedded.Internal.Helpers
{
    internal static class DebugHelper
    {
        public static void Log(string message)
        {
            Debug.WriteLine("[{0:HH:mm:ss.fff}]\t{1}", DateTime.Now, message);
        }
    }
}
