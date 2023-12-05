using ReindexerNet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReindexerNet;

/// <summary>
/// Builds <see cref="Query"/> object from given query, and serialize with target serializer.
/// </summary>
public sealed class SerializableQueryBuilder : IQueryBuilder, ISerializableQueryBuilder
{
    private readonly IReindexerSerializer _serializer;
    private readonly string _namespace;
    private readonly Query _query = new();
    private Bindings.Op? _nextOp = null;

    /// <summary>
    /// Builds <see cref="Query"/> object from given query, and serialize with target serializer.
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="namespace"></param>
    public SerializableQueryBuilder(IReindexerSerializer serializer, string @namespace)
    {
        _serializer = serializer;
        _namespace = @namespace;
        _query.Namespace = @namespace;
    }

    /// <inheritdoc />
    public int FetchCount { get; set; }
    /// <inheritdoc />
    public string TotalName { get; set; }

    private static string GetAggregateType(Bindings.Agg agg) => agg switch
    {
        Bindings.Agg.Sum => "SUM",
        Bindings.Agg.Avg => "AVG",
        Bindings.Agg.Facet => "FACET",
        Bindings.Agg.Min => "MIN",
        Bindings.Agg.Max => "MAX",
        Bindings.Agg.Distinct => "DISTINCT",
        _ => throw new NotImplementedException(),
    };

    /// <inheritdoc />
    public IQueryBuilder AggregateAvg(string field)
    {
        _query.Aggregations.Add(new AggregationsDef
        {
            Fields = [field],
            Type = GetAggregateType(Bindings.Agg.Avg),
        });
        return this;
    }

    private sealed class AggregateFacetRequest : IAggregateFacetRequest
    {
        private readonly AggregationsDef _agg;

        public AggregateFacetRequest(AggregationsDef agg)
        {
            _agg = agg;
        }

        public IAggregateFacetRequest Limit(int limit)
        {
            _agg.Limit = limit;
            return this;
        }

        public IAggregateFacetRequest Offset(int offset)
        {
            _agg.Offset = offset;
            return this;
        }

        public IAggregateFacetRequest Sort(string field, bool desc)
        {
            _agg.Sort ??= [];
            _agg.Sort.Add(new AggregationsSortDef { Field = field, Desc = desc });
            return this;
        }
    }

    /// <inheritdoc/>
    public IQueryBuilder AggregateFacet(Action<IAggregateFacetRequest> aggFacetQuery, params string[] fields)
    {
        var agg = new AggregationsDef
        {
            Fields = new(fields),
            Type = GetAggregateType(Bindings.Agg.Facet)
        };
        _query.Aggregations.Add(agg);
        aggFacetQuery(new AggregateFacetRequest(agg));
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder AggregateMax(string field)
    {
        _query.Aggregations.Add(new AggregationsDef
        {
            Fields = [field],
            Type = GetAggregateType(Bindings.Agg.Max),
        });
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder AggregateMin(string field)
    {
        _query.Aggregations.Add(new AggregationsDef
        {
            Fields = [field],
            Type = GetAggregateType(Bindings.Agg.Min),
        });
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder AggregateSum(string field)
    {
        _query.Aggregations.Add(new AggregationsDef
        {
            Fields = [field],
            Type = GetAggregateType(Bindings.Agg.Sum),
        });
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder And()
    {
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder CachedTotal(params string[] totalNames)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>

    public ReadOnlySpan<byte> CloseQuery()
    {
        return _serializer.Serialize(_query);
    }
    /// <inheritdoc/>

    public void Dispose()
    {
    }
    /// <inheritdoc/>

    public IQueryBuilder Distinct(string distinctIndex)
    {
        _query.Aggregations.Add(new AggregationsDef
        {
            Fields = [distinctIndex],
            Type = GetAggregateType(Bindings.Agg.Distinct),
        });
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder DWithin(string index, (double start, double end) point, double distance)
    {
        _query.Filters ??= new();
        _query.Filters.Add(new FilterDef { Field = index, Cond = Condition.DWITHIN.ToString("g"), Value = distance, Op = _nextOp?.ToString("g") });
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder EqualPosition(params string[] fields)
    {
        _query.EqualPositions = new(fields);
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder Explain()
    {
        _query.Explain = true;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder Functions(params string[] fields)
    {
        _query.SelectFunctions = new(fields);
        return this;
    }

    private string GetJsonJoinType(Bindings.JoinType joinType) => joinType switch
    {
        Bindings.JoinType.LeftJoin => "LEFT",
        Bindings.JoinType.InnerJoin => "INNER",
        Bindings.JoinType.OrInnerJoin => "ORINNER",
        Bindings.JoinType.Merge => throw new NotImplementedException(),
        _ => throw new NotImplementedException()
    };
    /// <inheritdoc/>

    public IQueryBuilder InnerJoin(string otherNamespace, Action<IQueryBuilder> otherQuery, string field)
    {
        _query.Filters ??= [];
        var otherQueryBuilder = new SerializableQueryBuilder(_serializer, otherNamespace);
        otherQuery(otherQueryBuilder);

        var joinType = Bindings.JoinType.InnerJoin;
        if (_nextOp == Bindings.Op.Or)
        {
            _nextOp = Bindings.Op.And;
            joinType = Bindings.JoinType.OrInnerJoin;
        }

        _query.Filters.Add(new FilterDef
        {
            Field = field,
            JoinQuery = new JoinedDef
            {
                Namespace = otherQueryBuilder._namespace,
                Filters = otherQueryBuilder._query.Filters,
                Limit = otherQueryBuilder._query.Limit,
                Offset = otherQueryBuilder._query.Offset,
                Sort = otherQueryBuilder._query.Sort.FirstOrDefault(),
                Type = GetJsonJoinType(joinType),
                On = otherQueryBuilder._onDefs
            }
        });

        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder Join(string otherNamespace, Action<IQueryBuilder> otherQuery, string field)
    {
        return LeftJoin(otherNamespace, otherQuery, field);
    }
    /// <inheritdoc/>

    public IQueryBuilder LeftJoin(string otherNamespace, Action<IQueryBuilder> otherQuery, string field)
    {
        _query.Filters ??= [];
        var otherQueryBuilder = new SerializableQueryBuilder(_serializer, otherNamespace);
        otherQuery(otherQueryBuilder);
        _query.Filters.Add(new FilterDef
        {
            Field = field,
            JoinQuery = new JoinedDef
            {
                Namespace = otherQueryBuilder._namespace,
                Filters = otherQueryBuilder._query.Filters,
                Limit = otherQueryBuilder._query.Limit,
                Offset = otherQueryBuilder._query.Offset,
                Sort = otherQueryBuilder._query.Sort.FirstOrDefault(),
                Type = GetJsonJoinType(Bindings.JoinType.LeftJoin),
                On = otherQueryBuilder._onDefs
            }
        });

        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder Limit(int limitItems)
    {
        _query.Limit = limitItems;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder Match(string index, params string[] keys)
    {
        return WhereString(index, Condition.EQ, keys);
    }
    /// <inheritdoc/>

    public IQueryBuilder Merge(string otherNamespace, Action<IQueryBuilder> otherQuery)
    {
        _query.MergeQueries ??= [];
        var otherQueryBuilder = new SerializableQueryBuilder(_serializer, otherNamespace);
        otherQuery(otherQueryBuilder);
        _query.MergeQueries.Add(otherQueryBuilder._query);
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder Not()
    {
        _nextOp = Bindings.Op.Not;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder Offset(int startOffset)
    {
        _query.Offset = startOffset;
        return this;
    }

    private readonly List<OnDef> _onDefs = [];
    /// <inheritdoc/>
    public IQueryBuilder On(string index, Condition condition, string joinIndex)
    {
        _onDefs.Add(new OnDef { LeftField = index, Cond = condition.ToString("g"), RightField = joinIndex, Op = _nextOp?.ToString("g") });
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder Or()
    {
        _nextOp = Bindings.Op.Or;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder ReqTotal(params string[] totalNames)
    {
        if (totalNames.Length != 0)
        {
            TotalName = totalNames[0];
            _query.ReqTotal = TotalName;
        }
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder Select(params string[] fields)
    {
        _query.SelectFilter = new(fields);
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder Sort(string sortIndex, bool desc, params object[] values)
    {
        _query.Sort ??= [];
        _query.Sort.Add(new SortDef { Field = sortIndex, Desc = desc, Values = new(values) });
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder SortStFieldDistance(string field1, string field2, bool desc)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>

    public IQueryBuilder SortStPointDistance(string field, (double X, double Y) p, bool desc)
    {
        throw new NotImplementedException();
    }

    private string GetStrictMode(QueryStrictMode mode) => mode switch
    {
        QueryStrictMode.QueryStrictModeNotSet => null,
        QueryStrictMode.QueryStrictModeNone => "none",
        QueryStrictMode.QueryStrictModeNames => "names",
        QueryStrictMode.QueryStrictModeIndexes => "indexes",
        _ => throw new NotImplementedException(),
    };
    /// <inheritdoc/>

    public IQueryBuilder Strict(QueryStrictMode mode)
    {
        _query.StrictMode = GetStrictMode(mode);
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder Where(Action<IFilterQueryBuilder> filterQuery)
    {
        _query.Filters ??= [];
        var subQueryBuilder = new SerializableQueryBuilder(_serializer, _namespace);
        filterQuery(subQueryBuilder);
        if (subQueryBuilder._query.Filters != null)
        {
            _query.Filters.Add(new FilterDef
            {
                Filters = subQueryBuilder._query.Filters,
                Op = _nextOp?.ToString("g")
            });
        }
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder Where(string index, Condition condition, object keys)
    {
        _query.Filters ??= [];
        _query.Filters.Add(new FilterDef { Field = index, Cond = condition.ToString("g"), Value = keys, Op = _nextOp?.ToString("g") });
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder WhereBetweenFields(string firstField, Condition condition, string secondField)
    {
        _query.Filters ??= new();
        _query.Filters.Add(new FilterDef { Field = firstField, Cond = condition.ToString("g"), Op = _nextOp?.ToString("g") });
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder WhereBool(string index, Condition condition, params bool[] keys)
    {
        _query.Filters ??= new();
        _query.Filters.Add(new FilterDef { Field = index, Cond = condition.ToString("g"), Value = keys, Op = _nextOp?.ToString("g") });
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder WhereComposite(string index, Condition condition, params object[] keys)
    {
        _query.Filters ??= new();
        _query.Filters.Add(new FilterDef { Field = index, Cond = condition.ToString("g"), Value = keys, Op = _nextOp?.ToString("g") });
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder WhereDouble(string index, Condition condition, params double[] keys)
    {
        _query.Filters ??= new();
        _query.Filters.Add(new FilterDef { Field = index, Cond = condition.ToString("g"), Value = keys, Op = _nextOp?.ToString("g") });
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder WhereInt(string index, Condition condition, params int[] keys)
    {
        _query.Filters ??= new();
        _query.Filters.Add(new FilterDef { Field = index, Cond = condition.ToString("g"), Value = keys, Op = _nextOp?.ToString("g") });
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder WhereInt32(string index, Condition condition, params int[] keys)
    {
        _query.Filters ??= new();
        _query.Filters.Add(new FilterDef { Field = index, Cond = condition.ToString("g"), Value = keys, Op = _nextOp?.ToString("g") });
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder WhereInt64(string index, Condition condition, params long[] keys)
    {
        _query.Filters ??= new();
        _query.Filters.Add(new FilterDef { Field = index, Cond = condition.ToString("g"), Value = keys, Op = _nextOp?.ToString("g") });
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder WhereString(string index, Condition condition, params string[] keys)
    {
        _query.Filters ??= new();
        _query.Filters.Add(new FilterDef { Field = index, Cond = condition.ToString("g"), Value = keys, Op = _nextOp?.ToString("g") });
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder WhereUuid(string index, Condition condition, params string[] keys)
    {
        _query.Filters ??= new();
        _query.Filters.Add(new FilterDef { Field = index, Cond = condition.ToString("g"), Value = keys, Op = _nextOp?.ToString("g") });
        _nextOp = Bindings.Op.And;
        return this;
    }
    /// <inheritdoc/>

    public IQueryBuilder WithRank()
    {
        _query.SelectWithRank = true;
        return this;
    }
}
