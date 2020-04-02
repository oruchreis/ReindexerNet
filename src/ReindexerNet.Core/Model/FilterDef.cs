using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// If contains &#39;filters&#39; then cannot contain &#39;cond&#39;, &#39;field&#39; and &#39;value&#39;. If not contains &#39;filters&#39; then &#39;field&#39; and &#39;cond&#39; are required.
  /// </summary>
  [DataContract]
  public class FilterDef {
    /// <summary>
    /// Field json path or index name for filter
    /// </summary>
    /// <value>Field json path or index name for filter</value>
    [DataMember(Name="field", EmitDefaultValue=false)]
    [JsonPropertyName("field")]
    public string Field { get; set; }

    /// <summary>
    /// Condition operator
    /// </summary>
    /// <value>Condition operator</value>
    [DataMember(Name="cond", EmitDefaultValue=false)]
    [JsonPropertyName("cond")]
    public string Cond { get; set; }

    /// <summary>
    /// Logic operator
    /// </summary>
    /// <value>Logic operator</value>
    [DataMember(Name="op", EmitDefaultValue=false)]
    [JsonPropertyName("op")]
    public string Op { get; set; }

    /// <summary>
    /// Value of filter. Single integer or string for EQ, GT, GE, LE, LT condition, array of 2 elements for RANGE condition, or variable len array for SET condition
    /// </summary>
    /// <value>Value of filter. Single integer or string for EQ, GT, GE, LE, LT condition, array of 2 elements for RANGE condition, or variable len array for SET condition</value>
    [DataMember(Name="value", EmitDefaultValue=false)]
    [JsonPropertyName("value")]
    public Object Value { get; set; }

    /// <summary>
    /// Filter for results documents
    /// </summary>
    /// <value>Filter for results documents</value>
    [DataMember(Name="filters", EmitDefaultValue=false)]
    [JsonPropertyName("filters")]
    public List<FilterDef> Filters { get; set; }

    /// <summary>
    /// Gets or Sets JoinQuery
    /// </summary>
    [DataMember(Name="join_query", EmitDefaultValue=false)]
    [JsonPropertyName("join_query")]
    public JoinedDef JoinQuery { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class FilterDef {\n");
      sb.Append("  Field: ").Append(Field).Append("\n");
      sb.Append("  Cond: ").Append(Cond).Append("\n");
      sb.Append("  Op: ").Append(Op).Append("\n");
      sb.Append("  Value: ").Append(Value).Append("\n");
      sb.Append("  Filters: ").Append(Filters).Append("\n");
      sb.Append("  JoinQuery: ").Append(JoinQuery).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
