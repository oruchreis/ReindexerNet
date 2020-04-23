using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNet {

  /// <summary>
  /// Fulltext Index configuration
  /// </summary>
  [DataContract]
  public class FulltextConfig {
    /// <summary>
    /// Enable russian translit variants processing. e.g. term 'luntik' will match word 'лунтик'
    /// </summary>
    /// <value>Enable russian translit variants processing. e.g. term 'luntik' will match word 'лунтик'</value>
    [DataMember(Name="enable_translit", EmitDefaultValue=false)]
    [JsonPropertyName("enable_translit")]
    public bool? EnableTranslit { get; set; }

    /// <summary>
    /// Enable number variants processing. e.g. term '100' will match words one hundred
    /// </summary>
    /// <value>Enable number variants processing. e.g. term '100' will match words one hundred</value>
    [DataMember(Name="enable_numbers_search", EmitDefaultValue=false)]
    [JsonPropertyName("enable_numbers_search")]
    public bool? EnableNumbersSearch { get; set; }

    /// <summary>
    /// Enable wrong keyboard layout variants processing. e.g. term 'keynbr' will match word 'лунтик'
    /// </summary>
    /// <value>Enable wrong keyboard layout variants processing. e.g. term 'keynbr' will match word 'лунтик'</value>
    [DataMember(Name="enable_kb_layout", EmitDefaultValue=false)]
    [JsonPropertyName("enable_kb_layout")]
    public bool? EnableKbLayout { get; set; }

    /// <summary>
    /// Log level of full text search engine
    /// </summary>
    /// <value>Log level of full text search engine</value>
    [DataMember(Name="log_level", EmitDefaultValue=false)]
    [JsonPropertyName("log_level")]
    public long? LogLevel { get; set; }

    /// <summary>
    /// Maximum documents count which will be processed in merge query results.  Increasing this value may refine ranking of queries with high frequency words, but will decrease search speed
    /// </summary>
    /// <value>Maximum documents count which will be processed in merge query results.  Increasing this value may refine ranking of queries with high frequency words, but will decrease search speed</value>
    [DataMember(Name="merge_limit", EmitDefaultValue=false)]
    [JsonPropertyName("merge_limit")]
    public long? MergeLimit { get; set; }

    /// <summary>
    /// List of symbols, which will be threated as word part, all other symbols will be thrated as wors separators
    /// </summary>
    /// <value>List of symbols, which will be threated as word part, all other symbols will be thrated as wors separators</value>
    [DataMember(Name="extra_word_symbols", EmitDefaultValue=false)]
    [JsonPropertyName("extra_word_symbols")]
    public string ExtraWordSymbols { get; set; }

    /// <summary>
    /// List of stop words. Words from this list will be ignored in documents and queries
    /// </summary>
    /// <value>List of stop words. Words from this list will be ignored in documents and queries</value>
    [DataMember(Name="stop_words", EmitDefaultValue=false)]
    [JsonPropertyName("stop_words")]
    public List<string> StopWords { get; set; }

    /// <summary>
    /// List of stemmers to use
    /// </summary>
    /// <value>List of stemmers to use</value>
    [DataMember(Name="stemmers", EmitDefaultValue=false)]
    [JsonPropertyName("stemmers")]
    public List<string> Stemmers { get; set; }

    /// <summary>
    /// List of synonyms for replacement
    /// </summary>
    /// <value>List of synonyms for replacement</value>
    [DataMember(Name="synonyms", EmitDefaultValue=false)]
    [JsonPropertyName("synonyms")]
    public List<FulltextSynonym> Synonyms { get; set; }

    /// <summary>
    /// Boost of bm25 ranking
    /// </summary>
    /// <value>Boost of bm25 ranking</value>
    [DataMember(Name="bm25_boost", EmitDefaultValue=false)]
    [JsonPropertyName("bm25_boost")]
    public float? Bm25Boost { get; set; }

    /// <summary>
    /// Weight of bm25 rank in final rank 0: bm25 will not change final rank. 1: bm25 will affect to finl rank in 0 - 100% range
    /// </summary>
    /// <value>Weight of bm25 rank in final rank 0: bm25 will not change final rank. 1: bm25 will affect to finl rank in 0 - 100% range</value>
    [DataMember(Name="bm25_weight", EmitDefaultValue=false)]
    [JsonPropertyName("bm25_weight")]
    public float? Bm25Weight { get; set; }

    /// <summary>
    /// Boost of search query term distance in found document
    /// </summary>
    /// <value>Boost of search query term distance in found document</value>
    [DataMember(Name="distance_boost", EmitDefaultValue=false)]
    [JsonPropertyName("distance_boost")]
    public float? DistanceBoost { get; set; }

    /// <summary>
    /// Weight of search query terms distance in found document in final rank 0: distance will not change final rank. 1: distance will affect to final rank in 0 - 100% range
    /// </summary>
    /// <value>Weight of search query terms distance in found document in final rank 0: distance will not change final rank. 1: distance will affect to final rank in 0 - 100% range</value>
    [DataMember(Name="distance_weght", EmitDefaultValue=false)]
    [JsonPropertyName("distance_weght")]
    public float? DistanceWeght { get; set; }

    /// <summary>
    /// Boost of search query term length
    /// </summary>
    /// <value>Boost of search query term length</value>
    [DataMember(Name="term_len_boost", EmitDefaultValue=false)]
    [JsonPropertyName("term_len_boost")]
    public float? TermLenBoost { get; set; }

    /// <summary>
    /// Weight of search query term length in final rank. 0: term length will not change final rank. 1: term length will affect to final rank in 0 - 100% range
    /// </summary>
    /// <value>Weight of search query term length in final rank. 0: term length will not change final rank. 1: term length will affect to final rank in 0 - 100% range</value>
    [DataMember(Name="term_len_weght", EmitDefaultValue=false)]
    [JsonPropertyName("term_len_weght")]
    public float? TermLenWeght { get; set; }

    /// <summary>
    /// Boost of full match of search phrase with doc
    /// </summary>
    /// <value>Boost of full match of search phrase with doc</value>
    [DataMember(Name="full_match_boost", EmitDefaultValue=false)]
    [JsonPropertyName("full_match_boost")]
    public float? FullMatchBoost { get; set; }

    /// <summary>
    /// Minimum rank of found documents. 0: all found documents will be returned 1: only documents with relevancy >= 100% will be returned 
    /// </summary>
    /// <value>Minimum rank of found documents. 0: all found documents will be returned 1: only documents with relevancy >= 100% will be returned </value>
    [DataMember(Name="min_relevancy", EmitDefaultValue=false)]
    [JsonPropertyName("min_relevancy")]
    public float? MinRelevancy { get; set; }

    /// <summary>
    /// Maximum possible typos in word. 0: typos is disabled, words with typos will not match. N: words with N possible typos will match. It is not recommended to set more than 1 possible typo -It will seriously increase RAM usage, and decrease search speed
    /// </summary>
    /// <value>Maximum possible typos in word. 0: typos is disabled, words with typos will not match. N: words with N possible typos will match. It is not recommended to set more than 1 possible typo -It will seriously increase RAM usage, and decrease search speed</value>
    [DataMember(Name="max_typos_in_word", EmitDefaultValue=false)]
    [JsonPropertyName("max_typos_in_word")]
    public long? MaxTyposInWord { get; set; }

    /// <summary>
    /// Maximum word length for building and matching variants with typos.
    /// </summary>
    /// <value>Maximum word length for building and matching variants with typos.</value>
    [DataMember(Name="max_typo_len", EmitDefaultValue=false)]
    [JsonPropertyName("max_typo_len")]
    public long? MaxTypoLen { get; set; }

    /// <summary>
    /// Maximum steps without full rebuild of ft - more steps faster commit slower select - optimal about 15.
    /// </summary>
    /// <value>Maximum steps without full rebuild of ft - more steps faster commit slower select - optimal about 15.</value>
    [DataMember(Name="max_rebuild_steps", EmitDefaultValue=false)]
    [JsonPropertyName("max_rebuild_steps")]
    public long? MaxRebuildSteps { get; set; }

    /// <summary>
    /// Maximum unique words to step
    /// </summary>
    /// <value>Maximum unique words to step</value>
    [DataMember(Name="max_step_size", EmitDefaultValue=false)]
    [JsonPropertyName("max_step_size")]
    public long? MaxStepSize { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class FulltextConfig {\n");
      sb.Append("  EnableTranslit: ").Append(EnableTranslit).Append("\n");
      sb.Append("  EnableNumbersSearch: ").Append(EnableNumbersSearch).Append("\n");
      sb.Append("  EnableKbLayout: ").Append(EnableKbLayout).Append("\n");
      sb.Append("  LogLevel: ").Append(LogLevel).Append("\n");
      sb.Append("  MergeLimit: ").Append(MergeLimit).Append("\n");
      sb.Append("  ExtraWordSymbols: ").Append(ExtraWordSymbols).Append("\n");
      sb.Append("  StopWords: ").Append(StopWords).Append("\n");
      sb.Append("  Stemmers: ").Append(Stemmers).Append("\n");
      sb.Append("  Synonyms: ").Append(Synonyms).Append("\n");
      sb.Append("  Bm25Boost: ").Append(Bm25Boost).Append("\n");
      sb.Append("  Bm25Weight: ").Append(Bm25Weight).Append("\n");
      sb.Append("  DistanceBoost: ").Append(DistanceBoost).Append("\n");
      sb.Append("  DistanceWeght: ").Append(DistanceWeght).Append("\n");
      sb.Append("  TermLenBoost: ").Append(TermLenBoost).Append("\n");
      sb.Append("  TermLenWeght: ").Append(TermLenWeght).Append("\n");
      sb.Append("  FullMatchBoost: ").Append(FullMatchBoost).Append("\n");
      sb.Append("  MinRelevancy: ").Append(MinRelevancy).Append("\n");
      sb.Append("  MaxTyposInWord: ").Append(MaxTyposInWord).Append("\n");
      sb.Append("  MaxTypoLen: ").Append(MaxTypoLen).Append("\n");
      sb.Append("  MaxRebuildSteps: ").Append(MaxRebuildSteps).Append("\n");
      sb.Append("  MaxStepSize: ").Append(MaxStepSize).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

}
}
