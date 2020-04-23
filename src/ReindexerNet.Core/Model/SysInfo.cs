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
  public class SysInfo {
    /// <summary>
    /// Server version
    /// </summary>
    /// <value>Server version</value>
    [DataMember(Name="version", EmitDefaultValue=false)]
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// Server uptime in seconds
    /// </summary>
    /// <value>Server uptime in seconds</value>
    [DataMember(Name="uptime", EmitDefaultValue=false)]
    [JsonPropertyName("uptime")]
    public long? Uptime { get; set; }

    /// <summary>
    /// Server start time in unix timestamp
    /// </summary>
    /// <value>Server start time in unix timestamp</value>
    [DataMember(Name="start_time", EmitDefaultValue=false)]
    [JsonPropertyName("start_time")]
    public long? StartTime { get; set; }

    /// <summary>
    /// Current heap size in bytes
    /// </summary>
    /// <value>Current heap size in bytes</value>
    [DataMember(Name="heap_size", EmitDefaultValue=false)]
    [JsonPropertyName("heap_size")]
    public long? HeapSize { get; set; }

    /// <summary>
    /// Current inuse allocated memory size in bytes
    /// </summary>
    /// <value>Current inuse allocated memory size in bytes</value>
    [DataMember(Name="current_allocated_bytes", EmitDefaultValue=false)]
    [JsonPropertyName("current_allocated_bytes")]
    public long? CurrentAllocatedBytes { get; set; }

    /// <summary>
    /// Heap free size in bytes
    /// </summary>
    /// <value>Heap free size in bytes</value>
    [DataMember(Name="pageheap_free", EmitDefaultValue=false)]
    [JsonPropertyName("pageheap_free")]
    public long? PageheapFree { get; set; }

    /// <summary>
    /// Unmapped free heap size in bytes
    /// </summary>
    /// <value>Unmapped free heap size in bytes</value>
    [DataMember(Name="pageheap_unmapped", EmitDefaultValue=false)]
    [JsonPropertyName("pageheap_unmapped")]
    public long? PageheapUnmapped { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class SysInfo {\n");
      sb.Append("  Version: ").Append(Version).Append("\n");
      sb.Append("  Uptime: ").Append(Uptime).Append("\n");
      sb.Append("  StartTime: ").Append(StartTime).Append("\n");
      sb.Append("  HeapSize: ").Append(HeapSize).Append("\n");
      sb.Append("  CurrentAllocatedBytes: ").Append(CurrentAllocatedBytes).Append("\n");
      sb.Append("  PageheapFree: ").Append(PageheapFree).Append("\n");
      sb.Append("  PageheapUnmapped: ").Append(PageheapUnmapped).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
