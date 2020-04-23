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
  public class NamespacesConfig {
    /// <summary>
    /// Name of namespace, or `*` for setting to all namespaces
    /// </summary>
    /// <value>Name of namespace, or `*` for setting to all namespaces</value>
    [DataMember(Name="namespace", EmitDefaultValue=false)]
    [JsonPropertyName("namespace")]
    public string Namespace { get; set; }

    /// <summary>
    /// Log level of queries core logger
    /// </summary>
    /// <value>Log level of queries core logger</value>
    [DataMember(Name="log_level", EmitDefaultValue=false)]
    [JsonPropertyName("log_level")]
    public string LogLevel { get; set; }

    /// <summary>
    /// Join cache mode
    /// </summary>
    /// <value>Join cache mode</value>
    [DataMember(Name="join_cache_mode", EmitDefaultValue=false)]
    [JsonPropertyName("join_cache_mode")]
    public string JoinCacheMode { get; set; }

    /// <summary>
    /// Enable namespace lazy load (namespace shoud be loaded from disk on first call, not at reindexer startup)
    /// </summary>
    /// <value>Enable namespace lazy load (namespace shoud be loaded from disk on first call, not at reindexer startup)</value>
    [DataMember(Name="lazyload", EmitDefaultValue=false)]
    [JsonPropertyName("lazyload")]
    public bool? Lazyload { get; set; }

    /// <summary>
    /// Unload namespace data from RAM after this idle timeout in seconds. If 0, then data should not be unloaded
    /// </summary>
    /// <value>Unload namespace data from RAM after this idle timeout in seconds. If 0, then data should not be unloaded</value>
    [DataMember(Name="unload_idle_threshold", EmitDefaultValue=false)]
    [JsonPropertyName("unload_idle_threshold")]
    public long? UnloadIdleThreshold { get; set; }

    /// <summary>
    /// Enable namespace copying for transaction with steps count greater than this value (if copy_politics_multiplier also allows this)
    /// </summary>
    /// <value>Enable namespace copying for transaction with steps count greater than this value (if copy_politics_multiplier also allows this)</value>
    [DataMember(Name="start_copy_policy_tx_size", EmitDefaultValue=false)]
    [JsonPropertyName("start_copy_policy_tx_size")]
    public long? StartCopyPolicyTxSize { get; set; }

    /// <summary>
    /// Disables copy policy if namespace size is greater than copy_policy_multiplier * start_copy_policy_tx_size
    /// </summary>
    /// <value>Disables copy policy if namespace size is greater than copy_policy_multiplier * start_copy_policy_tx_size</value>
    [DataMember(Name="copy_policy_multiplier", EmitDefaultValue=false)]
    [JsonPropertyName("copy_policy_multiplier")]
    public long? CopyPolicyMultiplier { get; set; }

    /// <summary>
    /// Force namespace copying for transaction with steps count greater than this value
    /// </summary>
    /// <value>Force namespace copying for transaction with steps count greater than this value</value>
    [DataMember(Name="tx_size_to_always_copy", EmitDefaultValue=false)]
    [JsonPropertyName("tx_size_to_always_copy")]
    public long? TxSizeToAlwaysCopy { get; set; }

    /// <summary>
    /// Timeout before background indexes optimization start after last update. 0 - disable optimizations
    /// </summary>
    /// <value>Timeout before background indexes optimization start after last update. 0 - disable optimizations</value>
    [DataMember(Name="optimization_timeout_ms", EmitDefaultValue=false)]
    [JsonPropertyName("optimization_timeout_ms")]
    public long? OptimizationTimeoutMs { get; set; }

    /// <summary>
    /// Maximum number of background threads of sort indexes optimization. 0 - disable sort optimizations
    /// </summary>
    /// <value>Maximum number of background threads of sort indexes optimization. 0 - disable sort optimizations</value>
    [DataMember(Name="optimization_sort_workers", EmitDefaultValue=false)]
    [JsonPropertyName("optimization_sort_workers")]
    public long? OptimizationSortWorkers { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class NamespacesConfig {\n");
      sb.Append("  Namespace: ").Append(Namespace).Append("\n");
      sb.Append("  LogLevel: ").Append(LogLevel).Append("\n");
      sb.Append("  JoinCacheMode: ").Append(JoinCacheMode).Append("\n");
      sb.Append("  Lazyload: ").Append(Lazyload).Append("\n");
      sb.Append("  UnloadIdleThreshold: ").Append(UnloadIdleThreshold).Append("\n");
      sb.Append("  StartCopyPolicyTxSize: ").Append(StartCopyPolicyTxSize).Append("\n");
      sb.Append("  CopyPolicyMultiplier: ").Append(CopyPolicyMultiplier).Append("\n");
      sb.Append("  TxSizeToAlwaysCopy: ").Append(TxSizeToAlwaysCopy).Append("\n");
      sb.Append("  OptimizationTimeoutMs: ").Append(OptimizationTimeoutMs).Append("\n");
      sb.Append("  OptimizationSortWorkers: ").Append(OptimizationSortWorkers).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
