using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Query execution explainings
  /// </summary>
  [DataContract]
  public class ExplainDef {
    /// <summary>
    /// Total query execution time
    /// </summary>
    /// <value>Total query execution time</value>
    [DataMember(Name="total_us", EmitDefaultValue=false)]
    [JsonPropertyName("total_us")]
    public int? TotalUs { get; set; }

    /// <summary>
    /// Intersection loop time
    /// </summary>
    /// <value>Intersection loop time</value>
    [DataMember(Name="loop_us", EmitDefaultValue=false)]
    [JsonPropertyName("loop_us")]
    public int? LoopUs { get; set; }

    /// <summary>
    /// Indexes keys selection time
    /// </summary>
    /// <value>Indexes keys selection time</value>
    [DataMember(Name="indexes_us", EmitDefaultValue=false)]
    [JsonPropertyName("indexes_us")]
    public int? IndexesUs { get; set; }

    /// <summary>
    /// Query post process time
    /// </summary>
    /// <value>Query post process time</value>
    [DataMember(Name="postprocess_us", EmitDefaultValue=false)]
    [JsonPropertyName("postprocess_us")]
    public int? PostprocessUs { get; set; }

    /// <summary>
    /// Query prepare and optimize time
    /// </summary>
    /// <value>Query prepare and optimize time</value>
    [DataMember(Name="prepare_us", EmitDefaultValue=false)]
    [JsonPropertyName("prepare_us")]
    public int? PrepareUs { get; set; }

    /// <summary>
    /// Index, which used for sort results
    /// </summary>
    /// <value>Index, which used for sort results</value>
    [DataMember(Name="sort_index", EmitDefaultValue=false)]
    [JsonPropertyName("sort_index")]
    public string SortIndex { get; set; }

    /// <summary>
    /// Filter selectos, used to proccess query conditions
    /// </summary>
    /// <value>Filter selectos, used to proccess query conditions</value>
    [DataMember(Name="selectors", EmitDefaultValue=false)]
    [JsonPropertyName("selectors")]
    public List<ExplainDefSelectors> Selectors { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ExplainDef {\n");
      sb.Append("  TotalUs: ").Append(TotalUs).Append("\n");
      sb.Append("  LoopUs: ").Append(LoopUs).Append("\n");
      sb.Append("  IndexesUs: ").Append(IndexesUs).Append("\n");
      sb.Append("  PostprocessUs: ").Append(PostprocessUs).Append("\n");
      sb.Append("  PrepareUs: ").Append(PrepareUs).Append("\n");
      sb.Append("  SortIndex: ").Append(SortIndex).Append("\n");
      sb.Append("  Selectors: ").Append(Selectors).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
