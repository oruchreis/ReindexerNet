using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Join cache stats. Stores results of selects to right table by ON condition
  /// </summary>
  [DataContract]
  public class JoinCacheMemStats : CacheMemStats {

    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class JoinCacheMemStats {\n");
      sb.Append("}\n");
      return sb.ToString();
    }


}
}
