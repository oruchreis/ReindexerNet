using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet
{
    /// <summary>
    /// The connection string to connect to Reindexer.
    /// </summary>
    public class ReindexerConnectionString
    {
        /// <summary>
        /// Database Name
        /// </summary>
        public string DatabaseName { get; set; }
        /// <summary>
        /// Http Address to bind
        /// </summary>
        public string HttpAddress { get; set; }
        /// <summary>
        /// Rpc Address to bind
        /// </summary>
        public string RpcAddress { get; set; }
        /// <summary>
        /// Grpc Address to bind
        /// </summary>
        public string GrpcAddress { get; set; }
        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Creates connection string
        /// </summary>
        public ReindexerConnectionString()
        {

        }

        /// <summary>
        /// Creates connection string from <c>key1=value1;key2=value2</c> string
        /// </summary>
        /// <param name="keyValueString"></param>
        public ReindexerConnectionString(string keyValueString)
        {
            var connStringParts = keyValueString.Split(';');
            foreach (var (key, value) in connStringParts.Select(p => p.Split('=')).Select(p => p.Length > 1 ? (p[0].ToLowerInvariant(), p[1]) : (p[0].ToLowerInvariant(), "")))
            {
                FillValue(key, value);
            }
        }

        /// <summary>
        /// Creates connection string from <c>key1=value1;key2=value2</c> string
        /// </summary>
        /// <param name="keyValueString"></param>
        public static implicit operator ReindexerConnectionString(string keyValueString)
        {
            return new ReindexerConnectionString(keyValueString);
        }

        /// <summary>
        /// Fills key value to corresponds with properties in the connection string.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected virtual void FillValue(string key, string value)
        {
            if (key.Equals("dbname", StringComparison.InvariantCultureIgnoreCase))
                DatabaseName = value;
            else if (key.Equals("httpaddr", StringComparison.InvariantCultureIgnoreCase))
                HttpAddress = value;
            else if (key.Equals("rpcaddr", StringComparison.InvariantCultureIgnoreCase))
                RpcAddress = value;
            else if (key.Equals("grpcaddr", StringComparison.InvariantCultureIgnoreCase))
                GrpcAddress = value;
            else if (key.Equals("user", StringComparison.InvariantCultureIgnoreCase))
                Username = value;
            else if (key.Equals("pass", StringComparison.InvariantCultureIgnoreCase))
                Password = value;
        }
    }
}
