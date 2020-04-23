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
  public class CacheMemStats {
    /// <summary>
    /// Total memory consumption by this cache
    /// </summary>
    /// <value>Total memory consumption by this cache</value>
    [DataMember(Name="total_size", EmitDefaultValue=false)]
    [JsonPropertyName("total_size")]
    public long? TotalSize { get; set; }

    /// <summary>
    /// Count of used elements stored in this cache
    /// </summary>
    /// <value>Count of used elements stored in this cache</value>
    [DataMember(Name="items_count", EmitDefaultValue=false)]
    [JsonPropertyName("items_count")]
    public long? ItemsCount { get; set; }

    /// <summary>
    /// Count of empty elements slots in this cache
    /// </summary>
    /// <value>Count of empty elements slots in this cache</value>
    [DataMember(Name="empty_count", EmitDefaultValue=false)]
    [JsonPropertyName("empty_count")]
    public long? EmptyCount { get; set; }

    /// <summary>
    /// Number of hits of queries, to store results in cache
    /// </summary>
    /// <value>Number of hits of queries, to store results in cache</value>
    [DataMember(Name="hit_count_limit", EmitDefaultValue=false)]
    [JsonPropertyName("hit_count_limit")]
    public long? HitCountLimit { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class CacheMemStats {\n");
      sb.Append("  TotalSize: ").Append(TotalSize).Append("\n");
      sb.Append("  ItemsCount: ").Append(ItemsCount).Append("\n");
      sb.Append("  EmptyCount: ").Append(EmptyCount).Append("\n");
      sb.Append("  HitCountLimit: ").Append(HitCountLimit).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
