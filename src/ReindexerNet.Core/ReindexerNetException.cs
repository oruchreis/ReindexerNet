using System;

namespace ReindexerNet
{
    /// <summary>
    /// Exception that occured in ReindexerNet library.
    /// </summary>
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class ReindexerNetException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    {
        /// <inheritdoc/>
        public ReindexerNetException(string message) : base(message)
        {
        }
        /// <inheritdoc/>
        public ReindexerNetException() : base()
        {
        }
        /// <inheritdoc/>
        public ReindexerNetException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
