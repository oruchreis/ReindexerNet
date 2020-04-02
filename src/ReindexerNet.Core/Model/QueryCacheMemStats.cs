using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Query cache stats. Stores results of SELECT COUNT(*) by Where conditions
  /// </summary>
  [DataContract]
  public class QueryCacheMemStats : CacheMemStats {

    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class QueryCacheMemStats {\n");
      sb.Append("}\n");
      return sb.ToString();
    }


}
}
