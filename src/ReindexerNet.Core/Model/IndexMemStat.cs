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
  public class IndexMemStat {
    /// <summary>
    /// Name of index. There are special index with name `-tuple`. It's stores original document's json structure with non indexe fields
    /// </summary>
    /// <value>Name of index. There are special index with name `-tuple`. It's stores original document's json structure with non indexe fields</value>
    [DataMember(Name="name", EmitDefaultValue=false)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Count of unique keys values stored in index
    /// </summary>
    /// <value>Count of unique keys values stored in index</value>
    [DataMember(Name="unique_keys_count", EmitDefaultValue=false)]
    [JsonPropertyName("unique_keys_count")]
    public int? UniqueKeysCount { get; set; }

    /// <summary>
    /// Total memory consumption of reverse index b-tree structures. For `dense` and `store` indexes always 0
    /// </summary>
    /// <value>Total memory consumption of reverse index b-tree structures. For `dense` and `store` indexes always 0</value>
    [DataMember(Name="idset_btree_size", EmitDefaultValue=false)]
    [JsonPropertyName("idset_btree_size")]
    public int? IdsetBtreeSize { get; set; }

    /// <summary>
    /// Total memory consumption of reverse index vectors. For `store` ndexes always 0
    /// </summary>
    /// <value>Total memory consumption of reverse index vectors. For `store` ndexes always 0</value>
    [DataMember(Name="idset_plain_size", EmitDefaultValue=false)]
    [JsonPropertyName("idset_plain_size")]
    public int? IdsetPlainSize { get; set; }

    /// <summary>
    /// Total memory consumption of SORT statement and `GT`, `LT` conditions optimized structures. Applicabe only to `tree` indexes
    /// </summary>
    /// <value>Total memory consumption of SORT statement and `GT`, `LT` conditions optimized structures. Applicabe only to `tree` indexes</value>
    [DataMember(Name="sort_orders_size", EmitDefaultValue=false)]
    [JsonPropertyName("sort_orders_size")]
    public int? SortOrdersSize { get; set; }

    /// <summary>
    /// Gets or Sets IdsetCache
    /// </summary>
    [DataMember(Name="idset_cache", EmitDefaultValue=false)]
    [JsonPropertyName("idset_cache")]
    public IndexCacheMemStats IdsetCache { get; set; }

    /// <summary>
    /// Total memory consumption of fulltext search structures
    /// </summary>
    /// <value>Total memory consumption of fulltext search structures</value>
    [DataMember(Name="fulltext_size", EmitDefaultValue=false)]
    [JsonPropertyName("fulltext_size")]
    public int? FulltextSize { get; set; }

    /// <summary>
    /// Total memory consumption of documents's data, holded by index
    /// </summary>
    /// <value>Total memory consumption of documents's data, holded by index</value>
    [DataMember(Name="data_size", EmitDefaultValue=false)]
    [JsonPropertyName("data_size")]
    public int? DataSize { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class IndexMemStat {\n");
      sb.Append("  Name: ").Append(Name).Append("\n");
      sb.Append("  UniqueKeysCount: ").Append(UniqueKeysCount).Append("\n");
      sb.Append("  IdsetBtreeSize: ").Append(IdsetBtreeSize).Append("\n");
      sb.Append("  IdsetPlainSize: ").Append(IdsetPlainSize).Append("\n");
      sb.Append("  SortOrdersSize: ").Append(SortOrdersSize).Append("\n");
      sb.Append("  IdsetCache: ").Append(IdsetCache).Append("\n");
      sb.Append("  FulltextSize: ").Append(FulltextSize).Append("\n");
      sb.Append("  DataSize: ").Append(DataSize).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
