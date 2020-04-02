using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Performance statistics for transactions
  /// </summary>
  [DataContract]
  public class TransactionsPerfStats {
    /// <summary>
    /// Total transactions count for this namespace
    /// </summary>
    /// <value>Total transactions count for this namespace</value>
    [DataMember(Name="total_count", EmitDefaultValue=false)]
    [JsonPropertyName("total_count")]
    public int? TotalCount { get; set; }

    /// <summary>
    /// Total namespace copy operations
    /// </summary>
    /// <value>Total namespace copy operations</value>
    [DataMember(Name="total_copy_count", EmitDefaultValue=false)]
    [JsonPropertyName("total_copy_count")]
    public int? TotalCopyCount { get; set; }

    /// <summary>
    /// Average steps count in transactions for this namespace
    /// </summary>
    /// <value>Average steps count in transactions for this namespace</value>
    [DataMember(Name="avg_steps_count", EmitDefaultValue=false)]
    [JsonPropertyName("avg_steps_count")]
    public int? AvgStepsCount { get; set; }

    /// <summary>
    /// Minimum steps count in transactions for this namespace
    /// </summary>
    /// <value>Minimum steps count in transactions for this namespace</value>
    [DataMember(Name="min_steps_count", EmitDefaultValue=false)]
    [JsonPropertyName("min_steps_count")]
    public int? MinStepsCount { get; set; }

    /// <summary>
    /// Maximum steps count in transactions for this namespace
    /// </summary>
    /// <value>Maximum steps count in transactions for this namespace</value>
    [DataMember(Name="max_steps_count", EmitDefaultValue=false)]
    [JsonPropertyName("max_steps_count")]
    public int? MaxStepsCount { get; set; }

    /// <summary>
    /// Average transaction preparation time usec
    /// </summary>
    /// <value>Average transaction preparation time usec</value>
    [DataMember(Name="avg_prepare_time_us", EmitDefaultValue=false)]
    [JsonPropertyName("avg_prepare_time_us")]
    public int? AvgPrepareTimeUs { get; set; }

    /// <summary>
    /// Minimum transaction preparation time usec
    /// </summary>
    /// <value>Minimum transaction preparation time usec</value>
    [DataMember(Name="min_prepare_time_us", EmitDefaultValue=false)]
    [JsonPropertyName("min_prepare_time_us")]
    public int? MinPrepareTimeUs { get; set; }

    /// <summary>
    /// Maximum transaction preparation time usec
    /// </summary>
    /// <value>Maximum transaction preparation time usec</value>
    [DataMember(Name="max_prepare_time_us", EmitDefaultValue=false)]
    [JsonPropertyName("max_prepare_time_us")]
    public int? MaxPrepareTimeUs { get; set; }

    /// <summary>
    /// Average transaction commit time usec
    /// </summary>
    /// <value>Average transaction commit time usec</value>
    [DataMember(Name="avg_commit_time_us", EmitDefaultValue=false)]
    [JsonPropertyName("avg_commit_time_us")]
    public int? AvgCommitTimeUs { get; set; }

    /// <summary>
    /// Minimum transaction commit time usec
    /// </summary>
    /// <value>Minimum transaction commit time usec</value>
    [DataMember(Name="min_commit_time_us", EmitDefaultValue=false)]
    [JsonPropertyName("min_commit_time_us")]
    public int? MinCommitTimeUs { get; set; }

    /// <summary>
    /// Maximum transaction commit time usec
    /// </summary>
    /// <value>Maximum transaction commit time usec</value>
    [DataMember(Name="max_commit_time_us", EmitDefaultValue=false)]
    [JsonPropertyName("max_commit_time_us")]
    public int? MaxCommitTimeUs { get; set; }

    /// <summary>
    /// Average namespace copy time usec
    /// </summary>
    /// <value>Average namespace copy time usec</value>
    [DataMember(Name="avg_copy_time_us", EmitDefaultValue=false)]
    [JsonPropertyName("avg_copy_time_us")]
    public int? AvgCopyTimeUs { get; set; }

    /// <summary>
    /// Maximum namespace copy time usec
    /// </summary>
    /// <value>Maximum namespace copy time usec</value>
    [DataMember(Name="min_copy_time_us", EmitDefaultValue=false)]
    [JsonPropertyName("min_copy_time_us")]
    public int? MinCopyTimeUs { get; set; }

    /// <summary>
    /// Minimum namespace copy time usec
    /// </summary>
    /// <value>Minimum namespace copy time usec</value>
    [DataMember(Name="max_copy_time_us", EmitDefaultValue=false)]
    [JsonPropertyName("max_copy_time_us")]
    public int? MaxCopyTimeUs { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class TransactionsPerfStats {\n");
      sb.Append("  TotalCount: ").Append(TotalCount).Append("\n");
      sb.Append("  TotalCopyCount: ").Append(TotalCopyCount).Append("\n");
      sb.Append("  AvgStepsCount: ").Append(AvgStepsCount).Append("\n");
      sb.Append("  MinStepsCount: ").Append(MinStepsCount).Append("\n");
      sb.Append("  MaxStepsCount: ").Append(MaxStepsCount).Append("\n");
      sb.Append("  AvgPrepareTimeUs: ").Append(AvgPrepareTimeUs).Append("\n");
      sb.Append("  MinPrepareTimeUs: ").Append(MinPrepareTimeUs).Append("\n");
      sb.Append("  MaxPrepareTimeUs: ").Append(MaxPrepareTimeUs).Append("\n");
      sb.Append("  AvgCommitTimeUs: ").Append(AvgCommitTimeUs).Append("\n");
      sb.Append("  MinCommitTimeUs: ").Append(MinCommitTimeUs).Append("\n");
      sb.Append("  MaxCommitTimeUs: ").Append(MaxCommitTimeUs).Append("\n");
      sb.Append("  AvgCopyTimeUs: ").Append(AvgCopyTimeUs).Append("\n");
      sb.Append("  MinCopyTimeUs: ").Append(MinCopyTimeUs).Append("\n");
      sb.Append("  MaxCopyTimeUs: ").Append(MaxCopyTimeUs).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
