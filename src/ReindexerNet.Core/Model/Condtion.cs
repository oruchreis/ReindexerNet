using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNet;

/// <summary>
/// Represents logical conditions in a reindexer query.
/// </summary>
public enum Condition
{
    /// <summary>
    /// Any of
    /// </summary>
    ANY = 0,
    /// <summary>
    /// Equals
    /// </summary>
    EQ = 1,
    /// <summary>
    /// Less Than
    /// </summary>
    LT = 2,
    /// <summary>
    /// Less or Equal
    /// </summary>
    LE = 3,
    /// <summary>
    /// Greater Than
    /// </summary>
    GT = 4,
    /// <summary>
    /// Greater Or Equal
    /// </summary>
    GE = 5,
    /// <summary>
    /// Between range
    /// </summary>
    RANGE = 6,
    /// <summary>
    /// Set
    /// </summary>
    SET = 7,
    /// <summary>
    /// AllSet
    /// </summary>
    ALLSET = 8,
    /// <summary>
    /// Empty
    /// </summary>
    EMPTY = 9,
    /// <summary>
    /// Like
    /// </summary>
    LIKE = 10,

    /// <summary>
    /// Geometry subsection
    /// </summary>
    DWITHIN = 11
}