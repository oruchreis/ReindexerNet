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
  public class ActionCommand {
    /// <summary>
    /// Command to execute
    /// </summary>
    /// <value>Command to execute</value>
    [DataMember(Name="command", EmitDefaultValue=false)]
    [JsonPropertyName("command")]
    public string Command { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ActionCommand {\n");
      sb.Append("  Command: ").Append(Command).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
