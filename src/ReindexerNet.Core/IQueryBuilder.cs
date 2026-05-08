using System;

namespace ReindexerNet;

/// <summary>
/// Builds Reindexer queries by using the fluent query DSL.
/// </summary>
public interface IQueryBuilder : IDisposable
{
    /// <summary>
    /// Gets or sets the number of items fetched by one operation.
    /// </summary>
    int FetchCount { get; set; }

    /// <summary>
    /// Gets or sets the name used for total item count calculation.
    /// </summary>
    string TotalName { get; set; }
    
    /// <summary>
    /// Adds the next condition with the AND operator.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder And();

    /// <summary>
    /// Adds an average aggregation for a field.
    /// </summary>
    /// <param name="field">Field name to aggregate.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder AggregateAvg(string field);

    /// <summary>
    /// Adds a facet aggregation for one or more fields.
    /// </summary>
    /// <param name="aggFacetQuery">Callback used to configure the facet aggregation request.</param>
    /// <param name="fields">Facet fields. At least one field is required.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder AggregateFacet(Action<IAggregateFacetRequest> aggFacetQuery, params string[] fields);

    /// <summary>
    /// Adds a maximum aggregation for a field.
    /// </summary>
    /// <param name="field">Field name to aggregate.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder AggregateMax(string field);

    /// <summary>
    /// Adds a minimum aggregation for a field.
    /// </summary>
    /// <param name="field">Field name to aggregate.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder AggregateMin(string field);

    /// <summary>
    /// Adds a sum aggregation for a field.
    /// </summary>
    /// <param name="field">Field name to aggregate.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder AggregateSum(string field);    

    /// <summary>
    /// Requests cached total item count calculation.
    /// </summary>
    /// <param name="totalNames">Names of cached total counters to request.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder CachedTotal(params string[] totalNames);

    /// <summary>
    /// Returns only items with unique values for the given field.
    /// </summary>
    /// <param name="distinctIndex">Field or index name used for distinct filtering.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Distinct(string distinctIndex);

    /// <summary>
    /// Adds a spatial distance condition to the query.
    /// </summary>
    /// <param name="index">Spatial index or field name.</param>
    /// <param name="point">Point coordinates used by the distance condition.</param>
    /// <param name="distance">Maximum allowed distance.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder DWithin(string index, (double start, double end) point, double distance);

    /// <summary>
    /// Adds equal-position constraints for array fields.
    /// </summary>
    /// <param name="fields">Array fields that must match at the same positions.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder EqualPosition(params string[] fields);

    /// <summary>
    /// Requests query execution explanation.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Explain();

    /// <summary>
    /// Adds select functions, such as highlight or snippet, to result fields.
    /// </summary>
    /// <param name="fields">Field function expressions to apply.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Functions(params string[] fields);

    /// <summary>
    /// Adds an inner join query.
    /// </summary>
    /// <param name="otherNamespace">Joined namespace name.</param>
    /// <param name="otherQuery">Callback used to build the joined query.</param>
    /// <param name="field">Alias used to identify the joined result.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder InnerJoin(string otherNamespace, Action<IQueryBuilder> otherQuery, string field);

    /// <summary>
    /// Adds a left join query. This method is an alias for <see cref="LeftJoin(string, Action{IQueryBuilder}, string)"/>.
    /// </summary>
    /// <param name="otherNamespace">Joined namespace name.</param>
    /// <param name="otherQuery">Callback used to build the joined query.</param>
    /// <param name="field">Alias used to identify the joined result.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Join(string otherNamespace, Action<IQueryBuilder> otherQuery, string field);

    /// <summary>
    /// Adds a left join query.
    /// </summary>
    /// <param name="otherNamespace">Joined namespace name.</param>
    /// <param name="otherQuery">Callback used to build the joined query.</param>
    /// <param name="field">Alias used to identify the joined result.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder LeftJoin(string otherNamespace, Action<IQueryBuilder> otherQuery, string field);

    /// <summary>
    /// Sets the maximum number of returned items.
    /// </summary>
    /// <param name="limitItems">Maximum number of items to return.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Limit(int limitItems);

    /// <summary>
    /// Adds a full-text match condition.
    /// </summary>
    /// <param name="index">Full-text index or field name.</param>
    /// <param name="keys">Search terms to match.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Match(string index, params string[] keys);

    /// <summary>
    /// Merges another query into this query.
    /// </summary>
    /// <param name="otherNamespace">Merged namespace name.</param>
    /// <param name="otherQuery">Callback used to build the merged query.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Merge(string otherNamespace, Action<IQueryBuilder> otherQuery);

    /// <summary>
    /// Adds the next condition with the NOT AND operator.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Not();

    /// <summary>
    /// Sets the start offset of returned items.
    /// </summary>
    /// <param name="startOffset">Number of items to skip before returning results.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Offset(int startOffset);

    /// <summary>
    /// Adds a join condition for the latest join query.
    /// </summary>
    /// <param name="index">Field from the main namespace used for the join.</param>
    /// <param name="condition">Join comparison condition.</param>
    /// <param name="joinIndex">Field from the joined namespace used for the join.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder On(string index, Condition condition, string joinIndex);

    /// <summary>
    /// Adds the next condition with the OR operator.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Or();

    /// <summary>
    /// Requests total item count calculation.
    /// </summary>
    /// <param name="totalNames">Names of total counters to request.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder ReqTotal(params string[] totalNames);

    /// <summary>
    /// Selects fields to include in result objects.
    /// </summary>
    /// <param name="fields">Fields to include.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Select(params string[] fields);

    /// <summary>
    /// Applies sort order to returned items.
    /// </summary>
    /// <param name="sortIndex">Index or field name to sort by.</param>
    /// <param name="desc">Whether to sort descending.</param>
    /// <param name="values">Optional values that should be forced to the top positions.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Sort(string sortIndex, bool desc, params object[] values);

    /// <summary>
    /// Sorts by the shortest distance between two geometry fields.
    /// </summary>
    /// <param name="field1">First geometry field name.</param>
    /// <param name="field2">Second geometry field name.</param>
    /// <param name="desc">Whether to sort descending.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder SortStFieldDistance(string field1, string field2, bool desc);

    /// <summary>
    /// Sorts by the shortest distance between a geometry field and a point.
    /// </summary>
    /// <param name="field">Geometry field name.</param>
    /// <param name="p">Point coordinates.</param>
    /// <param name="desc">Whether to sort descending.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder SortStPointDistance(string field, (double X, double Y) p, bool desc);

    /// <summary>
    /// Sets query strict mode.
    /// </summary>
    /// <param name="mode">Strict validation mode to apply.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Strict(QueryStrictMode mode);

    /// <summary>
    /// Requests full-text rank output. This is allowed only with full-text queries.
    /// </summary>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder WithRank();

    /// <summary>
    /// Adds a nested filter query, equivalent to parentheses in a SQL WHERE condition.
    /// </summary>
    /// <param name="filterQuery">Callback used to build the nested filter.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Where(Action<IQueryBuilder> filterQuery);

    /// <summary>
    /// Adds a where condition to the query.
    /// </summary>
    /// <param name="index">Index or field name.</param>
    /// <param name="condition">Comparison condition.</param>
    /// <param name="keys">Value or values to compare.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder Where(string index, Condition condition, object keys);

    /// <summary>
    /// Adds a condition that compares two fields.
    /// </summary>
    /// <param name="firstField">Left-side field name.</param>
    /// <param name="condition">Comparison condition.</param>
    /// <param name="secondField">Right-side field name.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder WhereBetweenFields(string firstField, Condition condition, string secondField);

    /// <summary>
    /// Adds a bool where condition to the query.
    /// </summary>
    /// <param name="index">Index or field name.</param>
    /// <param name="condition">Comparison condition.</param>
    /// <param name="keys">Bool values to compare.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder WhereBool(string index, Condition condition, params bool[] keys);

    /// <summary>
    /// Adds a composite-index where condition to the query.
    /// </summary>
    /// <param name="index">Composite index name.</param>
    /// <param name="condition">Comparison condition.</param>
    /// <param name="keys">Composite key values to compare.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder WhereComposite(string index, Condition condition, params object[] keys);

    /// <summary>
    /// Adds a double where condition to the query.
    /// </summary>
    /// <param name="index">Index or field name.</param>
    /// <param name="condition">Comparison condition.</param>
    /// <param name="keys">Double values to compare.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder WhereDouble(string index, Condition condition, params double[] keys);

    /// <summary>
    /// Adds an int where condition to the query.
    /// </summary>
    /// <param name="index">Index or field name.</param>
    /// <param name="condition">Comparison condition.</param>
    /// <param name="keys">Int values to compare.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder WhereInt(string index, Condition condition, params int[] keys);

    /// <summary>
    /// Adds an int where condition to the query.
    /// </summary>
    /// <remarks>Same as <see cref="WhereInt(string, Condition, int[])"/>.</remarks>
    /// <param name="index">Index or field name.</param>
    /// <param name="condition">Comparison condition.</param>
    /// <param name="keys">Int values to compare.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder WhereInt32(string index, Condition condition, params int[] keys);

    /// <summary>
    /// Adds a long where condition to the query.
    /// </summary>
    /// <param name="index">Index or field name.</param>
    /// <param name="condition">Comparison condition.</param>
    /// <param name="keys">Long values to compare.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder WhereInt64(string index, Condition condition, params long[] keys);

    /// <summary>
    /// Adds a string where condition to the query.
    /// </summary>
    /// <param name="index">Index or field name.</param>
    /// <param name="condition">Comparison condition.</param>
    /// <param name="keys">String values to compare.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder WhereString(string index, Condition condition, params string[] keys);

    /// <summary>
    /// Adds a UUID string where condition to the query.
    /// </summary>
    /// <param name="index">Index or field name.</param>
    /// <param name="condition">Comparison condition.</param>
    /// <param name="keys">UUID string values to compare.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder WhereUuid(string index, Condition condition, params string[] keys);

    /// <summary>
    /// Adds a GUID where condition to the query.
    /// </summary>
    /// <param name="index">Index or field name.</param>
    /// <param name="condition">Comparison condition.</param>
    /// <param name="keys">GUID values to compare.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    IQueryBuilder WhereGuid(string index, Condition condition, params Guid[] keys);
}

/// <summary>
/// Configures facet aggregation paging and sorting.
/// </summary>
public interface IAggregateFacetRequest
{
    /// <summary>
    /// Sets the maximum number of facet rows.
    /// </summary>
    /// <param name="limit">Maximum number of facet rows to return.</param>
    /// <returns>The current facet request for fluent chaining.</returns>
    IAggregateFacetRequest Limit(int limit);

    /// <summary>
    /// Sets the number of facet rows to skip.
    /// </summary>
    /// <param name="offset">Number of facet rows to skip.</param>
    /// <returns>The current facet request for fluent chaining.</returns>
    IAggregateFacetRequest Offset(int offset);

    /// <summary>
    /// Sorts facet rows by a field. Use <c>count</c> to sort by facet count.
    /// </summary>
    /// <param name="field">Facet field to sort by.</param>
    /// <param name="desc">Whether to sort descending.</param>
    /// <returns>The current facet request for fluent chaining.</returns>
    IAggregateFacetRequest Sort(string field, bool desc);
}

/// <summary>
/// Represents serialization methods of a query builder.
/// </summary>
public interface ISerializableQueryBuilder
{
    /// <summary>
    /// Closes the query and returns the serialized query bytes.
    /// </summary>
    /// <returns>The serialized query payload.</returns>
    ReadOnlySpan<byte> CloseQuery();
}
