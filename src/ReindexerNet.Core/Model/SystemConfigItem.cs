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
  public class SystemConfigItem {
    /// <summary>
    /// Gets or Sets Type
    /// </summary>
    [DataMember(Name="type", EmitDefaultValue=false)]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or Sets Profiling
    /// </summary>
    [DataMember(Name="profiling", EmitDefaultValue=false)]
    [JsonPropertyName("profiling")]
    public ProfilingConfig Profiling { get; set; }

    /// <summary>
    /// Gets or Sets Namespaces
    /// </summary>
    [DataMember(Name="namespaces", EmitDefaultValue=false)]
    [JsonPropertyName("namespaces")]
    public List<NamespacesConfig> Namespaces { get; set; }

    /// <summary>
    /// Gets or Sets Replication
    /// </summary>
    [DataMember(Name="replication", EmitDefaultValue=false)]
    [JsonPropertyName("replication")]
    public ReplicationConfig Replication { get; set; }

    /// <summary>
    /// Gets or Sets Action
    /// </summary>
    [DataMember(Name="action", EmitDefaultValue=false)]
    [JsonPropertyName("action")]
    public ActionCommand Action { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class SystemConfigItem {\n");
      sb.Append("  Type: ").Append(Type).Append("\n");
      sb.Append("  Profiling: ").Append(Profiling).Append("\n");
      sb.Append("  Namespaces: ").Append(Namespaces).Append("\n");
      sb.Append("  Replication: ").Append(Replication).Append("\n");
      sb.Append("  Action: ").Append(Action).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
