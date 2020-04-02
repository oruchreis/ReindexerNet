using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Fulltext synonym definition
  /// </summary>
  [DataContract]
  public class FulltextSynonym {
    /// <summary>
    /// List source tokens in query, which will be replaced with alternatives
    /// </summary>
    /// <value>List source tokens in query, which will be replaced with alternatives</value>
    [DataMember(Name="tokens", EmitDefaultValue=false)]
    [JsonPropertyName("tokens")]
    public List<string> Tokens { get; set; }

    /// <summary>
    /// List of alternatives, which will be used for search documents
    /// </summary>
    /// <value>List of alternatives, which will be used for search documents</value>
    [DataMember(Name="alternatives", EmitDefaultValue=false)]
    [JsonPropertyName("alternatives")]
    public List<string> Alternatives { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class FulltextSynonym {\n");
      sb.Append("  Tokens: ").Append(Tokens).Append("\n");
      sb.Append("  Alternatives: ").Append(Alternatives).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
