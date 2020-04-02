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
  public class SuggestItems {
    /// <summary>
    /// Suggested query autocompletion variants
    /// </summary>
    /// <value>Suggested query autocompletion variants</value>
    [DataMember(Name="suggests", EmitDefaultValue=false)]
    [JsonPropertyName("suggests")]
    public List<string> Suggests { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class SuggestItems {\n");
      sb.Append("  Suggests: ").Append(Suggests).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
