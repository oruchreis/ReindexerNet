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
  public class Namespace {
    /// <summary>
    /// Name of namespace
    /// </summary>
    /// <value>Name of namespace</value>
    [DataMember(Name="name", EmitDefaultValue=false)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or Sets Storage
    /// </summary>
    [DataMember(Name="storage", EmitDefaultValue=false)]
    [JsonPropertyName("storage")]
    public NamespaceStorage Storage { get; set; }

    /// <summary>
    /// Gets or Sets Indexes
    /// </summary>
    [DataMember(Name="indexes", EmitDefaultValue=false)]
    [JsonPropertyName("indexes")]
    public List<Index> Indexes { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class Namespace {\n");
      sb.Append("  Name: ").Append(Name).Append("\n");
      sb.Append("  Storage: ").Append(Storage).Append("\n");
      sb.Append("  Indexes: ").Append(Indexes).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
