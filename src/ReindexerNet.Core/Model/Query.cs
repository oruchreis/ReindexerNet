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
    public class Query
    {
        /// <summary>
        /// Namespace name
        /// </summary>
        /// <value>Namespace name</value>
        [DataMember(Name = "namespace", EmitDefaultValue = false)]
        [JsonPropertyName("namespace")]
        public string Namespace { get; set; }

        /// <summary>
        /// Maximum count of returned items
        /// </summary>
        /// <value>Maximum count of returned items</value>
        [DataMember(Name = "limit", EmitDefaultValue = false)]
        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// Offset of first returned item
        /// </summary>
        /// <value>Offset of first returned item</value>
        [DataMember(Name = "offset", EmitDefaultValue = false)]
        [JsonPropertyName("offset")]
        public int? Offset { get; set; }

        /// <summary>
        /// Ask query to calculate total documents, match condition
        /// </summary>
        /// <value>Ask query to calculate total documents, match condition</value>
        [DataMember(Name = "req_total", EmitDefaultValue = false)]
        [JsonPropertyName("req_total")]
        public string ReqTotal { get; set; }

        /// <summary>
        /// Filter for results documents
        /// </summary>
        /// <value>Filter for results documents</value>
        [DataMember(Name = "filters", EmitDefaultValue = false)]
        [JsonPropertyName("filters")]
        public List<FilterDef> Filters { get; set; }

        /// <summary>
        /// Specifies results sorting order
        /// </summary>
        /// <value>Specifies results sorting order</value>
        [DataMember(Name = "sort", EmitDefaultValue = false)]
        [JsonPropertyName("sort")]
        public List<SortDef> Sort { get; set; }

        /// <summary>
        /// Merged queries to be merged with main query
        /// </summary>
        /// <value>Merged queries to be merged with main query</value>
        [DataMember(Name = "merge_queries", EmitDefaultValue = false)]
        [JsonPropertyName("merge_queries")]
        public List<Query> MergeQueries { get; set; }

        /// <summary>
        /// Filter fields of returned document. Can be dot separated, e.g 'subobject.field'
        /// </summary>
        /// <value>Filter fields of returned document. Can be dot separated, e.g 'subobject.field'</value>
        [DataMember(Name = "select_filter", EmitDefaultValue = false)]
        [JsonPropertyName("select_filter")]
        public List<string> SelectFilter { get; set; }

        /// <summary>
        /// Add extra select functions to query
        /// </summary>
        /// <value>Add extra select functions to query</value>
        [DataMember(Name = "select_functions", EmitDefaultValue = false)]
        [JsonPropertyName("select_functions")]
        public List<string> SelectFunctions { get; set; }

        /// <summary>
        /// Ask query calculate aggregation
        /// </summary>
        /// <value>Ask query calculate aggregation</value>
        [DataMember(Name = "aggregations", EmitDefaultValue = false)]
        [JsonPropertyName("aggregations")]
        public List<AggregationsDef> Aggregations { get; set; }

        /// <summary>
        /// Array fields to be searched with equal array indexes
        /// </summary>
        /// <value>Array fields to be searched with equal array indexes</value>
        [DataMember(Name = "equal_positions", EmitDefaultValue = false)]
        [JsonPropertyName("equal_positions")]
        public List<string> EqualPositions { get; set; }

        /// <summary>
        /// Add query execution explain information
        /// </summary>
        /// <value>Add query execution explain information</value>
        [DataMember(Name = "explain", EmitDefaultValue = false)]
        [JsonPropertyName("explain")]
        public bool? Explain { get; set; }


        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Query {\n");
            sb.Append("  Namespace: ").Append(Namespace).Append("\n");
            sb.Append("  Limit: ").Append(Limit).Append("\n");
            sb.Append("  Offset: ").Append(Offset).Append("\n");
            sb.Append("  ReqTotal: ").Append(ReqTotal).Append("\n");
            sb.Append("  Filters: ").Append(Filters).Append("\n");
            sb.Append("  Sort: ").Append(Sort).Append("\n");
            sb.Append("  MergeQueries: ").Append(MergeQueries).Append("\n");
            sb.Append("  SelectFilter: ").Append(SelectFilter).Append("\n");
            sb.Append("  SelectFunctions: ").Append(SelectFunctions).Append("\n");
            sb.Append("  Aggregations: ").Append(Aggregations).Append("\n");
            sb.Append("  EqualPositions: ").Append(EqualPositions).Append("\n");
            sb.Append("  Explain: ").Append(Explain).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

    }
}
