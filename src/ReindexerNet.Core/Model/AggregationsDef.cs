using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// 
  /// </summary>
  [DataContract]
  public class AggregationsDef {
    /// <summary>
    /// Fields or indexes names for aggregation function
    /// </summary>
    /// <value>Fields or indexes names for aggregation function</value>
    [DataMember(Name="fields", EmitDefaultValue=false)]
    [JsonPropertyName("fields")]
    public List<string> Fields { get; set; }

    /// <summary>
    /// Aggregation function
    /// </summary>
    /// <value>Aggregation function</value>
    [DataMember(Name="type", EmitDefaultValue=false)]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// Specifies results sorting order. Allowed only for FACET
    /// </summary>
    /// <value>Specifies results sorting order. Allowed only for FACET</value>
    [DataMember(Name="sort", EmitDefaultValue=false)]
    [JsonPropertyName("sort")]
    public List<AggregationsSortDef> Sort { get; set; }

    /// <summary>
    /// Number of rows to get from result set. Allowed only for FACET
    /// </summary>
    /// <value>Number of rows to get from result set. Allowed only for FACET</value>
    [DataMember(Name="limit", EmitDefaultValue=false)]
    [JsonPropertyName("limit")]
    public long? Limit { get; set; }

    /// <summary>
    /// Index of the first row to get from result set. Allowed only for FACET
    /// </summary>
    /// <value>Index of the first row to get from result set. Allowed only for FACET</value>
    [DataMember(Name="offset", EmitDefaultValue=false)]
    [JsonPropertyName("offset")]
    public long? Offset { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class AggregationsDef {\n");
      sb.Append("  Fields: ").Append(Fields).Append("\n");
      sb.Append("  Type: ").Append(Type).Append("\n");
      sb.Append("  Sort: ").Append(Sort).Append("\n");
      sb.Append("  Limit: ").Append(Limit).Append("\n");
      sb.Append("  Offset: ").Append(Offset).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
