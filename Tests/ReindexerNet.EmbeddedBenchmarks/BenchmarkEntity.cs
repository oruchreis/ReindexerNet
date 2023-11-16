using Client.Interface;
using Realms;
using Realms.Schema;
using Realms.Weaving;
using System.Runtime.Serialization;

namespace ReindexerNetBenchmark.EmbeddedBenchmarks;

[DataContract]
internal class BenchmarkEntity
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
}


internal partial class BenchmarkRealmEntity: IRealmObject
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
        return new BenchmarkEntity{ 
            Id = realmEntity.Id,
            IntProperty = realmEntity.IntProperty,
            StringProperty = realmEntity.StringProperty,
            CreateDate = realmEntity.CreateDate,
            IntArray = realmEntity.IntArray.ToArray(),
            StrArray = realmEntity.StrArray.ToArray()
            };
    }

    public static implicit operator BenchmarkRealmEntity(BenchmarkEntity realmEntity)
    {
        var entity = new BenchmarkRealmEntity{ 
            Id = realmEntity.Id,
            IntProperty = realmEntity.IntProperty,
            StringProperty = realmEntity.StringProperty,
            CreateDate = realmEntity.CreateDate,
            };
        entity.IntArray.UnionWith(realmEntity.IntArray);
        entity.StrArray.UnionWith(realmEntity.StrArray);
        return entity;
    }
}