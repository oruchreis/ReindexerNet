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
  public class MetaListResponseMeta {
    /// <summary>
    /// Gets or Sets Key
    /// </summary>
    [DataMember(Name="key", EmitDefaultValue=false)]
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    /// Optional: Provided if 'with_values' = true
    /// </summary>
    /// <value>Optional: Provided if 'with_values' = true</value>
    [DataMember(Name="value", EmitDefaultValue=false)]
    [JsonPropertyName("value")]
    public string Value { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class MetaListResponseMeta {\n");
      sb.Append("  Key: ").Append(Key).Append("\n");
      sb.Append("  Value: ").Append(Value).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
