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
  public class NamespaceStorage {
    /// <summary>
    /// If true, then documents will be stored to disc storage, else all data will be lost on server shutdown
    /// </summary>
    /// <value>If true, then documents will be stored to disc storage, else all data will be lost on server shutdown</value>
    [DataMember(Name="enabled", EmitDefaultValue=false)]
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class NamespaceStorage {\n");
      sb.Append("  Enabled: ").Append(Enabled).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
