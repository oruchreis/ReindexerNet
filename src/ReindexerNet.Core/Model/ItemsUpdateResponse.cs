using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet
{

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class ItemsUpdateResponseOf<T>
    {
        /// <summary>
        /// Count of updated items
        /// </summary>
        /// <value>Count of updated items</value>
        [DataMember(Name = "updated", EmitDefaultValue = false)]
        [JsonPropertyName("updated")]
        public long? Updated { get; set; }

        /// <summary>
        /// Updated documents. Contains only if precepts were provided
        /// </summary>
        /// <value>Updated documents. Contains only if precepts were provided</value>
        [DataMember(Name = "items", EmitDefaultValue = false)]
        [JsonPropertyName("items")]
        public List<T> Items { get; set; }


        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("class {0} {{\n", GetType().Name);
            sb.Append("  Updated: ").Append(Updated).Append("\n");
            sb.Append("  Items: ").Append(Items).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

    }
}
