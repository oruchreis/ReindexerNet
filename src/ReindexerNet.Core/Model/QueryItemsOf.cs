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
  public class QueryItemsOf<T> {
    /// <summary>
    /// Documents, matched query
    /// </summary>
    /// <value>Documents, matched query</value>
    [DataMember(Name="items", EmitDefaultValue=false)]
    [JsonPropertyName("items")]
    public List<T> Items { get; set; }

    /// <summary>
    /// Namespaces, used in query
    /// </summary>
    /// <value>Namespaces, used in query</value>
    [DataMember(Name="namespaces", EmitDefaultValue=false)]
    [JsonPropertyName("namespaces")]
    public List<string> Namespaces { get; set; }

    /// <summary>
    /// Enables to client cache returned items. If false, then returned items has been modified  by reindexer, e.g. by select filter, or by functions, and can't be cached
    /// </summary>
    /// <value>Enables to client cache returned items. If false, then returned items has been modified  by reindexer, e.g. by select filter, or by functions, and can't be cached</value>
    [DataMember(Name="cache_enabled", EmitDefaultValue=false)]
    [JsonPropertyName("cache_enabled")]
    public bool? CacheEnabled { get; set; }

    /// <summary>
    /// Total count of documents, matched query
    /// </summary>
    /// <value>Total count of documents, matched query</value>
    [DataMember(Name="query_total_items", EmitDefaultValue=false)]
    [JsonPropertyName("query_total_items")]
    public int? QueryTotalItems { get; set; }

    /// <summary>
    /// Aggregation functions results
    /// </summary>
    /// <value>Aggregation functions results</value>
    [DataMember(Name="aggregations", EmitDefaultValue=false)]
    [JsonPropertyName("aggregations")]
    public List<AggregationResDef> Aggregations { get; set; }

    /// <summary>
    /// Array fields to be searched with equal array indexes
    /// </summary>
    /// <value>Array fields to be searched with equal array indexes</value>
    [DataMember(Name="equal_position", EmitDefaultValue=false)]
    [JsonPropertyName("equal_position")]
    public List<string> EqualPosition { get; set; }

    /// <summary>
    /// Columns for table output
    /// </summary>
    /// <value>Columns for table output</value>
    [DataMember(Name="columns", EmitDefaultValue=false)]
    [JsonPropertyName("columns")]
    public List<QueryColumnDef> Columns { get; set; }

    /// <summary>
    /// Gets or Sets Explain
    /// </summary>
    [DataMember(Name="explain", EmitDefaultValue=false)]
    [JsonPropertyName("explain")]
    public ExplainDef Explain { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.AppendFormat("class {0} {{\n", GetType().Name);
      sb.Append("  Items: ").Append(Items).Append("\n");
      sb.Append("  Namespaces: ").Append(Namespaces).Append("\n");
      sb.Append("  CacheEnabled: ").Append(CacheEnabled).Append("\n");
      sb.Append("  QueryTotalItems: ").Append(QueryTotalItems).Append("\n");
      sb.Append("  Aggregations: ").Append(Aggregations).Append("\n");
      sb.Append("  EqualPosition: ").Append(EqualPosition).Append("\n");
      sb.Append("  Columns: ").Append(Columns).Append("\n");
      sb.Append("  Explain: ").Append(Explain).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
