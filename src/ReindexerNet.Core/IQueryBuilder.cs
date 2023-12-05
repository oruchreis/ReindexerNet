using System;

namespace ReindexerNet;

/// <summary>
/// Represents generic query builder.
/// </summary>
public interface IQueryBuilder: IFilterQueryBuilder, IDisposable
{
    /// <summary>
    /// sets the number of items that will be fetched by one operation
    /// </summary>
    int FetchCount { get; set; }
    /// <summary>
    /// 
    /// </summary>
    string TotalName { get; set; }    
    /// <summary>
    /// Adds Avarage Aggregate
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    IQueryBuilder AggregateAvg(string field);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="aggFacetQuery"></param>
    /// <param name="fields">fields should not be empty.</param>
    /// <returns></returns>
    IQueryBuilder AggregateFacet(Action<IAggregateFacetRequest> aggFacetQuery, params string[] fields);
    /// <summary>
    /// Adds Max Aggregate
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    IQueryBuilder AggregateMax(string field);
    /// <summary>
    /// Adds Min Aggregate
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    IQueryBuilder AggregateMin(string field);
    /// <summary>
    /// Adds Sum Aggregate
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    IQueryBuilder AggregateSum(string field);
    /// <summary>
    /// next condition will added with AND.
    /// This is the default operation for WHERE statement. Do not have to be called explicitly in user's code. Used in DSL convertion
    /// </summary>
    /// <returns></returns>
    IQueryBuilder And();
    /// <summary>
    /// Request cached total items calculation
    /// </summary>
    /// <param name="totalNames"></param>
    /// <returns></returns>
    IQueryBuilder CachedTotal(params string[] totalNames);
    
    /// <summary>
    /// Return only items with uniq value of field
    /// </summary>
    /// <param name="distinctIndex"></param>
    /// <returns></returns>
    IQueryBuilder Distinct(string distinctIndex);    
    /// <summary>
    /// Add DWithin condition to DB query
    /// </summary>
    /// <param name="index"></param>
    /// <param name="point"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    IQueryBuilder DWithin(string index, (double start, double end) point, double distance);
    /// <summary>
    /// Adds equal position fields to arrays
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    IQueryBuilder EqualPosition(params string[] fields);
    /// <summary>
    /// Request explain for query
    /// </summary>
    /// <returns></returns>
    IQueryBuilder Explain();
    /// <summary>
    /// add optional select functions (e.g highlight or snippet ) to fields of result's objects
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    IQueryBuilder Functions(params string[] fields);
    /// <summary>
    /// joins 2 queries
    /// Items from the 1-st query are filtered by and expanded with the data from the 2-nd query
    /// </summary>
    /// <param name="otherNamespace"></param>
    /// <param name="otherQuery"></param>
    /// <param name="field"> parameter serves as unique identifier for the join between `q` and `q2`</param>
    /// <returns></returns>
    IQueryBuilder InnerJoin(string otherNamespace, Action<IQueryBuilder> otherQuery, string field);
    /// <summary>
    /// This method is an alias for <see cref="LeftJoin(string, Action{IQueryBuilder}, string)"/>
    /// </summary>
    /// <param name="otherNamespace"></param>
    /// <param name="otherQuery"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    IQueryBuilder Join(string otherNamespace, Action<IQueryBuilder> otherQuery, string field);
    /// <summary>
    /// joins 2 queries
    /// Items from the 1-st query are expanded with the data from the 2-nd query
    /// </summary>
    /// <param name="otherNamespace"></param>
    /// <param name="otherQuery"></param>
    /// <param name="field">parameter serves as unique identifier for the join between `q` and `q2`</param>
    /// <returns></returns>
    IQueryBuilder LeftJoin(string otherNamespace, Action<IQueryBuilder> otherQuery, string field);
    /// <summary>
    /// Set limit (count) of returned items
    /// </summary>
    /// <param name="limitItems"></param>
    /// <returns></returns>
    IQueryBuilder Limit(int limitItems);
    /// <summary>
    /// Add where condition to DB query with string args
    /// </summary>
    /// <param name="index"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    IQueryBuilder Match(string index, params string[] keys);
    /// <summary>
    ///  Merge 2 queries
    /// </summary>
    /// <param name="otherNamespace"></param>
    /// <param name="otherQuery"></param>
    /// <returns></returns>
    IQueryBuilder Merge(string otherNamespace, Action<IQueryBuilder> otherQuery);
    /// <summary>
    /// next condition will added with NOT AND.
    /// Implements short-circuiting:
    /// if the previous condition is failed the next will not be evaluated
    /// </summary>
    /// <returns></returns>
    IQueryBuilder Not();
    /// <summary>
    /// Set start offset of returned items
    /// </summary>
    /// <param name="startOffset"></param>
    /// <returns></returns>
    IQueryBuilder Offset(int startOffset);
    /// <summary>
    /// Specifies join condition
    /// </summary>
    /// <param name="index">specifies which field from `q` namespace should be used during join</param>
    /// <param name="condition">specifies how `q` will be joined with the latest join query issued on `q` (e.g. `EQ`/`GT`/`SET`/...)</param>
    /// <param name="joinIndex">specifies which field from namespace for the latest join query issued on `q` should be used during join</param>
    /// <returns></returns>
    IQueryBuilder On(string index, Condition condition, string joinIndex);
    /// <summary>
    /// next condition will added with OR.
    /// Implements short-circuiting:
    /// if the previous condition is successful the next will not be evaluated, but except Join conditions
    /// </summary>
    /// <returns></returns>
    IQueryBuilder Or();
    /// <summary>
    /// Request total items calculation
    /// </summary>
    /// <param name="totalNames"></param>
    /// <returns></returns>
    IQueryBuilder ReqTotal(params string[] totalNames);
    /// <summary>
    /// add filter to  fields of result's objects
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    IQueryBuilder Select(params string[] fields);
    /// <summary>
    /// Apply sort order to returned from query items.
    /// If values argument specified, then items equal to values, if found will be placed in the top positions.
    /// For composite indexes values must be []interface{}, with value of each subindex.
    /// Forced sort is support for the first sorting field only
    /// </summary>
    /// <param name="sortIndex"></param>
    /// <param name="desc"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    IQueryBuilder Sort(string sortIndex, bool desc, params object[] values);
    /// <summary>
    /// wrapper for geometry sorting by shortes distance between 2 geometry fields (ST_Distance)
    /// </summary>
    /// <param name="field1"></param>
    /// <param name="field2"></param>
    /// <param name="desc"></param>
    /// <returns></returns>
    IQueryBuilder SortStFieldDistance(string field1, string field2, bool desc);
    /// <summary>
    ///  wrapper for geometry sorting by shortes distance between geometry field and point (ST_Distance)
    /// </summary>
    /// <param name="field"></param>
    /// <param name="p"></param>
    /// <param name="desc"></param>
    /// <returns></returns>
    IQueryBuilder SortStPointDistance(string field, (double X, double Y) p, bool desc);
    /// <summary>
    /// Set query strict mode
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    IQueryBuilder Strict(QueryStrictMode mode);
    /// <summary>
    /// Creates sub filter query. Equals to Brackets () in an SQL query in the whre condition.
    /// </summary>
    /// <param name="filterQuery"></param>
    /// <returns></returns>
    IQueryBuilder Where(Action<IFilterQueryBuilder> filterQuery);
    /// <summary>
    /// Output fulltext rank.
    /// Allowed only with fulltext query
    /// </summary>
    /// <returns></returns>
    IQueryBuilder WithRank();
}

/// <summary>
/// Aggregate Facet Request
/// </summary>
public interface IAggregateFacetRequest
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="limit"></param>
    /// <returns></returns>
    IAggregateFacetRequest Limit(int limit);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    IAggregateFacetRequest Offset(int offset);
    /// <summary>
    /// Use field 'count' to sort by facet's count value.
    /// </summary>
    /// <param name="field"></param>
    /// <param name="desc"></param>
    /// <returns></returns>
    IAggregateFacetRequest Sort(string field, bool desc);
}

/// <summary>
/// Represents serialization methods of a query builder.
/// </summary>
public interface ISerializableQueryBuilder
{
    /// <summary>
    /// Closes query and returns query as a byte array of the serialized query. Should not call explictly inside a query call like Execute method of the client. 
    /// And Should not do any query operations after this call. Only proper call is Dispose method of the query after this call.
    /// </summary>
    /// <returns></returns>
    ReadOnlySpan<byte> CloseQuery();
}