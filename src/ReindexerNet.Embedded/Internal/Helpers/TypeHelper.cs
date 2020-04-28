using System;
using System.Collections.Generic;
using System.Text;

namespace ReindexerNet.Embedded.Internal.Helpers
{
    internal static class TypeHelper
    {
        public static ReindexerStringHandle GetHandle(this string str)
        {
            return reindexer_string.From(str);
        }

        public static ReindexerStringHandle GetStringHandle(this byte[] utf8ByteArray)
        {
            return reindexer_string.From(utf8ByteArray);
        }

        public static ReindexerBufferHandle GetHandle(this byte[] byteArray)
        {
            return reindexer_buffer.From(byteArray);
        }
    }
}
