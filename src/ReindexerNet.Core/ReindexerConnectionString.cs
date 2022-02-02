using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet
{
    public class ReindexerConnectionString
    {
        public string DatabaseName { get; set; }
        public string HttpAddress { get; set; }
        public string RpcAddress { get; set; }
        public string GrpcAddress { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public ReindexerConnectionString()
        {

        }

        public ReindexerConnectionString(string keyValueString)
        {
            var connStringParts = keyValueString.Split(';');
            foreach (var (key, value) in connStringParts.Select(p => p.Split('=')).Select(p => p.Length > 1 ? (p[0].ToLowerInvariant(), p[1]) : (p[0].ToLowerInvariant(), "")))
            {
                FillValue(key, value);
            }
        }

        public static implicit operator ReindexerConnectionString(string keyValueString)
        {
            return new ReindexerConnectionString(keyValueString);
        }

        protected virtual void FillValue(string key, string value)
        {
            if (key.Equals("dbname", StringComparison.InvariantCultureIgnoreCase))
                DatabaseName = value;
            else if (key.Equals("httpAddr", StringComparison.InvariantCultureIgnoreCase))
                HttpAddress = value;
            else if (key.Equals("rpcAddr", StringComparison.InvariantCultureIgnoreCase))
                RpcAddress = value;
            else if (key.Equals("grpcAddr", StringComparison.InvariantCultureIgnoreCase))
                GrpcAddress = value;
            else if (key.Equals("user", StringComparison.InvariantCultureIgnoreCase))
                Username = value;
            else if (key.Equals("pass", StringComparison.InvariantCultureIgnoreCase))
                Password = value;
        }
    }
}
