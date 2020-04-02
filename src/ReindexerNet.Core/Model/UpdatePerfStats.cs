using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Performance statistics for update operations
  /// </summary>
  [DataContract]
  public class UpdatePerfStats : CommonPerfStats {

    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class UpdatePerfStats {\n");
      sb.Append("}\n");
      return sb.ToString();
    }


}
}
