using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Specifies facet aggregations results sorting order
  /// </summary>
  [DataContract]
  public class AggregationsSortDef {
    /// <summary>
    /// Field or index name for sorting
    /// </summary>
    /// <value>Field or index name for sorting</value>
    [DataMember(Name="field", EmitDefaultValue=false)]
    [JsonPropertyName("field")]
    public string Field { get; set; }

    /// <summary>
    /// Descent or ascent sorting direction
    /// </summary>
    /// <value>Descent or ascent sorting direction</value>
    [DataMember(Name="desc", EmitDefaultValue=false)]
    [JsonPropertyName("desc")]
    public bool? Desc { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class AggregationsSortDef {\n");
      sb.Append("  Field: ").Append(Field).Append("\n");
      sb.Append("  Desc: ").Append(Desc).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
