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
  public class ProfilingConfig {
    /// <summary>
    /// Enables tracking activity statistics
    /// </summary>
    /// <value>Enables tracking activity statistics</value>
    [DataMember(Name="activitystats", EmitDefaultValue=false)]
    [JsonPropertyName("activitystats")]
    public bool? Activitystats { get; set; }

    /// <summary>
    /// Enables tracking memory statistics
    /// </summary>
    /// <value>Enables tracking memory statistics</value>
    [DataMember(Name="memstats", EmitDefaultValue=false)]
    [JsonPropertyName("memstats")]
    public bool? Memstats { get; set; }

    /// <summary>
    /// Enables tracking overal perofrmance statistics
    /// </summary>
    /// <value>Enables tracking overal perofrmance statistics</value>
    [DataMember(Name="perfstats", EmitDefaultValue=false)]
    [JsonPropertyName("perfstats")]
    public bool? Perfstats { get; set; }

    /// <summary>
    /// Enables record queries perofrmance statistics
    /// </summary>
    /// <value>Enables record queries perofrmance statistics</value>
    [DataMember(Name="queriesperfstats", EmitDefaultValue=false)]
    [JsonPropertyName("queriesperfstats")]
    public bool? Queriesperfstats { get; set; }

    /// <summary>
    /// Minimum query execution time to be recoreded in #queriesperfstats namespace
    /// </summary>
    /// <value>Minimum query execution time to be recoreded in #queriesperfstats namespace</value>
    [DataMember(Name="queries_threshold_us", EmitDefaultValue=false)]
    [JsonPropertyName("queries_threshold_us")]
    public long? QueriesThresholdUs { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ProfilingConfig {\n");
      sb.Append("  Activitystats: ").Append(Activitystats).Append("\n");
      sb.Append("  Memstats: ").Append(Memstats).Append("\n");
      sb.Append("  Perfstats: ").Append(Perfstats).Append("\n");
      sb.Append("  Queriesperfstats: ").Append(Queriesperfstats).Append("\n");
      sb.Append("  QueriesThresholdUs: ").Append(QueriesThresholdUs).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
