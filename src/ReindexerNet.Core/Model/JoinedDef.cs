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
  public class JoinedDef {
    /// <summary>
    /// Namespace name
    /// </summary>
    /// <value>Namespace name</value>
    [DataMember(Name="namespace", EmitDefaultValue=false)]
    [JsonPropertyName("namespace")]
    public string Namespace { get; set; }

    /// <summary>
    /// Join type
    /// </summary>
    /// <value>Join type</value>
    [DataMember(Name="type", EmitDefaultValue=false)]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// Filter for results documents
    /// </summary>
    /// <value>Filter for results documents</value>
    [DataMember(Name="filters", EmitDefaultValue=false)]
    [JsonPropertyName("filters")]
    public List<FilterDef> Filters { get; set; }

    /// <summary>
    /// Gets or Sets Sort
    /// </summary>
    [DataMember(Name="sort", EmitDefaultValue=false)]
    [JsonPropertyName("sort")]
    public SortDef Sort { get; set; }

    /// <summary>
    /// Maximum count of returned items
    /// </summary>
    /// <value>Maximum count of returned items</value>
    [DataMember(Name="limit", EmitDefaultValue=false)]
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// Offset of first returned item
    /// </summary>
    /// <value>Offset of first returned item</value>
    [DataMember(Name="offset", EmitDefaultValue=false)]
    [JsonPropertyName("offset")]
    public int? Offset { get; set; }

    /// <summary>
    /// Join ON statement
    /// </summary>
    /// <value>Join ON statement</value>
    [DataMember(Name="on", EmitDefaultValue=false)]
    [JsonPropertyName("on")]
    public List<OnDef> On { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class JoinedDef {\n");
      sb.Append("  Namespace: ").Append(Namespace).Append("\n");
      sb.Append("  Type: ").Append(Type).Append("\n");
      sb.Append("  Filters: ").Append(Filters).Append("\n");
      sb.Append("  Sort: ").Append(Sort).Append("\n");
      sb.Append("  Limit: ").Append(Limit).Append("\n");
      sb.Append("  Offset: ").Append(Offset).Append("\n");
      sb.Append("  On: ").Append(On).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
