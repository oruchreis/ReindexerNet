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
    public class ItemsOf<T>
    {
        /// <summary>
        /// Total count of documents, matched specified filters
        /// </summary>
        /// <value>Total count of documents, matched specified filters</value>
        [DataMember(Name = "total_items", EmitDefaultValue = false)]
        [JsonPropertyName("total_items")]
        public int? TotalItems { get; set; }

        /// <summary>
        /// Documents, matched specified filters
        /// </summary>
        /// <value>Documents, matched specified filters</value>
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
            sb.Append("  TotalItems: ").Append(TotalItems).Append("\n");
            sb.Append("  Items: ").Append(Items).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

    }
}
