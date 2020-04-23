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
  public class ExplainDefSelectors {
    /// <summary>
    /// Method, used to process condition
    /// </summary>
    /// <value>Method, used to process condition</value>
    [DataMember(Name="method", EmitDefaultValue=false)]
    [JsonPropertyName("method")]
    public string Method { get; set; }

    /// <summary>
    /// Field or index name
    /// </summary>
    /// <value>Field or index name</value>
    [DataMember(Name="field", EmitDefaultValue=false)]
    [JsonPropertyName("field")]
    public string Field { get; set; }

    /// <summary>
    /// Count of scanned documents by this selector
    /// </summary>
    /// <value>Count of scanned documents by this selector</value>
    [DataMember(Name="items", EmitDefaultValue=false)]
    [JsonPropertyName("items")]
    public long? Items { get; set; }

    /// <summary>
    /// Count of processed documents, matched this selector
    /// </summary>
    /// <value>Count of processed documents, matched this selector</value>
    [DataMember(Name="matched", EmitDefaultValue=false)]
    [JsonPropertyName("matched")]
    public long? Matched { get; set; }

    /// <summary>
    /// Count of comparators used, for this selector
    /// </summary>
    /// <value>Count of comparators used, for this selector</value>
    [DataMember(Name="comparators", EmitDefaultValue=false)]
    [JsonPropertyName("comparators")]
    public long? Comparators { get; set; }

    /// <summary>
    /// Cost expectation of this selector
    /// </summary>
    /// <value>Cost expectation of this selector</value>
    [DataMember(Name="cost", EmitDefaultValue=false)]
    [JsonPropertyName("cost")]
    public long? Cost { get; set; }

    /// <summary>
    /// Number of uniq keys, processed by this selector (may be incorrect, in case of internal query optimization/caching
    /// </summary>
    /// <value>Number of uniq keys, processed by this selector (may be incorrect, in case of internal query optimization/caching</value>
    [DataMember(Name="keys", EmitDefaultValue=false)]
    [JsonPropertyName("keys")]
    public long? Keys { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ExplainDefSelectors {\n");
      sb.Append("  Method: ").Append(Method).Append("\n");
      sb.Append("  Field: ").Append(Field).Append("\n");
      sb.Append("  Items: ").Append(Items).Append("\n");
      sb.Append("  Matched: ").Append(Matched).Append("\n");
      sb.Append("  Comparators: ").Append(Comparators).Append("\n");
      sb.Append("  Cost: ").Append(Cost).Append("\n");
      sb.Append("  Keys: ").Append(Keys).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
