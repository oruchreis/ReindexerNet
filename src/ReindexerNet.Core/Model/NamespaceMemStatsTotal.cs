using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Summary of total namespace memory consumption
  /// </summary>
  [DataContract]
  public class NamespaceMemStatsTotal {
    /// <summary>
    /// Total memory size of stored documents, including system structures
    /// </summary>
    /// <value>Total memory size of stored documents, including system structures</value>
    [DataMember(Name="data_size", EmitDefaultValue=false)]
    [JsonPropertyName("data_size")]
    public int? DataSize { get; set; }

    /// <summary>
    /// Total memory consumption of namespace's indexes
    /// </summary>
    /// <value>Total memory consumption of namespace's indexes</value>
    [DataMember(Name="indexes_size", EmitDefaultValue=false)]
    [JsonPropertyName("indexes_size")]
    public int? IndexesSize { get; set; }

    /// <summary>
    /// Total memory consumption of namespace's caches. e.g. idset and join caches
    /// </summary>
    /// <value>Total memory consumption of namespace's caches. e.g. idset and join caches</value>
    [DataMember(Name="cache_size", EmitDefaultValue=false)]
    [JsonPropertyName("cache_size")]
    public int? CacheSize { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class NamespaceMemStatsTotal {\n");
      sb.Append("  DataSize: ").Append(DataSize).Append("\n");
      sb.Append("  IndexesSize: ").Append(IndexesSize).Append("\n");
      sb.Append("  CacheSize: ").Append(CacheSize).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
