using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet;

/// <summary>
/// Represents filter query operation builder
/// </summary>
public interface IFilterQueryBuilder
{
    /// <summary>
    /// Creates sub filter query. Equals to Brackets () in an SQL query in the whre condition.
    /// </summary>
    /// <param name="filterQuery"></param>
    /// <returns></returns>
    IQueryBuilder Where(Action<IFilterQueryBuilder> filterQuery);

    /// <summary>
    /// Add where condition to DB query
    /// </summary>
    /// <param name="index"></param>
    /// <param name="condition"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    IQueryBuilder Where(string index, Condition condition, object keys);
    /// <summary>
    ///  Add comparing two fields where condition to DB query
    /// </summary>
    /// <param name="firstField"></param>
    /// <param name="condition"></param>
    /// <param name="secondField"></param>
    /// <returns></returns>
    IQueryBuilder WhereBetweenFields(string firstField, Condition condition, string secondField);
    /// <summary>
    /// Add where condition to DB query with bool args
    /// </summary>
    /// <param name="index"></param>
    /// <param name="condition"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    IQueryBuilder WhereBool(string index, Condition condition, params bool[] keys);
    /// <summary>
    /// Add where condition to DB query with interface args for composite indexes
    /// </summary>
    /// <param name="index"></param>
    /// <param name="condition"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    IQueryBuilder WhereComposite(string index, Condition condition, params object[] keys);
    /// <summary>
    /// Add where condition to DB query with float args
    /// </summary>
    /// <param name="index"></param>
    /// <param name="condition"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    IQueryBuilder WhereDouble(string index, Condition condition, params double[] keys);
    /// <summary>
    /// Add where condition to DB query with int args
    /// </summary>
    /// <param name="index"></param>
    /// <param name="condition"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    IQueryBuilder WhereInt(string index, Condition condition, params int[] keys);
    /// <summary>
    /// Add where condition to DB query with int args
    /// </summary>
    /// <remarks>Same as <see cref="WhereInt(string, Condition, int[])"/></remarks>
    /// <param name="index"></param>
    /// <param name="condition"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    IQueryBuilder WhereInt32(string index, Condition condition, params int[] keys);
    /// <summary>
    /// Add where condition to DB query with int64(long) args
    /// </summary>
    /// <param name="index"></param>
    /// <param name="condition"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    IQueryBuilder WhereInt64(string index, Condition condition, params long[] keys);
    /// <summary>
    /// Add where condition to DB query with string args
    /// </summary>
    /// <param name="index"></param>
    /// <param name="condition"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    IQueryBuilder WhereString(string index, Condition condition, params string[] keys);
    /// <summary>
    /// Add where condition to DB query with guid args
    /// </summary>
    /// <param name="index"></param>
    /// <param name="condition"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    IQueryBuilder WhereUuid(string index, Condition condition, params string[] keys);
    /// <summary>
    /// Add where condition to DB query with guid args
    /// </summary>
    /// <param name="index"></param>
    /// <param name="condition"></param>
    /// <param name="keys"></param>
    /// <returns></returns>
    IQueryBuilder WhereGuid(string index, Condition condition, params Guid[] keys);
}
