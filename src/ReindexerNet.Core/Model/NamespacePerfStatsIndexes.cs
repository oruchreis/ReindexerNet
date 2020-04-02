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
  public class NamespacePerfStatsIndexes {
    /// <summary>
    /// Name of index
    /// </summary>
    /// <value>Name of index</value>
    [DataMember(Name="name", EmitDefaultValue=false)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or Sets Updates
    /// </summary>
    [DataMember(Name="updates", EmitDefaultValue=false)]
    [JsonPropertyName("updates")]
    public UpdatePerfStats Updates { get; set; }

    /// <summary>
    /// Gets or Sets Selects
    /// </summary>
    [DataMember(Name="selects", EmitDefaultValue=false)]
    [JsonPropertyName("selects")]
    public SelectPerfStats Selects { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class NamespacePerfStatsIndexes {\n");
      sb.Append("  Name: ").Append(Name).Append("\n");
      sb.Append("  Updates: ").Append(Updates).Append("\n");
      sb.Append("  Selects: ").Append(Selects).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
