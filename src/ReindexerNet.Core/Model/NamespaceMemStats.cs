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
  public class NamespaceMemStats {
    /// <summary>
    /// Name of namespace
    /// </summary>
    /// <value>Name of namespace</value>
    [DataMember(Name="name", EmitDefaultValue=false)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Total count of documents in namespace
    /// </summary>
    /// <value>Total count of documents in namespace</value>
    [DataMember(Name="items_count", EmitDefaultValue=false)]
    [JsonPropertyName("items_count")]
    public long? ItemsCount { get; set; }

    /// <summary>
    /// Raw size of documents, stored in the namespace, except string fields
    /// </summary>
    /// <value>Raw size of documents, stored in the namespace, except string fields</value>
    [DataMember(Name="data_size", EmitDefaultValue=false)]
    [JsonPropertyName("data_size")]
    public long? DataSize { get; set; }

    /// <summary>
    /// [[deperecated]]. do not use
    /// </summary>
    /// <value>[[deperecated]]. do not use</value>
    [DataMember(Name="updated_unix_nano", EmitDefaultValue=false)]
    [JsonPropertyName("updated_unix_nano")]
    public long? UpdatedUnixNano { get; set; }

    /// <summary>
    /// Status of disk storage
    /// </summary>
    /// <value>Status of disk storage</value>
    [DataMember(Name="storage_ok", EmitDefaultValue=false)]
    [JsonPropertyName("storage_ok")]
    public bool? StorageOk { get; set; }

    /// <summary>
    /// Filesystem path to namespace storage
    /// </summary>
    /// <value>Filesystem path to namespace storage</value>
    [DataMember(Name="storage_path", EmitDefaultValue=false)]
    [JsonPropertyName("storage_path")]
    public string StoragePath { get; set; }

    /// <summary>
    /// Background indexes optimization has been completed
    /// </summary>
    /// <value>Background indexes optimization has been completed</value>
    [DataMember(Name="optimization_completed", EmitDefaultValue=false)]
    [JsonPropertyName("optimization_completed")]
    public bool? OptimizationCompleted { get; set; }

    /// <summary>
    /// Gets or Sets Total
    /// </summary>
    [DataMember(Name="total", EmitDefaultValue=false)]
    [JsonPropertyName("total")]
    public NamespaceMemStatsTotal Total { get; set; }

    /// <summary>
    /// Gets or Sets JoinCache
    /// </summary>
    [DataMember(Name="join_cache", EmitDefaultValue=false)]
    [JsonPropertyName("join_cache")]
    public JoinCacheMemStats JoinCache { get; set; }

    /// <summary>
    /// Gets or Sets QueryCache
    /// </summary>
    [DataMember(Name="query_cache", EmitDefaultValue=false)]
    [JsonPropertyName("query_cache")]
    public QueryCacheMemStats QueryCache { get; set; }

    /// <summary>
    /// Gets or Sets Replication
    /// </summary>
    [DataMember(Name="replication", EmitDefaultValue=false)]
    [JsonPropertyName("replication")]
    public ReplicationStats Replication { get; set; }

    /// <summary>
    /// Memory consumption of each namespace index
    /// </summary>
    /// <value>Memory consumption of each namespace index</value>
    [DataMember(Name="indexes", EmitDefaultValue=false)]
    [JsonPropertyName("indexes")]
    public List<IndexMemStat> Indexes { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class NamespaceMemStats {\n");
      sb.Append("  Name: ").Append(Name).Append("\n");
      sb.Append("  ItemsCount: ").Append(ItemsCount).Append("\n");
      sb.Append("  DataSize: ").Append(DataSize).Append("\n");
      sb.Append("  UpdatedUnixNano: ").Append(UpdatedUnixNano).Append("\n");
      sb.Append("  StorageOk: ").Append(StorageOk).Append("\n");
      sb.Append("  StoragePath: ").Append(StoragePath).Append("\n");
      sb.Append("  OptimizationCompleted: ").Append(OptimizationCompleted).Append("\n");
      sb.Append("  Total: ").Append(Total).Append("\n");
      sb.Append("  JoinCache: ").Append(JoinCache).Append("\n");
      sb.Append("  QueryCache: ").Append(QueryCache).Append("\n");
      sb.Append("  Replication: ").Append(Replication).Append("\n");
      sb.Append("  Indexes: ").Append(Indexes).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
