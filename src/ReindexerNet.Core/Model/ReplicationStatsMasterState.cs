using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// State of current master namespace
  /// </summary>
  [DataContract]
  public class ReplicationStatsMasterState {
    /// <summary>
    /// Hashsum of all records in namespace
    /// </summary>
    /// <value>Hashsum of all records in namespace</value>
    [DataMember(Name="data_hash", EmitDefaultValue=false)]
    [JsonPropertyName("data_hash")]
    public long? DataHash { get; set; }

    /// <summary>
    /// Last Log Sequence Number (LSN) of applied namespace modification
    /// </summary>
    /// <value>Last Log Sequence Number (LSN) of applied namespace modification</value>
    [DataMember(Name="last_lsn", EmitDefaultValue=false)]
    [JsonPropertyName("last_lsn")]
    public long? LastLsn { get; set; }

    /// <summary>
    /// Last update time
    /// </summary>
    /// <value>Last update time</value>
    [DataMember(Name="updated_unix_nano", EmitDefaultValue=false)]
    [JsonPropertyName("updated_unix_nano")]
    public long? UpdatedUnixNano { get; set; }

    /// <summary>
    /// Items count in master namespace
    /// </summary>
    /// <value>Items count in master namespace</value>
    [DataMember(Name="data_count", EmitDefaultValue=false)]
    [JsonPropertyName("data_count")]
    public long? DataCount { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ReplicationStatsMasterState {\n");
      sb.Append("  DataHash: ").Append(DataHash).Append("\n");
      sb.Append("  LastLsn: ").Append(LastLsn).Append("\n");
      sb.Append("  UpdatedUnixNano: ").Append(UpdatedUnixNano).Append("\n");
      sb.Append("  DataCount: ").Append(DataCount).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
