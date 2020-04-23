using System.Text;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet
{
    /// <summary>
    /// State of namespace replication
    /// </summary>
    [DataContract]
    public class ReplicationStats
    {
        /// <summary>
        /// Last Log Sequence Number (LSN) of applied namespace modification
        /// </summary>
        /// <value>Last Log Sequence Number (LSN) of applied namespace modification</value>
        [DataMember(Name = "last_lsn", EmitDefaultValue = false)]
        [JsonPropertyName("last_lsn")]
        public long? LastLsn { get; set; }

        /// <summary>
        /// Cluster ID - must be same for client and for master
        /// </summary>
        /// <value>Cluster ID - must be same for client and for master</value>
        [DataMember(Name = "cluster_id", EmitDefaultValue = false)]
        [JsonPropertyName("cluster_id")]
        public int? ClusterId { get; set; }

        /// <summary>
        /// If true, then namespace is in slave mode
        /// </summary>
        /// <value>If true, then namespace is in slave mode</value>
        [DataMember(Name = "slave_mode", EmitDefaultValue = false)]
        [JsonPropertyName("slave_mode")]
        public bool? SlaveMode { get; set; }

        /// <summary>
        /// Error code of last replication
        /// </summary>
        /// <value>Error code of last replication</value>
        [DataMember(Name = "error_code", EmitDefaultValue = false)]
        [JsonPropertyName("error_code")]
        public int? ErrorCode { get; set; }

        /// <summary>
        /// Error message of last replication
        /// </summary>
        /// <value>Error message of last replication</value>
        [DataMember(Name = "error_message", EmitDefaultValue = false)]
        [JsonPropertyName("error_message")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Current replication status for this namespace
        /// </summary>
        /// <value>Current replication status for this namespace</value>
        [DataMember(Name = "status", EmitDefaultValue = false)]
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or Sets MasterState
        /// </summary>
        [DataMember(Name = "master_state", EmitDefaultValue = false)]
        [JsonPropertyName("master_state")]
        public ReplicationStatsMasterState MasterState { get; set; }

        /// <summary>
        /// Number of storage's master - slave switches
        /// </summary>
        /// <value>Number of storage's master - slave switches</value>
        [DataMember(Name = "incarnation_counter", EmitDefaultValue = false)]
        [JsonPropertyName("incarnation_counter")]
        public long? IncarnationCounter { get; set; }

        /// <summary>
        /// Hashsum of all records in namespace
        /// </summary>
        /// <value>Hashsum of all records in namespace</value>
        [DataMember(Name = "data_hash", EmitDefaultValue = false)]
        [JsonPropertyName("data_hash")]
        public long? DataHash { get; set; }

        /// <summary>
        /// Write Ahead Log (WAL) records count
        /// </summary>
        /// <value>Write Ahead Log (WAL) records count</value>
        [DataMember(Name = "wal_count", EmitDefaultValue = false)]
        [JsonPropertyName("wal_count")]
        public long? WalCount { get; set; }

        /// <summary>
        /// Total memory consumption of Write Ahead Log (WAL)
        /// </summary>
        /// <value>Total memory consumption of Write Ahead Log (WAL)</value>
        [DataMember(Name = "wal_size", EmitDefaultValue = false)]
        [JsonPropertyName("wal_size")]
        public long? WalSize { get; set; }

        /// <summary>
        /// Last update time
        /// </summary>
        /// <value>Last update time</value>
        [DataMember(Name = "updated_unix_nano", EmitDefaultValue = false)]
        [JsonPropertyName("updated_unix_nano")]
        public long? UpdatedUnixNano { get; set; }

        /// <summary>
        /// Items count in namespace
        /// </summary>
        /// <value>Items count in namespace</value>
        [DataMember(Name = "data_count", EmitDefaultValue = false)]
        [JsonPropertyName("data_count")]
        public long? DataCount { get; set; }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ReplicationStats {\n");
            sb.Append("  LastLsn: ").Append(LastLsn).Append("\n");
            sb.Append("  ClusterId: ").Append(ClusterId).Append("\n");
            sb.Append("  SlaveMode: ").Append(SlaveMode).Append("\n");
            sb.Append("  ErrorCode: ").Append(ErrorCode).Append("\n");
            sb.Append("  ErrorMessage: ").Append(ErrorMessage).Append("\n");
            sb.Append("  Status: ").Append(Status).Append("\n");
            sb.Append("  MasterState: ").Append(MasterState).Append("\n");
            sb.Append("  IncarnationCounter: ").Append(IncarnationCounter).Append("\n");
            sb.Append("  DataHash: ").Append(DataHash).Append("\n");
            sb.Append("  WalCount: ").Append(WalCount).Append("\n");
            sb.Append("  WalSize: ").Append(WalSize).Append("\n");
            sb.Append("  UpdatedUnixNano: ").Append(UpdatedUnixNano).Append("\n");
            sb.Append("  DataCount: ").Append(DataCount).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
