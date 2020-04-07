namespace ReindexerNet
{
    /// <summary>
    /// Modify modes.
    /// </summary>
    public enum ItemModifyMode
    {
        /// <summary>
        /// Update
        /// </summary>
        Update = 0,
        /// <summary>
        /// Insert
        /// </summary>
        Insert = 1,
        /// <summary>
        /// Update or Insert
        /// </summary>
        Upsert = 2,
        /// <summary>
        /// Delete
        /// </summary>
        Delete = 3
    }
}
