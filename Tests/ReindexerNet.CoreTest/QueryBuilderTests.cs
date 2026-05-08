using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ReindexerNet.CoreTest;

[TestClass]
public class QueryBuilderTests
{
    [TestMethod]
    public void WhereAcceptsGenericEnumerableKeys()
    {
        using var builder = new CJsonQueryBuilder(new ReindexerJsonSerializer(), "items");

        builder.Where("Id", Condition.SET, new List<int> { 1, 2, 3 });

        Assert.IsTrue(builder.CloseQuery().Length > 0);
    }

    [TestMethod]
    public void WhereStringAcceptsUtf8ValuesAboveTwoBytesPerChar()
    {
        using var builder = new CJsonQueryBuilder(new ReindexerJsonSerializer(), "items");

        builder.WhereString("Name", Condition.EQ, "漢字");

        Assert.IsTrue(builder.CloseQuery().Length > 0);
    }

    [TestMethod]
    public void SetAcceptsPrimitiveValue()
    {
        using var builder = new CJsonQueryBuilder(new ReindexerJsonSerializer(), "items");

        builder.Set("Count", 1);

        Assert.IsTrue(builder.CloseQuery().Length > 0);
    }
}
