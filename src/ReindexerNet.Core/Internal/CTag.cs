using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace ReindexerNet.Internal;

readonly ref struct CTag(uint value)
{
    private const int TypeBits = 3;
    private const int TypeMask = (1 << TypeBits) - 1;
    private const int NameBits = 12;
    private const int NameMask = (1 << NameBits) - 1;
    private const int FieldBits = 10;
    private const int FieldMask = (1 << FieldBits) - 1;
    private const int ReservedBits = 4;
    private const int Type2Offset = TypeBits + NameBits + FieldBits + ReservedBits;

    public enum TagType
    {
        Varint = 0,
        Double = 1,
        String = 2,
        Bool = 3,
        Null = 4,
        Array = 5,
        Object = 6,
        End = 7,
        UUID = 8
    }

    public readonly uint Value = value;

    public int Name => (int)(Value >> TypeBits) & NameMask;

    public TagType Type => (TagType)((int)Value & TypeMask | (((int)Value >> Type2Offset) & TypeMask) << TypeBits);

    public int Field => ((int)Value >> (TypeBits + NameBits)) & FieldMask - 1;

    public string Dump()
    {
        return $"({TagTypeName(Type)} n:{Name} f:{Field})";
    }

    private static string TagTypeName(TagType tagType)
    {
        switch (tagType)
        {
            case TagType.Varint:
                return "<varint>";
            case TagType.Object:
                return "<object>";
            case TagType.End:
                return "<end>";
            case TagType.Array:
                return "<array>";
            case TagType.Bool:
                return "<bool>";
            case TagType.String:
                return "<string>";
            case TagType.Double:
                return "<double>";
            case TagType.Null:
                return "<null>";
            case TagType.UUID:
                return "<uuid>";
            default:
                return $"<unknown {(int)tagType}>";
        }
    }

    public static CTag Create(uint tagType, uint tagName, uint tagField)
    {
        return new CTag((tagType & TypeMask) | (tagName << TypeBits) | (tagField << (NameBits + TypeBits)) | (((tagType >> TypeBits) & TypeMask) << Type2Offset));
    }
}

readonly ref struct CArrayTag(uint value)
{
    private const int CountBits = 24;
    private const int CountMask = (1 << CountBits) - 1;

    public readonly uint Value = value;

    public int Count => (int)Value & CountMask;

    public int Tag => (int)Value >> CountBits;

    public static CArrayTag Create(uint count, uint tag)
    {
        return new CArrayTag(count | (tag << CountBits));
    }
}

public class CtagsCacheEntry
{
    public List<int> StructIdx { get; set; }
    public CtagsCache SubCache { get; set; }
}

public class CtagsCache : List<CtagsCacheEntry> { }

public class StructCache : Dictionary<Type, CtagsCache> { }

public static class CtagsCacheExtensions
{
    public static void Reset(this CtagsCache tc)
    {
        tc.Clear();
    }

    public static List<int>? Lookup(this CtagsCache tc, List<int> cachePath, bool canAdd)
    {
        var ctag = cachePath[0];
        if (tc.Count <= ctag)
        {
            if (!canAdd)
            {
                return null;
            }

            if (tc.Capacity <= ctag)
            {
                var nc = new List<CtagsCacheEntry>(tc);
                tc.Clear();
                tc.AddRange(nc);
            }

            for (var n = tc.Count; n < ctag + 1; n++)
            {
                tc.Add(new CtagsCacheEntry());
            }
        }

        if (cachePath.Count == 1)
        {
            return tc[ctag].StructIdx;
        }

        return tc[ctag].SubCache.Lookup(cachePath.GetRange(1, cachePath.Count - 1), canAdd);
    }
}

public class CtagsWCacheEntry
{
    public uint CTagName { get; set; }
    public CtagsWCache SubCache { get; set; }
}

public class CtagsWCache : List<CtagsWCacheEntry> { }

public static class CtagsWCacheExtensions
{
    public static void Reset(this CtagsWCache tc)
    {
        tc.Clear();
    }

    public static CtagsWCacheEntry Lookup(this CtagsWCache tc, List<int> idx)
    {
        if (idx.Count == 0)
        {
            return new CtagsWCacheEntry();
        }

        var field = idx[0];

        if (tc.Count <= field)
        {
            if (tc.Capacity <= field)
            {
                var nc = new List<CtagsWCacheEntry>(tc);
                tc.Clear();
                tc.AddRange(nc);
            }

            for (var n = tc.Count; n < field + 1; n++)
            {
                tc.Add(new CtagsWCacheEntry());
            }
        }

        if (idx.Count == 1)
        {
            return tc[field];
        }

        return tc[field].SubCache.Lookup(
            #if NET8_0_OR_GREATER
            idx[1..]
            #else
            idx.Skip(1).ToList()
            #endif
            );
    }
}