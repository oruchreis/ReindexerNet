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
  public class AggregationResDefFacets {
    /// <summary>
    /// Facet fields values
    /// </summary>
    /// <value>Facet fields values</value>
    [DataMember(Name="values", EmitDefaultValue=false)]
    [JsonPropertyName("values")]
    public List<string> Values { get; set; }

    /// <summary>
    /// Count of elemens these fields values
    /// </summary>
    /// <value>Count of elemens these fields values</value>
    [DataMember(Name="count", EmitDefaultValue=false)]
    [JsonPropertyName("count")]
    public long? Count { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class AggregationResDefFacets {\n");
      sb.Append("  Values: ").Append(Values).Append("\n");
      sb.Append("  Count: ").Append(Count).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
