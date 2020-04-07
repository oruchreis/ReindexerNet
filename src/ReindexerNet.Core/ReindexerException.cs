using System;

namespace ReindexerNet
{
    /// <summary>
    /// Reindexer response error.
    /// </summary>
#pragma warning disable RCS1194 // Implement exception constructors.
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class ReindexerException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
#pragma warning restore RCS1194 // Implement exception constructors.
    {
        /// <summary>
        /// Creates reindexer exception
        /// </summary>
        /// <param name="code">Reindexer error code</param>
        /// <param name="message">Reindexer error message</param>
        public ReindexerException(int code, string message) : base($"Reindexer returned an error response, ErrCode: {code}, Msg:{message}")
        {
        }
    }
}
