using ReindexerNet.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using static ReindexerNet.Internal.Bindings;

namespace ReindexerNet;

public enum QueryStrictMode
{
    QueryStrictModeNotSet = 0,
    QueryStrictModeNone = 1,
    QueryStrictModeNames = 2,
    QueryStrictModeIndexes = 3
}

/// <summary>
/// Builds CJson query from given query.
/// </summary>
#if NET5_0_OR_GREATER
[DebuggerTypeProxy(typeof(CJsonQueryBuilderDebugTypeProxy))]
#endif
public sealed class CJsonQueryBuilder : IQueryBuilder, IUpdateQueryBuilder, ISerializableQueryBuilder
{
    private readonly ReindexerJsonSerializer _jsonSerializer;
    private Bindings.Op _nextOp = Bindings.Op.And;
    private readonly CJsonWriter _ser = new();
    private CJsonQueryBuilder _root;
    private readonly List<CJsonQueryBuilder> _joinQueries = [];
    private readonly List<CJsonQueryBuilder> _mergedQueries = [];
    private Bindings.JoinType _joinType = 0;
    private bool _closed;
    private int _queriesCount;
    private readonly List<int> _opennedBrackets = [];
    private readonly List<StackFrame> _traceNew = [];
    private readonly List<StackFrame> _traceClose = [];

    /// <inheritdoc/>
    public string TotalName { get; set; } = string.Empty;
    /// <inheritdoc/>
    public int FetchCount { get; set; } = 1000;

    /// <summary>
    /// Builds CJson query from given query.
    /// </summary>
    /// <param name="jsonSerializer"></param>
    /// <param name="namespace"></param>
    public CJsonQueryBuilder(ReindexerJsonSerializer jsonSerializer, string @namespace)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([@namespace], "namespace");
        }

        _jsonSerializer = jsonSerializer;
        _ser.PutVString(@namespace);
    }

    /// <inheritdoc/>
    public IQueryBuilder Where(string index, Condition condition, object keys)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([index, condition, keys]);
        }

        Type t = keys.GetType();
        List<object> keysList = [];
        if (t.IsArray)
        {
            keysList = ((Array)keys).Cast<object>().ToList();
        }
        else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
        {
            keysList = ((List<object>)keys).ToList();
        }
        else
        {
            keysList.Add(keys);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Condition);
        _ser.PutVString(index);
        _ser.PutVarCUInt((int)_nextOp);
        _ser.PutVarCUInt((int)condition);
        _nextOp = Bindings.Op.And;
        _queriesCount++;
        _ser.PutVarCUInt(keysList.Count);
        foreach (object key in keysList)
        {
            PutValue(key);
        }

        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder WhereBetweenFields(string firstField, Condition condition, string secondField)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([firstField, condition, secondField]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.BetweenFieldsCondition);
        _ser.PutVarCUInt((int)_nextOp);
        _ser.PutVString(firstField);
        _ser.PutVarCUInt((int)condition);
        _ser.PutVString(secondField);
        _nextOp = Bindings.Op.And;
        _queriesCount++;
        return this;
    }

    public IQueryBuilder OpenBracket()
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([]);
        }
        _ser.PutVarCUInt((int)Bindings.Query.OpenBracket);
        _ser.PutVarCUInt((int)_nextOp);
        _nextOp = Bindings.Op.And;
        _opennedBrackets.Add(_queriesCount);
        _queriesCount++;
        return this;
    }

    public IQueryBuilder CloseBracket()
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([]);
        }

        if (_nextOp != Bindings.Op.And)
        {
            throw new ReindexerNetException("Operation before close bracket");
        }
        if (_opennedBrackets.Count < 1)
        {
            throw new ReindexerNetException("Close bracket before open it");
        }
        _ser.PutVarCUInt((int)Bindings.Query.CloseBracket);
        _opennedBrackets.RemoveAt(_opennedBrackets.Count - 1);
        return this;
    }

    private void PutValue(object value)
    {
        var t = value.GetType();
        if (value is bool b)
        {
            _ser.PutVarCUInt((int)Bindings.Value.Bool);
            _ser.PutVarUInt(b ? 1u : 0u);
        }
        else if (value is int i)
        {
            _ser.PutVarCUInt((int)Bindings.Value.Int);
            _ser.PutVarInt(i);
        }
        else if (value is long l)
        {
            _ser.PutVarCUInt((int)Bindings.Value.Int64);
            _ser.PutVarInt(l);
        }
        else if (value is string s)
        {
            _ser.PutVarCUInt((int)Bindings.Value.String);
            _ser.PutVString(s);
        }
        else if (value is double d)
        {
            _ser.PutVarCUInt((int)Bindings.Value.Double);
            _ser.PutDouble(d);
        }
        else if (value is ICollection<object> c)
        {
            _ser.PutVarCUInt((int)Bindings.Value.Tuple);
            _ser.PutVarCUInt(c.Count);
            foreach (object val in c)
            {
                PutValue(val);
            }
        }
        else
        {
            throw new ReindexerNetException("Invalid reflection type " + t.Name.ToString());
        }
    }

    /// <inheritdoc/>
    public IQueryBuilder Where(Action<IQueryBuilder> filterQuery)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([filterQuery]);
        }

        OpenBracket();

        filterQuery(this);

        CloseBracket();
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder WhereInt(string index, Condition condition, params int[] keys)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([index, condition, keys]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Condition);
        _ser.PutVString(index);
        _ser.PutVarCUInt((int)_nextOp);
        _ser.PutVarCUInt((int)condition);
        _nextOp = Bindings.Op.And;
        _queriesCount++;
        _ser.PutVarCUInt(keys.Length);
        foreach (int key in keys)
        {
            _ser.PutVarCUInt((int)Bindings.Value.Int);
            _ser.PutVarInt(key);
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder WhereInt32(string index, Condition condition, params int[] keys)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([index, condition, keys]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Condition);
        _ser.PutVString(index);
        _ser.PutVarCUInt((int)_nextOp);
        _ser.PutVarCUInt((int)condition);
        _nextOp = Bindings.Op.And;
        _queriesCount++;
        _ser.PutVarCUInt(keys.Length);
        foreach (int key in keys)
        {
            _ser.PutVarCUInt((int)Bindings.Value.Int);
            _ser.PutVarInt(key);
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder WhereInt64(string index, Condition condition, params long[] keys)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([index, condition, keys]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Condition);
        _ser.PutVString(index);
        _ser.PutVarCUInt((int)_nextOp);
        _ser.PutVarCUInt((int)condition);
        _nextOp = Bindings.Op.And;
        _queriesCount++;
        _ser.PutVarCUInt(keys.Length);
        foreach (long key in keys)
        {
            _ser.PutVarCUInt((int)Bindings.Value.Int64);
            _ser.PutVarInt(key);
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder WhereString(string index, Condition condition, params string[] keys)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([index, condition, keys]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Condition);
        _ser.PutVString(index);
        _ser.PutVarCUInt((int)_nextOp);
        _ser.PutVarCUInt((int)condition);
        _nextOp = Bindings.Op.And;
        _queriesCount++;
        _ser.PutVarCUInt(keys.Length);
        foreach (string key in keys)
        {
            _ser.PutVarCUInt((int)Bindings.Value.String);
            _ser.PutVString(key);
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder WhereUuid(string index, Condition condition, params string[] keys)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([index, condition, keys]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Condition);
        _ser.PutVString(index);
        _ser.PutVarCUInt((int)_nextOp);
        _ser.PutVarCUInt((int)condition);
        _nextOp = Bindings.Op.And;
        _queriesCount++;
        _ser.PutVarCUInt(keys.Length);
        foreach (string key in keys)
        {
            if (Guid.TryParse(key, out Guid uuid))
            {
                _ser.PutVarCUInt((int)Bindings.Value.Uuid);
                _ser.PutUuid(uuid);
            }
            else
            {
                _ser.PutVarCUInt((int)Bindings.Value.String);
                _ser.PutVString(key);
            }
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder WhereGuid(string index, Condition condition, params Guid[] keys)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([index, condition, keys]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Condition);
        _ser.PutVString(index);
        _ser.PutVarCUInt((int)_nextOp);
        _ser.PutVarCUInt((int)condition);
        _nextOp = Bindings.Op.And;
        _queriesCount++;
        _ser.PutVarCUInt(keys.Length);
        foreach (var uuid in keys)
        {
            _ser.PutVarCUInt((int)Bindings.Value.Uuid);
            _ser.PutUuid(uuid);
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder WhereComposite(string index, Condition condition, params object[] keys)
    {
        return Where(index, condition, keys);
    }

    /// <inheritdoc/>
    public IQueryBuilder Match(string index, params string[] keys)
    {
        return WhereString(index, Condition.EQ, keys);
    }

    /// <inheritdoc/>
    public IQueryBuilder WhereBool(string index, Condition condition, params bool[] keys)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([index, condition, keys]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Condition);
        _ser.PutVString(index);
        _ser.PutVarCUInt((int)_nextOp);
        _ser.PutVarCUInt((int)condition);
        _nextOp = Bindings.Op.And;
        _queriesCount++;
        _ser.PutVarCUInt(keys.Length);
        foreach (bool key in keys)
        {
            _ser.PutVarCUInt((int)Bindings.Value.Bool);
            _ser.PutVarUInt(key ? 1u : 0u);
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder WhereDouble(string index, Condition condition, params double[] keys)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([index, condition, keys]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Condition);
        _ser.PutVString(index);
        _ser.PutVarCUInt((int)_nextOp);
        _ser.PutVarCUInt((int)condition);
        _nextOp = Bindings.Op.And;
        _queriesCount++;
        _ser.PutVarCUInt(keys.Length);
        foreach (double key in keys)
        {
            _ser.PutVarCUInt((int)Bindings.Value.Double);
            _ser.PutDouble(key);
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder DWithin(string index, (double start, double end) point, double distance)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([index, point, distance]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Condition);
        _ser.PutVString(index);
        _ser.PutVarCUInt((int)_nextOp);
        _ser.PutVarCUInt((int)Condition.DWITHIN);
        _nextOp = Bindings.Op.And;
        _queriesCount++;
        _ser.PutVarCUInt(3);
        _ser.PutVarCUInt((int)Bindings.Value.Double);
        _ser.PutDouble(point.start);
        _ser.PutVarCUInt((int)Bindings.Value.Double);
        _ser.PutDouble(point.end);
        _ser.PutVarCUInt((int)Bindings.Value.Double);
        _ser.PutDouble(distance);
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder AggregateSum(string field)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([field]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Aggregation);
        _ser.PutVarCUInt((int)Bindings.Agg.Sum);
        _ser.PutVarCUInt(1);
        _ser.PutVString(field);
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder AggregateAvg(string field)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([field]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Aggregation);
        _ser.PutVarCUInt((int)Bindings.Agg.Avg);
        _ser.PutVarCUInt(1);
        _ser.PutVString(field);
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder AggregateMin(string field)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([field]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Aggregation);
        _ser.PutVarCUInt((int)Bindings.Agg.Min);
        _ser.PutVarCUInt(1);
        _ser.PutVString(field);
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder AggregateMax(string field)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([field]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Aggregation);
        _ser.PutVarCUInt((int)Bindings.Agg.Max);
        _ser.PutVarCUInt(1);
        _ser.PutVString(field);
        return this;
    }

    private sealed class AggregateFacetRequest : IAggregateFacetRequest
    {
        private readonly CJsonQueryBuilder query;

        public AggregateFacetRequest(CJsonQueryBuilder query)
        {
            this.query = query;
        }

        public IAggregateFacetRequest Limit(int limit)
        {
            if (Debugger.IsAttached)
            {
                query.AddToDebugView([limit], action: "Facet.Limit");
            }
            query._ser.PutVarCUInt((int)Bindings.Query.AggregationLimit);
            query._ser.PutVarCUInt(limit);
            return this;
        }

        public IAggregateFacetRequest Offset(int offset)
        {
            if (Debugger.IsAttached)
            {
                query.AddToDebugView([offset], action: "Facet.Offset");
            }
            query._ser.PutVarCUInt((int)Bindings.Query.AggregationOffset);
            query._ser.PutVarCUInt(offset);
            return this;
        }

        public IAggregateFacetRequest Sort(string field, bool desc)
        {
            if (Debugger.IsAttached)
            {
                query.AddToDebugView([field, desc], action: "Facet.Sort");
            }
            query._ser.PutVarCUInt((int)Bindings.Query.AggregationSort);
            query._ser.PutVString(field);
            if (desc)
            {
                query._ser.PutVarCUInt(1);
            }
            else
            {
                query._ser.PutVarCUInt(0);
            }
            return this;
        }
    }

    /// <inheritdoc/>
    public IQueryBuilder AggregateFacet(Action<IAggregateFacetRequest> aggFacetQuery, params string[] fields)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([fields]);
        }
        _ser.PutVarCUInt((int)Bindings.Query.Aggregation);
        _ser.PutVarCUInt((int)Bindings.Agg.Facet);
        _ser.PutVarCUInt(fields.Length);
        foreach (string field in fields)
        {
            _ser.PutVString(field);
        }
        aggFacetQuery(new AggregateFacetRequest(this));
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder Sort(string sortIndex, bool desc, params object[] values)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([sortIndex, desc, values]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.SortIndex);
        _ser.PutVString(sortIndex);
        if (desc)
        {
            _ser.PutVarUInt(1);
        }
        else
        {
            _ser.PutVarUInt(0);
        }
        _ser.PutVarCUInt(values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            PutValue(values[i]);
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder SortStPointDistance(string field, (double X, double Y) p, bool desc)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([field, p, desc]);
        }

        var sb = new StringBuilder();
        sb.Append("ST_Distance(")
          .Append(field)
          .Append(",ST_GeomFromText('point(")
          .Append(p.X.ToString("f", System.Globalization.CultureInfo.InvariantCulture))
          .Append(' ')
          .Append(p.Y.ToString("f", System.Globalization.CultureInfo.InvariantCulture))
          .Append(")'))");

        var expression = sb.ToString();
        return Sort(expression, desc);
    }

    /// <inheritdoc/>
    public IQueryBuilder SortStFieldDistance(string field1, string field2, bool desc)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([field1, field2, desc]);
        }

        var sb = new StringBuilder();
        sb.Append("ST_Distance(")
          .Append(field1)
          .Append(',')
          .Append(field2)
          .Append(')');
        return Sort(sb.ToString(), desc);
    }

    /// <inheritdoc/>
    public IQueryBuilder And()
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([]);
        }

        _nextOp = Bindings.Op.And;
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder Or()
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([]);
        }

        _nextOp = Bindings.Op.Or;
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder Not()
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([]);
        }

        _nextOp = Bindings.Op.Not;
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder Distinct(string distinctIndex)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([distinctIndex]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Aggregation);
        _ser.PutVarCUInt((int)Bindings.Agg.Distinct);
        _ser.PutVarCUInt(1);
        _ser.PutVString(distinctIndex);
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder ReqTotal(params string[] totalNames)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([totalNames]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.ReqTotal);
        _ser.PutVarCUInt((int)Bindings.TotalMode.AccurateTotal);
        if (totalNames.Length != 0)
        {
            TotalName = totalNames[0];
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder CachedTotal(params string[] totalNames)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([totalNames]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.ReqTotal);
        _ser.PutVarCUInt((int)Bindings.TotalMode.CachedTotal);
        if (totalNames.Length != 0)
        {
            TotalName = totalNames[0];
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder Limit(int limitItems)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([limitItems]);
        }

        if (limitItems > Bindings.CInt32Max)
        {
            limitItems = Bindings.CInt32Max;
        }
        _ser.PutVarCUInt((int)Bindings.Query.Limit);
        _ser.PutVarCUInt(limitItems);
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder Offset(int startOffset)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([startOffset]);
        }

        if (startOffset > Bindings.CInt32Max)
        {
            startOffset = Bindings.CInt32Max;
        }
        _ser.PutVarCUInt((int)Bindings.Query.Offset);
        _ser.PutVarCUInt(startOffset);
        return this;
    }

    public IQueryBuilder Debug(int level)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([level]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.DebugLevel);
        _ser.PutVarCUInt(level);
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder Strict(QueryStrictMode mode)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([mode]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.StrictMode);
        _ser.PutVarCUInt((int)mode);
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder Explain()
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.Explain);
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder WithRank()
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.WithRank);
        return this;
    }

    private static bool enableDebug => Environment.GetEnvironmentVariable("ReindexerNet_Debug") != "1";

    private static void mktrace(ref List<StackFrame> buf)
    {
        if (enableDebug)
        {
            if (buf == null)
            {
                buf = [];
            }

            StackTrace stackTrace = new StackTrace(false);
            buf.AddRange(stackTrace.GetFrames());
        }
    }

    private void panicTrace(string msg)
    {
        if (Environment.GetEnvironmentVariable("ReindexerNet_Debug") != "1")
        {
            Console.WriteLine("To see query allocation/close traces set ReindexerNet_Debug=1 environment variable!");
        }
        else
        {
            Console.WriteLine("CJsonQueryBuilder allocation trace: " + _traceNew.ToString() + "\n\nQuery close trace " + _traceClose.ToString() + "\n\n");
        }
        throw new ReindexerNetException(msg);
    }

    private string getValueJSON(object? value)
    {
        bool ok = false;
        ReadOnlySpan<byte> objectJSON = Array.Empty<byte>();
        Type t = value?.GetType();
        if (value == null)
        {
            objectJSON = Encoding.UTF8.GetBytes("{}");
        }
        else if (t == typeof(object) || t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            objectJSON = _jsonSerializer.Serialize(value);
        }
        else if (!ok)
        {
            throw new ReindexerNetException("SetObject doesn't support this type of objects: " + t.Name.ToString());
        }

        return Encoding.UTF8.GetString(objectJSON
#if NETSTANDARD2_0 || NET472
            .ToArray()
#endif
            );
    }

    /// <inheritdoc/>
    public IQueryBuilder SetObject(string field, object values)
    {
        int size = 1;
        bool isArray = false;
        var v = values as IList<object>;
        if (values is not byte[] && v != null)
        {
            size = v.Count;
            isArray = true;
        }
        string[] jsonValues = new string[size];
        if (isArray)
        {
            for (int i = 0; i < size; i++)
            {
                jsonValues[i] = getValueJSON(v[i]);
            }
        }
        else if (size > 0)
        {
            jsonValues[0] = getValueJSON(values);
        }
        _ser.PutVarCUInt((int)Bindings.Query.UpdateObject);
        _ser.PutVString(field);
        _ser.PutVarCUInt(size);
        if (isArray)
        {
            _ser.PutVarCUInt(1);
        }
        else
        {
            _ser.PutVarCUInt(0);
        }
        for (int i = 0; i < size; i++)
        {
            _ser.PutVarUInt(0);
            _ser.PutVarCUInt((int)Bindings.Value.String);
            _ser.PutVString(jsonValues[i]);
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder Set(string field, object values)
    {
        if (values != null)
        {
            Type t = values.GetType();
            if (t == typeof(object) || t.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                return SetObject(field, values);
            }
            if (t.IsArray && !t.GetElementType().IsPrimitive && t.GetElementType() != typeof(string))
            {
                return SetObject(field, values);
            }
        }

        var v = values as IList<object>;
        Bindings.Query cmd = Bindings.Query.UpdateField;
        if (v != null && v.Count <= 1)
        {
            cmd = Bindings.Query.UpdateFieldV2;
        }
        _ser.PutVarCUInt((int)cmd);
        _ser.PutVString(field);
        if (values == null)
        {
            if (cmd == Bindings.Query.UpdateFieldV2)
            {
                _ser.PutVarUInt(0);
            }
            _ser.PutVarUInt(0);
        }
        else if (v != null)
        {
            if (cmd == Bindings.Query.UpdateFieldV2)
            {
                _ser.PutVarUInt(1);
            }
            _ser.PutVarCUInt(v.Count);
            for (int i = 0; i < v.Count; i++)
            {
                _ser.PutVarUInt(0);
                PutValue(v[i]);
            }
        }
        else
        {
            if (cmd == Bindings.Query.UpdateFieldV2)
            {
                _ser.PutVarUInt(0);
            }
            _ser.PutVarCUInt(1);
            _ser.PutVarUInt(0);
            PutValue(v);
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder Drop(string field)
    {
        _ser.PutVarCUInt((int)Bindings.Query.DropField);
        _ser.PutVString(field);
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder SetExpression(string field, string value)
    {
        _ser.PutVarCUInt((int)Bindings.Query.UpdateField);
        _ser.PutVString(field);
        _ser.PutVarCUInt(1);
        _ser.PutVarUInt(1);
        PutValue(value);
        return this;
    }

    private CJsonQueryBuilder join(CJsonQueryBuilder q2, string field, Bindings.JoinType joinType)
    {
        var query = this;
        if (_root != null)
        {
            query = _root;
        }
        if (q2._root != null)
        {
            throw new Exception("this.Join call on already joined this. You should create new Query");
        }
        if (joinType != Bindings.JoinType.LeftJoin)
        {
            _ser.PutVarCUInt((int)Bindings.Query.JoinCondition);
            _ser.PutVarCUInt((int)joinType);
            _ser.PutVarCUInt(_joinQueries.Count);
        }
        q2._joinType = joinType;
        q2._root = query;
        _joinQueries.Add(q2);
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder InnerJoin(string otherNamespace, Action<IQueryBuilder> otherQuery, string field)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([otherNamespace, otherQuery, field]);
        }

        var otherQueryBuilder = new CJsonQueryBuilder(_jsonSerializer, otherNamespace);
        otherQuery(otherQueryBuilder);
        if (_nextOp == Bindings.Op.Or)
        {
            _nextOp = Bindings.Op.And;
            return join(otherQueryBuilder, field, Bindings.JoinType.OrInnerJoin);
        }
        return join(otherQueryBuilder, field, Bindings.JoinType.InnerJoin);
    }

    /// <inheritdoc/>
    public IQueryBuilder Join(string otherNamespace, Action<IQueryBuilder> otherQuery, string field)
    {
        return LeftJoin(otherNamespace, otherQuery, field);
    }

    /// <inheritdoc/>
    public IQueryBuilder LeftJoin(string otherNamespace, Action<IQueryBuilder> otherQuery, string field)
    {        
        if (Debugger.IsAttached)
        {
            AddToDebugView([otherNamespace, otherQuery, field]);
        }

        var otherQueryBuilder = new CJsonQueryBuilder(_jsonSerializer, otherNamespace);
        otherQuery(otherQueryBuilder);
        return join(otherQueryBuilder, field, Bindings.JoinType.LeftJoin);
    }

    /// <inheritdoc/>
    public IQueryBuilder Merge(string otherNamespace, Action<IQueryBuilder> otherQuery)
    {       
        if (Debugger.IsAttached)
        {
            AddToDebugView([otherNamespace, otherQuery]);
        }

        var otherQueryBuilder = new CJsonQueryBuilder(_jsonSerializer, otherNamespace);
        otherQuery(otherQueryBuilder);
        var query = this;
        if (_root != null)
        {
            query = _root;
        }
        if (otherQueryBuilder._root != null)
        {
            otherQueryBuilder = otherQueryBuilder._root;
        }
        otherQueryBuilder._root = query;
        _mergedQueries.Add(otherQueryBuilder);
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder On(string index, Condition condition, string joinIndex)
    {   
        if (Debugger.IsAttached)
        {
            AddToDebugView([index, condition, joinIndex]);
        }

        if (_closed)
        {
            panicTrace("this.On call on already closed this. You should create new Query");
        }
        if (_root == null)
        {
            throw new ReindexerNetException("Can't join on root query");
        }
        _ser.PutVarCUInt((int)Bindings.Query.JoinOn);
        _ser.PutVarCUInt((int)_nextOp);
        _ser.PutVarCUInt((int)condition);
        _ser.PutVString(index);
        _ser.PutVString(joinIndex);
        _nextOp = Bindings.Op.And;
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder Select(params string[] fields)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([fields]);
        }

        foreach (string field in fields)
        {
            _ser.PutVarCUInt((int)Bindings.Query.SelectFilter);
            _ser.PutVString(field);
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder Functions(params string[] fields)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([fields]);
        }

        foreach (string field in fields)
        {
            _ser.PutVarCUInt((int)Bindings.Query.SelectFunction);
            _ser.PutVString(field);
        }
        return this;
    }

    /// <inheritdoc/>
    public IQueryBuilder EqualPosition(params string[] fields)
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([fields]);
        }

        _ser.PutVarCUInt((int)Bindings.Query.EqualPosition);
        if (_opennedBrackets.Count == 0)
        {
            _ser.PutVarCUInt(0);
        }
        else
        {
            _ser.PutVarCUInt(_opennedBrackets[_opennedBrackets.Count - 1] + 1);
        }
        _ser.PutVarCUInt(fields.Length);
        foreach (string field in fields)
        {
            _ser.PutVString(field);
        }
        return this;
    }

    /// <inheritdoc/>
    public ReadOnlySpan<byte> CloseQuery()
    {
        if (Debugger.IsAttached)
        {
            AddToDebugView([]);
        }

        //var ns = db.GetNS(q.Namespace);
        //if (ns != null)
        //{
        //    q.NsArray.Add(new NsArrayEntry { Ns = ns, CjsonState = ns.CjsonState.Copy() });
        //}
        //else
        //{
        //    return (null, new ReindexerNetException("Namespace retrieval failed"));
        //}

        //foreach (var sq in _mergedQueries)
        //{
        //    ns = db.GetNS(sq.Namespace);
        //    if (ns != null)
        //    {
        //        q.NsArray.Add(new NsArrayEntry { Ns = ns, CjsonState = ns.CjsonState.Copy() });
        //    }
        //    else
        //    {
        //        return (null, new Exception("Namespace retrieval failed"));
        //    }
        //}

        //foreach (var sq in _joinQueries)
        //{
        //    ns = db.GetNS(sq.Namespace);
        //    if (ns != null)
        //    {
        //        q.NsArray.Add(new NsArrayEntry { Ns = ns, CjsonState = ns.CjsonState.Copy() });
        //    }
        //    else
        //    {
        //        return (null, new Exception("Namespace retrieval failed"));
        //    }
        //}

        //foreach (var mq in q.MergedQueries)
        //{
        //    foreach (var sq in mq.JoinQueries)
        //    {
        //        ns = db.GetNS(sq.Namespace);
        //        if (ns != null)
        //        {
        //            q.NsArray.Add(new NsArrayEntry { Ns = ns, CjsonState = ns.CjsonState.Copy() });
        //        }
        //        else
        //        {
        //            return (null, new Exception("Namespace retrieval failed"));
        //        }
        //    }
        //}

        _ser.PutVarCUInt((int)Bindings.Query.End);
        foreach (var sq in _joinQueries)
        {
            _ser.PutVarCUInt((int)sq._joinType);
            _ser.Append(sq._ser);
            _ser.PutVarCUInt((int)Bindings.Query.End);
        }

        foreach (var mq in _mergedQueries)
        {
            _ser.PutVarCUInt((int)Bindings.JoinType.Merge);
            _ser.Append(mq._ser);
            _ser.PutVarCUInt((int)Bindings.Query.End);
            foreach (var sq in mq._joinQueries)
            {
                _ser.PutVarCUInt((int)sq._joinType);
                _ser.Append(sq._ser);
                _ser.PutVarCUInt((int)Bindings.Query.End);
            }
        }

        //foreach (var nsEntry in q.NsArray)
        //{
        //    q.PtVersions.Add(nsEntry.LocalCjsonState.Version ^ nsEntry.LocalCjsonState.StateToken);
        //}

        //if (asJson)
        //{
        //    // json iterator not support fetch queries
        //    FetchCount = -1;
        //}

        _closed = true;

        return _ser.CurrentBuffer;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _ser.Dispose();
    }

    private List<(string Action, Dictionary<string, object> Parameters)> _debugView = [];
#if NET5_0_OR_GREATER
    private void AddToDebugView(object[] parameters, [CallerMemberName] string action = null, [CallerArgumentExpression(nameof(parameters))] string parameterExpression = null)
    {
        _debugView.Add((action, parameterExpression.TrimStart('[').TrimEnd(']').Split(",").Zip(parameters).ToDictionary(z => z.First, z => z.Second)));
    }

#else
    private void AddToDebugView(object[] parameters, [CallerMemberName]string action = null)
    {
        _debugView.Add((action, parameters.Select((p,i) => (p,i)).ToDictionary(kv => kv.i.ToString(), kv => kv.p)));
    }
#endif

    private sealed class CJsonQueryBuilderDebugTypeProxy(CJsonQueryBuilder cJsonQueryBuilder)
    {
        private readonly CJsonQueryBuilder cJsonQueryBuilder = cJsonQueryBuilder;

        public List<(string Action, Dictionary<string, object> Parameters)> QueryAfterDebuggerAttached => cJsonQueryBuilder._debugView;
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member