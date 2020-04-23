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
  public class CommonPerfStats {
    /// <summary>
    /// Total count of queries to this object
    /// </summary>
    /// <value>Total count of queries to this object</value>
    [DataMember(Name="total_queries_count", EmitDefaultValue=false)]
    [JsonPropertyName("total_queries_count")]
    public long? TotalQueriesCount { get; set; }

    /// <summary>
    /// Average latency (execution time) for queries to this object
    /// </summary>
    /// <value>Average latency (execution time) for queries to this object</value>
    [DataMember(Name="total_avg_latency_us", EmitDefaultValue=false)]
    [JsonPropertyName("total_avg_latency_us")]
    public long? TotalAvgLatencyUs { get; set; }

    /// <summary>
    /// Average waiting time for acquiring lock to this object
    /// </summary>
    /// <value>Average waiting time for acquiring lock to this object</value>
    [DataMember(Name="total_avg_lock_time_us", EmitDefaultValue=false)]
    [JsonPropertyName("total_avg_lock_time_us")]
    public long? TotalAvgLockTimeUs { get; set; }

    /// <summary>
    /// Count of queries to this object, requested at last second
    /// </summary>
    /// <value>Count of queries to this object, requested at last second</value>
    [DataMember(Name="last_sec_qps", EmitDefaultValue=false)]
    [JsonPropertyName("last_sec_qps")]
    public long? LastSecQps { get; set; }

    /// <summary>
    /// Average latency (execution time) for queries to this object at last second
    /// </summary>
    /// <value>Average latency (execution time) for queries to this object at last second</value>
    [DataMember(Name="last_sec_avg_latency_us", EmitDefaultValue=false)]
    [JsonPropertyName("last_sec_avg_latency_us")]
    public long? LastSecAvgLatencyUs { get; set; }

    /// <summary>
    /// Average waiting time for acquiring lock to this object at last second
    /// </summary>
    /// <value>Average waiting time for acquiring lock to this object at last second</value>
    [DataMember(Name="last_sec_avg_lock_time_us", EmitDefaultValue=false)]
    [JsonPropertyName("last_sec_avg_lock_time_us")]
    public long? LastSecAvgLockTimeUs { get; set; }

    /// <summary>
    /// Standard deviation of latency values
    /// </summary>
    /// <value>Standard deviation of latency values</value>
    [DataMember(Name="latency_stddev", EmitDefaultValue=false)]
    [JsonPropertyName("latency_stddev")]
    public decimal? LatencyStddev { get; set; }

    /// <summary>
    /// Minimal latency value
    /// </summary>
    /// <value>Minimal latency value</value>
    [DataMember(Name="min_latency_us", EmitDefaultValue=false)]
    [JsonPropertyName("min_latency_us")]
    public long? MinLatencyUs { get; set; }

    /// <summary>
    /// Maximum latency value
    /// </summary>
    /// <value>Maximum latency value</value>
    [DataMember(Name="max_latency_us", EmitDefaultValue=false)]
    [JsonPropertyName("max_latency_us")]
    public long? MaxLatencyUs { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class CommonPerfStats {\n");
      sb.Append("  TotalQueriesCount: ").Append(TotalQueriesCount).Append("\n");
      sb.Append("  TotalAvgLatencyUs: ").Append(TotalAvgLatencyUs).Append("\n");
      sb.Append("  TotalAvgLockTimeUs: ").Append(TotalAvgLockTimeUs).Append("\n");
      sb.Append("  LastSecQps: ").Append(LastSecQps).Append("\n");
      sb.Append("  LastSecAvgLatencyUs: ").Append(LastSecAvgLatencyUs).Append("\n");
      sb.Append("  LastSecAvgLockTimeUs: ").Append(LastSecAvgLockTimeUs).Append("\n");
      sb.Append("  LatencyStddev: ").Append(LatencyStddev).Append("\n");
      sb.Append("  MinLatencyUs: ").Append(MinLatencyUs).Append("\n");
      sb.Append("  MaxLatencyUs: ").Append(MaxLatencyUs).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
