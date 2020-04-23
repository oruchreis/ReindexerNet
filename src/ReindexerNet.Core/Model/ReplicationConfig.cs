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
  public class ReplicationConfig {
    /// <summary>
    /// Replication role
    /// </summary>
    /// <value>Replication role</value>
    [DataMember(Name="role", EmitDefaultValue=false)]
    [JsonPropertyName("role")]
    public string Role { get; set; }

    /// <summary>
    /// DSN to master. Only cproto schema is supported
    /// </summary>
    /// <value>DSN to master. Only cproto schema is supported</value>
    [DataMember(Name="master_dsn", EmitDefaultValue=false)]
    [JsonPropertyName("master_dsn")]
    public string MasterDsn { get; set; }

    /// <summary>
    /// Network timeout for communication with master, in seconds
    /// </summary>
    /// <value>Network timeout for communication with master, in seconds</value>
    [DataMember(Name="timeout_sec", EmitDefaultValue=false)]
    [JsonPropertyName("timeout_sec")]
    public long? TimeoutSec { get; set; }

    /// <summary>
    /// Cluser ID - must be same for client and for master
    /// </summary>
    /// <value>Cluser ID - must be same for client and for master</value>
    [DataMember(Name="cluster_id", EmitDefaultValue=false)]
    [JsonPropertyName("cluster_id")]
    public long? ClusterId { get; set; }

    /// <summary>
    /// force resync on logic error conditions
    /// </summary>
    /// <value>force resync on logic error conditions</value>
    [DataMember(Name="force_sync_on_logic_error", EmitDefaultValue=false)]
    [JsonPropertyName("force_sync_on_logic_error")]
    public bool? ForceSyncOnLogicError { get; set; }

    /// <summary>
    /// force resync on wrong data hash conditions
    /// </summary>
    /// <value>force resync on wrong data hash conditions</value>
    [DataMember(Name="force_sync_on_wrong_data_hash", EmitDefaultValue=false)]
    [JsonPropertyName("force_sync_on_wrong_data_hash")]
    public bool? ForceSyncOnWrongDataHash { get; set; }

    /// <summary>
    /// List of namespaces for replication. If emply, all namespaces. All replicated namespaces will become read only for slave
    /// </summary>
    /// <value>List of namespaces for replication. If emply, all namespaces. All replicated namespaces will become read only for slave</value>
    [DataMember(Name="namespaces", EmitDefaultValue=false)]
    [JsonPropertyName("namespaces")]
    public List<string> Namespaces { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ReplicationConfig {\n");
      sb.Append("  Role: ").Append(Role).Append("\n");
      sb.Append("  MasterDsn: ").Append(MasterDsn).Append("\n");
      sb.Append("  TimeoutSec: ").Append(TimeoutSec).Append("\n");
      sb.Append("  ClusterId: ").Append(ClusterId).Append("\n");
      sb.Append("  ForceSyncOnLogicError: ").Append(ForceSyncOnLogicError).Append("\n");
      sb.Append("  ForceSyncOnWrongDataHash: ").Append(ForceSyncOnWrongDataHash).Append("\n");
      sb.Append("  Namespaces: ").Append(Namespaces).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
