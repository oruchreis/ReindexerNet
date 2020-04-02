using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Performance statistics per each query
  /// </summary>
  [DataContract]
  public class QueryPerfStats : CommonPerfStats {
    /// <summary>
    /// normalized SQL representation of query
    /// </summary>
    /// <value>normalized SQL representation of query</value>
    [DataMember(Name="query", EmitDefaultValue=false)]
    [JsonPropertyName("query")]
    public string Query { get; set; }

    /// <summary>
    /// not normalized SQL representation of longest query
    /// </summary>
    /// <value>not normalized SQL representation of longest query</value>
    [DataMember(Name="longest_query", EmitDefaultValue=false)]
    [JsonPropertyName("longest_query")]
    public string LongestQuery { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class QueryPerfStats {\n");
      sb.Append("  Query: ").Append(Query).Append("\n");
      sb.Append("  LongestQuery: ").Append(LongestQuery).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }


}
}
