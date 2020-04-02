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
  public class AggregationResDef {
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
    /// Value, calculated by aggregator
    /// </summary>
    /// <value>Value, calculated by aggregator</value>
    [DataMember(Name="value", EmitDefaultValue=false)]
    [JsonPropertyName("value")]
    public decimal? Value { get; set; }

    /// <summary>
    /// Facets, calculated by aggregator
    /// </summary>
    /// <value>Facets, calculated by aggregator</value>
    [DataMember(Name="facets", EmitDefaultValue=false)]
    [JsonPropertyName("facets")]
    public List<AggregationResDefFacets> Facets { get; set; }

    /// <summary>
    /// List of distinct values of the field
    /// </summary>
    /// <value>List of distinct values of the field</value>
    [DataMember(Name="distincts", EmitDefaultValue=false)]
    [JsonPropertyName("distincts")]
    public List<string> Distincts { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class AggregationResDef {\n");
      sb.Append("  Fields: ").Append(Fields).Append("\n");
      sb.Append("  Type: ").Append(Type).Append("\n");
      sb.Append("  Value: ").Append(Value).Append("\n");
      sb.Append("  Facets: ").Append(Facets).Append("\n");
      sb.Append("  Distincts: ").Append(Distincts).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
