using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Idset cache stats. Stores merged reverse index results of SELECT field IN(...) by IN(...) keys
  /// </summary>
  [DataContract]
  public class IndexCacheMemStats : CacheMemStats {

    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class IndexCacheMemStats {\n");
      sb.Append("}\n");
      return sb.ToString();
    }


}
}
