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
        /// Error code
        /// </summary>
        public ReindexerErrorCode ErrorCode { get; set; }
        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Creates reindexer exception
        /// </summary>
        /// <param name="code">Reindexer error code</param>
        /// <param name="message">Reindexer error message</param>
        public ReindexerException(int code, string message) : base($"Reindexer returned an error response, ErrCode: {code}, Msg:{message}")
        {
            ErrorCode = Enum.IsDefined(typeof(ReindexerErrorCode), code) ? (ReindexerErrorCode)code : ReindexerErrorCode.Unknown;
            ErrorMessage = message;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public enum ReindexerErrorCode
    {
        Unknown          =-1,
        OK               = 0,
	    ParseSQL         = 1,
	    QueryExec        = 2,
	    Params           = 3,
	    Logic            = 4,
	    ParseJson        = 5,
	    ParseDSL         = 6,
	    Conflict         = 7,
	    ParseBin         = 8,
	    Forbidden        = 9,
	    WasRelock        = 10,
	    NotValid         = 11,
	    Network          = 12,
	    NotFound         = 13,
	    StateInvalidated = 14,
	    Timeout          = 19,
	    Canceled         = 20,
	    TagsMissmatch    = 21
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
