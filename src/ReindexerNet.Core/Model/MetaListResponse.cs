using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// List of meta info of the specified namespace
  /// </summary>
  [DataContract]
  public class MetaListResponse {
    /// <summary>
    /// Total count of meta info in the namespace
    /// </summary>
    /// <value>Total count of meta info in the namespace</value>
    [DataMember(Name="total_items", EmitDefaultValue=false)]
    [JsonPropertyName("total_items")]
    public int? TotalItems { get; set; }

    /// <summary>
    /// Gets or Sets Meta
    /// </summary>
    [DataMember(Name="meta", EmitDefaultValue=false)]
    [JsonPropertyName("meta")]
    public List<MetaListResponseMeta> Meta { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class MetaListResponse {\n");
      sb.Append("  TotalItems: ").Append(TotalItems).Append("\n");
      sb.Append("  Meta: ").Append(Meta).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
