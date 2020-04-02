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
  public class ActivityStatsItems {
    /// <summary>
    /// Client identifier
    /// </summary>
    /// <value>Client identifier</value>
    [DataMember(Name="client", EmitDefaultValue=false)]
    [JsonPropertyName("client")]
    public string _Client { get; set; }

    /// <summary>
    /// User name
    /// </summary>
    /// <value>User name</value>
    [DataMember(Name="user", EmitDefaultValue=false)]
    [JsonPropertyName("user")]
    public string User { get; set; }

    /// <summary>
    /// Query text
    /// </summary>
    /// <value>Query text</value>
    [DataMember(Name="query", EmitDefaultValue=false)]
    [JsonPropertyName("query")]
    public string Query { get; set; }

    /// <summary>
    /// Query identifier
    /// </summary>
    /// <value>Query identifier</value>
    [DataMember(Name="query_id", EmitDefaultValue=false)]
    [JsonPropertyName("query_id")]
    public int? QueryId { get; set; }

    /// <summary>
    /// Query start time
    /// </summary>
    /// <value>Query start time</value>
    [DataMember(Name="query_start", EmitDefaultValue=false)]
    [JsonPropertyName("query_start")]
    public string QueryStart { get; set; }

    /// <summary>
    /// Current operation state
    /// </summary>
    /// <value>Current operation state</value>
    [DataMember(Name="state", EmitDefaultValue=false)]
    [JsonPropertyName("state")]
    public string State { get; set; }

    /// <summary>
    /// Gets or Sets LockDescription
    /// </summary>
    [DataMember(Name="lock_description", EmitDefaultValue=false)]
    [JsonPropertyName("lock_description")]
    public string LockDescription { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ActivityStatsItems {\n");
      sb.Append("  _Client: ").Append(_Client).Append("\n");
      sb.Append("  User: ").Append(User).Append("\n");
      sb.Append("  Query: ").Append(Query).Append("\n");
      sb.Append("  QueryId: ").Append(QueryId).Append("\n");
      sb.Append("  QueryStart: ").Append(QueryStart).Append("\n");
      sb.Append("  State: ").Append(State).Append("\n");
      sb.Append("  LockDescription: ").Append(LockDescription).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
