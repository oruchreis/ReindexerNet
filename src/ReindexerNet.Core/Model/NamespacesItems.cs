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
  public class NamespacesItems {
    /// <summary>
    /// Name of namespace
    /// </summary>
    /// <value>Name of namespace</value>
    [DataMember(Name="name", EmitDefaultValue=false)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// If true, then documents will be stored to disc storage, else all data will be lost on server shutdown
    /// </summary>
    /// <value>If true, then documents will be stored to disc storage, else all data will be lost on server shutdown</value>
    [DataMember(Name="storage_enabled", EmitDefaultValue=false)]
    [JsonPropertyName("storage_enabled")]
    public bool? StorageEnabled { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class NamespacesItems {\n");
      sb.Append("  Name: ").Append(Name).Append("\n");
      sb.Append("  StorageEnabled: ").Append(StorageEnabled).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
