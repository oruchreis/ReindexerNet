using System;

namespace ReindexerNet
{
    /// <summary>
    /// Exception that occured in Reindexer native library.
    /// </summary>
#pragma warning disable RCS1194 // Implement exception constructors.
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class ReindexerInternalException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
#pragma warning restore RCS1194 // Implement exception constructors.
    {
        private const string _message = "Reindexer internal error ocurred. See for the inner exception.";
        /// <inheritdoc/>
        public ReindexerInternalException(Exception innerException) : base(_message, innerException)
        {
        }
    }
}
