namespace ReindexerNet;

/// <summary>
/// Represents update query builder.
/// </summary>
public interface IUpdateQueryBuilder
{
    /// <summary>
    /// Drop removes field from item within Update statement
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    IQueryBuilder Drop(string field);
    /// <summary>
    /// Set adds update field request for update query
    /// </summary>
    /// <param name="field"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    IQueryBuilder Set(string field, object values);
    /// <summary>
    /// SetExpression updates indexed field by arithmetical expression
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    IQueryBuilder SetExpression(string field, string value);
    /// <summary>
    /// SetObject adds update of object field request for update query
    /// </summary>
    /// <param name="field"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    IQueryBuilder SetObject(string field, object values);
}
