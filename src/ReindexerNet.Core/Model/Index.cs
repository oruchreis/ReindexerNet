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
  public class Index {
    /// <summary>
    /// Name of index, can contains letters, digits and underscores
    /// </summary>
    /// <value>Name of index, can contains letters, digits and underscores</value>
    [DataMember(Name="name", EmitDefaultValue=false)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Fields path in json object, e.g 'id' or 'subobject.field'. If index is 'composite' or 'is_array', than multiple json_paths can be specified, and index will get values from all specified fields.
    /// </summary>
    /// <value>Fields path in json object, e.g 'id' or 'subobject.field'. If index is 'composite' or 'is_array', than multiple json_paths can be specified, and index will get values from all specified fields.</value>
    [DataMember(Name="json_paths", EmitDefaultValue=false)]
    [JsonPropertyName("json_paths")]
    public List<string> JsonPaths { get; set; }

    /// <summary>
    /// Field data type
    /// </summary>
    /// <value>Field data type</value>
    [DataMember(Name="field_type", EmitDefaultValue=false)]
    [JsonPropertyName("field_type")]
    public FieldType FieldType { get; set; }

    /// <summary>
    /// Index structure type
    /// </summary>
    /// <value>Index structure type</value>
    [DataMember(Name="index_type", EmitDefaultValue=false)]
    [JsonPropertyName("index_type")]
    public IndexType IndexType { get; set; }

    /// <summary>
    /// Specifies, that index is primary key. The update opertations will checks, that PK field is unique. The namespace MUST have only 1 PK index
    /// </summary>
    /// <value>Specifies, that index is primary key. The update opertations will checks, that PK field is unique. The namespace MUST have only 1 PK index</value>
    [DataMember(Name="is_pk", EmitDefaultValue=false)]
    [JsonPropertyName("is_pk")]
    public bool? IsPk { get; set; }

    /// <summary>
    /// Specifies, that index is array. Array indexes can work with array fields, or work with multiple fields
    /// </summary>
    /// <value>Specifies, that index is array. Array indexes can work with array fields, or work with multiple fields</value>
    [DataMember(Name="is_array", EmitDefaultValue=false)]
    [JsonPropertyName("is_array")]
    public bool? IsArray { get; set; }

    /// <summary>
    /// Reduces the index size. For hash and tree it will save ~8 bytes per unique key value. Useful for indexes with high selectivity, but for tree and hash indexes with low selectivity can seriously decrease update performance;
    /// </summary>
    /// <value>Reduces the index size. For hash and tree it will save ~8 bytes per unique key value. Useful for indexes with high selectivity, but for tree and hash indexes with low selectivity can seriously decrease update performance;</value>
    [DataMember(Name="is_dense", EmitDefaultValue=false)]
    [JsonPropertyName("is_dense")]
    public bool? IsDense { get; set; }

    /// <summary>
    /// Value of index may not present in the document, and threfore, reduce data size but decreases speed operations on index
    /// </summary>
    /// <value>Value of index may not present in the document, and threfore, reduce data size but decreases speed operations on index</value>
    [DataMember(Name="is_sparse", EmitDefaultValue=false)]
    [JsonPropertyName("is_sparse")]
    public bool? IsSparse { get; set; }

    /// <summary>
    /// String collate mode
    /// </summary>
    /// <value>String collate mode</value>
    [DataMember(Name="collate_mode", EmitDefaultValue=false)]
    [JsonPropertyName("collate_mode")]
    public string CollateMode { get; set; }

    /// <summary>
    /// Sort order letters
    /// </summary>
    /// <value>Sort order letters</value>
    [DataMember(Name="sort_order_letters", EmitDefaultValue=false)]
    [JsonPropertyName("sort_order_letters")]
    public string SortOrderLetters { get; set; }

    /// <summary>
    /// Gets or Sets Config
    /// </summary>
    [DataMember(Name="config", EmitDefaultValue=false)]
    [JsonPropertyName("config")]
    public FulltextConfig Config { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class Index {\n");
      sb.Append("  Name: ").Append(Name).Append("\n");
      sb.Append("  JsonPaths: ").Append(JsonPaths).Append("\n");
      sb.Append("  FieldType: ").Append(FieldType).Append("\n");
      sb.Append("  IndexType: ").Append(IndexType).Append("\n");
      sb.Append("  IsPk: ").Append(IsPk).Append("\n");
      sb.Append("  IsArray: ").Append(IsArray).Append("\n");
      sb.Append("  IsDense: ").Append(IsDense).Append("\n");
      sb.Append("  IsSparse: ").Append(IsSparse).Append("\n");
      sb.Append("  CollateMode: ").Append(CollateMode).Append("\n");
      sb.Append("  SortOrderLetters: ").Append(SortOrderLetters).Append("\n");
      sb.Append("  Config: ").Append(Config).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
