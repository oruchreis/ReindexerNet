﻿using Client.Interface;
using Realms;
using Realms.Schema;
using Realms.Weaving;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ReindexerNetBenchmark.EmbeddedBenchmarks;

[DataContract]
public sealed class BenchmarkEntity
{
    [ServerSideValue(Client.Core.IndexType.Primary)]
    [DataMember(Order = 0)]
    public Guid Id { get; set; }

    [ServerSideValue(Client.Core.IndexType.Ordered)]
    [DataMember(Order = 1)]
    public int? IntProperty { get; set; }

    [ServerSideValue(Client.Core.IndexType.Dictionary)]
    [DataMember(Order = 2)]
    public string? StringProperty { get; set; }

    [ServerSideValue(Client.Core.IndexType.Dictionary)]
    [DataMember(Order = 3)]
    public DateTimeOffset? CreateDate { get; set; }

    [ServerSideValue(Client.Core.IndexType.Dictionary)]
    [DataMember(Order = 4)]
    public int[] IntArray { get; set; }

    [ServerSideValue(Client.Core.IndexType.Dictionary)]
    [DataMember(Order = 5)]
    public string[] StrArray { get; set; }

    public BenchmarkEntity PreventLazy()
        => new BenchmarkEntity
        {
            Id = Id,
            IntArray = IntArray.ToArray(),
            StrArray = StrArray.ToArray(),
            CreateDate = CreateDate,
            IntProperty = IntProperty,
            StringProperty = StringProperty,            
        };
}

public sealed partial class BenchmarkRealmEntity : IRealmObject
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed(Realms.IndexType.General)]
    public int? IntProperty { get; set; }

    [Indexed(Realms.IndexType.General)]
    public string? StringProperty { get; set; }

    [Indexed(Realms.IndexType.General)]
    public DateTimeOffset? CreateDate { get; set; }

    public ISet<int> IntArray { get; }

    public ISet<string> StrArray { get; }

    public static implicit operator BenchmarkEntity(BenchmarkRealmEntity realmEntity)
    {
        return new BenchmarkEntity
        {
            Id = realmEntity.Id,
            IntProperty = realmEntity.IntProperty,
            StringProperty = realmEntity.StringProperty,
            CreateDate = realmEntity.CreateDate,
            IntArray = [.. realmEntity.IntArray],
            StrArray = [.. realmEntity.StrArray]
        };
    }

    public static implicit operator BenchmarkRealmEntity(BenchmarkEntity realmEntity)
    {
        var entity = new BenchmarkRealmEntity
        {
            Id = realmEntity.Id,
            IntProperty = realmEntity.IntProperty,
            StringProperty = realmEntity.StringProperty,
            CreateDate = realmEntity.CreateDate,
        };
        entity.IntArray.UnionWith(realmEntity.IntArray);
        entity.StrArray.UnionWith(realmEntity.StrArray);
        return entity;
    }

    public BenchmarkEntity PreventLazy()
        => (BenchmarkEntity)this;
}