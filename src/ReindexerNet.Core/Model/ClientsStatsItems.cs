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
  public class ClientsStatsItems {
    /// <summary>
    /// Connection identifier
    /// </summary>
    /// <value>Connection identifier</value>
    [DataMember(Name="connection_id", EmitDefaultValue=false)]
    [JsonPropertyName("connection_id")]
    public long? ConnectionId { get; set; }

    /// <summary>
    /// Ip
    /// </summary>
    /// <value>Ip</value>
    [DataMember(Name="ip", EmitDefaultValue=false)]
    [JsonPropertyName("ip")]
    public string Ip { get; set; }

    /// <summary>
    /// User name
    /// </summary>
    /// <value>User name</value>
    [DataMember(Name="user_name", EmitDefaultValue=false)]
    [JsonPropertyName("user_name")]
    public string UserName { get; set; }

    /// <summary>
    /// User right
    /// </summary>
    /// <value>User right</value>
    [DataMember(Name="user_rights", EmitDefaultValue=false)]
    [JsonPropertyName("user_rights")]
    public string UserRights { get; set; }

    /// <summary>
    /// Database name
    /// </summary>
    /// <value>Database name</value>
    [DataMember(Name="db_name", EmitDefaultValue=false)]
    [JsonPropertyName("db_name")]
    public string DbName { get; set; }

    /// <summary>
    /// Current activity
    /// </summary>
    /// <value>Current activity</value>
    [DataMember(Name="current_activity", EmitDefaultValue=false)]
    [JsonPropertyName("current_activity")]
    public string CurrentActivity { get; set; }

    /// <summary>
    /// Server start time in unix timestamp
    /// </summary>
    /// <value>Server start time in unix timestamp</value>
    [DataMember(Name="start_time", EmitDefaultValue=false)]
    [JsonPropertyName("start_time")]
    public long? StartTime { get; set; }

    /// <summary>
    /// Receive byte
    /// </summary>
    /// <value>Receive byte</value>
    [DataMember(Name="recv_bytes", EmitDefaultValue=false)]
    [JsonPropertyName("recv_bytes")]
    public long? RecvBytes { get; set; }

    /// <summary>
    /// Send byte
    /// </summary>
    /// <value>Send byte</value>
    [DataMember(Name="sent_bytes", EmitDefaultValue=false)]
    [JsonPropertyName("sent_bytes")]
    public long? SentBytes { get; set; }

    /// <summary>
    /// Client version string
    /// </summary>
    /// <value>Client version string</value>
    [DataMember(Name="client_version", EmitDefaultValue=false)]
    [JsonPropertyName("client_version")]
    public string ClientVersion { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ClientsStatsItems {\n");
      sb.Append("  ConnectionId: ").Append(ConnectionId).Append("\n");
      sb.Append("  Ip: ").Append(Ip).Append("\n");
      sb.Append("  UserName: ").Append(UserName).Append("\n");
      sb.Append("  UserRights: ").Append(UserRights).Append("\n");
      sb.Append("  DbName: ").Append(DbName).Append("\n");
      sb.Append("  CurrentActivity: ").Append(CurrentActivity).Append("\n");
      sb.Append("  StartTime: ").Append(StartTime).Append("\n");
      sb.Append("  RecvBytes: ").Append(RecvBytes).Append("\n");
      sb.Append("  SentBytes: ").Append(SentBytes).Append("\n");
      sb.Append("  ClientVersion: ").Append(ClientVersion).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
