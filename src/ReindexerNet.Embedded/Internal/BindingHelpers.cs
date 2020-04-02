using System;
using System.Collections.Generic;
using System.Text;

namespace ReindexerNet.Embedded.Internal
{
    internal static class BindingHelpers
    {
        public static (string json, List<int> offsets, byte[] explain) RawResultToJson(ReadOnlySpan<byte> rawResult, string jsonName, string totalName)
        {
            var ser = new CJsonReader(rawResult);
            var rawQueryParams = ser.ReadRawQueryParams();
            var explain = rawQueryParams.explainResults;

            var jsonReserveLen = rawResult.Length + totalName.Length + jsonName.Length + 20;
            var jsonBuf = new StringBuilder(jsonReserveLen);

            var offsets = new List<int>(rawQueryParams.count);

            jsonBuf.Append("{\"");

            if (totalName.Length != 0 && rawQueryParams.totalcount != 0)
            {
                jsonBuf.Append(totalName);
                jsonBuf.Append("\":");
                jsonBuf.Append(rawQueryParams.totalcount);
                jsonBuf.Append(",\"");
            }

            jsonBuf.Append(jsonName);
            jsonBuf.Append("\":[");

            for (var i = 0; i < rawQueryParams.count; i++)
            {
                var item = ser.ReadRawItemParams();
                if (i != 0)
                {
                    jsonBuf.Append(",");
                }
                offsets.Add(jsonBuf.Length);
                jsonBuf.Append(Encoding.UTF8.GetString(item.data
#if NETSTANDARD2_0 || NET472
                    .ToArray()
#endif
                    ));

                if ((rawQueryParams.flags & CJsonReader.ResultsWithJoined) != 0 && ser.GetVarUInt() != 0)
                {
                    throw new NotImplementedException("Sorry, not implemented: Can't return join query results as json");
                }
            }
            jsonBuf.Append("]}");

            return (jsonBuf.ToString(), offsets, explain.ToArray());
        }
    }
}
