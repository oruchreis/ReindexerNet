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
  public class OnDef {
    /// <summary>
    /// Field from left namespace (main query namespace)
    /// </summary>
    /// <value>Field from left namespace (main query namespace)</value>
    [DataMember(Name="left_field", EmitDefaultValue=false)]
    [JsonPropertyName("left_field")]
    public string LeftField { get; set; }

    /// <summary>
    /// Field from right namespace (joined query namespace)
    /// </summary>
    /// <value>Field from right namespace (joined query namespace)</value>
    [DataMember(Name="right_field", EmitDefaultValue=false)]
    [JsonPropertyName("right_field")]
    public string RightField { get; set; }

    /// <summary>
    /// Condition operator
    /// </summary>
    /// <value>Condition operator</value>
    [DataMember(Name="cond", EmitDefaultValue=false)]
    [JsonPropertyName("cond")]
    public string Cond { get; set; }

    /// <summary>
    /// Logic operator
    /// </summary>
    /// <value>Logic operator</value>
    [DataMember(Name="op", EmitDefaultValue=false)]
    [JsonPropertyName("op")]
    public string Op { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class OnDef {\n");
      sb.Append("  LeftField: ").Append(LeftField).Append("\n");
      sb.Append("  RightField: ").Append(RightField).Append("\n");
      sb.Append("  Cond: ").Append(Cond).Append("\n");
      sb.Append("  Op: ").Append(Op).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
