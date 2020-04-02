using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Query columns for table outputs
  /// </summary>
  [DataContract]
  public class QueryColumnDef {
    /// <summary>
    /// Column name
    /// </summary>
    /// <value>Column name</value>
    [DataMember(Name="name", EmitDefaultValue=false)]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Column width in percents of total width
    /// </summary>
    /// <value>Column width in percents of total width</value>
    [DataMember(Name="width_percents", EmitDefaultValue=false)]
    [JsonPropertyName("width_percents")]
    public decimal? WidthPercents { get; set; }

    /// <summary>
    /// Column width in chars
    /// </summary>
    /// <value>Column width in chars</value>
    [DataMember(Name="width_chars", EmitDefaultValue=false)]
    [JsonPropertyName("width_chars")]
    public decimal? WidthChars { get; set; }

    /// <summary>
    /// Maximum count of chars in column
    /// </summary>
    /// <value>Maximum count of chars in column</value>
    [DataMember(Name="max_chars", EmitDefaultValue=false)]
    [JsonPropertyName("max_chars")]
    public decimal? MaxChars { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class QueryColumnDef {\n");
      sb.Append("  Name: ").Append(Name).Append("\n");
      sb.Append("  WidthPercents: ").Append(WidthPercents).Append("\n");
      sb.Append("  WidthChars: ").Append(WidthChars).Append("\n");
      sb.Append("  MaxChars: ").Append(MaxChars).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
