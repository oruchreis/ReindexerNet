using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Specifies results sorting order
  /// </summary>
  [DataContract]
  public class SortDef {
    /// <summary>
    /// Field or index name for sorting
    /// </summary>
    /// <value>Field or index name for sorting</value>
    [DataMember(Name="field", EmitDefaultValue=false)]
    [JsonPropertyName("field")]
    public string Field { get; set; }

    /// <summary>
    /// Optional: Documents with this values of field will be returned first
    /// </summary>
    /// <value>Optional: Documents with this values of field will be returned first</value>
    [DataMember(Name="values", EmitDefaultValue=false)]
    [JsonPropertyName("values")]
    public List<object> Values { get; set; }

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
      sb.AppendFormat("class {0} {{\n", GetType().Name);
      sb.Append("  Field: ").Append(Field).Append("\n");
      sb.Append("  Values: ").Append(Values).Append("\n");
      sb.Append("  Desc: ").Append(Desc).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
