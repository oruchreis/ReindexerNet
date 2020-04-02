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
  public class StatusResponse {
    /// <summary>
    /// Status of operation
    /// </summary>
    /// <value>Status of operation</value>
    [DataMember(Name="success", EmitDefaultValue=false)]
    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    /// <summary>
    /// Error code:  * 0 - errOK  * 1 - errParseSQL  * 2 - errQueryExec  * 3 - errParams  * 4 - errLogic  * 5 - errParseJson  * 6 - errParseDSL  * 7 - errConflict  * 8 - errParseBin  * 9 - errForbidden  * 10 - errWasRelock  * 11 - errNotValid  * 12 - errNetwork  * 13 - errNotFound  * 14 - errStateInvalidated  * 15 - errBadTransaction  * 16 - errOutdatedWAL  * 17 - errNoWAL  * 18 - errDataHashMismatch 
    /// </summary>
    /// <value>Error code:  * 0 - errOK  * 1 - errParseSQL  * 2 - errQueryExec  * 3 - errParams  * 4 - errLogic  * 5 - errParseJson  * 6 - errParseDSL  * 7 - errConflict  * 8 - errParseBin  * 9 - errForbidden  * 10 - errWasRelock  * 11 - errNotValid  * 12 - errNetwork  * 13 - errNotFound  * 14 - errStateInvalidated  * 15 - errBadTransaction  * 16 - errOutdatedWAL  * 17 - errNoWAL  * 18 - errDataHashMismatch </value>
    [DataMember(Name="response_code", EmitDefaultValue=false)]
    [JsonPropertyName("response_code")]
    public int? ResponseCode { get; set; }

    /// <summary>
    /// Text description of error details
    /// </summary>
    /// <value>Text description of error details</value>
    [DataMember(Name="description", EmitDefaultValue=false)]
    [JsonPropertyName("description")]
    public string Description { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class StatusResponse {\n");
      sb.Append("  Success: ").Append(Success).Append("\n");
      sb.Append("  ResponseCode: ").Append(ResponseCode).Append("\n");
      sb.Append("  Description: ").Append(Description).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
