namespace ReindexerNet;

/// <summary>
/// Represents update query builder.
/// </summary>
public interface IUpdateQueryBuilder
{
    /// <summary>
    /// Removes a field from matching items within an update statement.
    /// </summary>
    /// <param name="field">Field name to remove.</param>
    /// <returns>The current query builder for fluent chaining.</returns>
    IQueryBuilder Drop(string field);
    /// <summary>
    /// Sets a field value within an update statement.
    /// </summary>
    /// <param name="field">Field name to update.</param>
    /// <param name="values">Value or values to assign.</param>
    /// <returns>The current query builder for fluent chaining.</returns>
    IQueryBuilder Set(string field, object values);
    /// <summary>
    /// Updates an indexed field by arithmetic expression.
    /// </summary>
    /// <param name="field">Field name to update.</param>
    /// <param name="value">Expression to evaluate.</param>
    /// <returns>The current query builder for fluent chaining.</returns>
    IQueryBuilder SetExpression(string field, string value);
    /// <summary>
    /// Sets an object field value within an update statement.
    /// </summary>
    /// <param name="field">Field name to update.</param>
    /// <param name="values">Object value to assign.</param>
    /// <returns>The current query builder for fluent chaining.</returns>
    IQueryBuilder SetObject(string field, object values);
}
