using ReindexerNet.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static ReindexerNet.Internal.CTag;

namespace ReindexerNet;

public class CJsonSerializer : IReindexerSerializer
{
    private const uint TAG_VARINT = 0;
    private const uint TAG_DOUBLE = 1;
    private const uint TAG_STRING = 2;
    private const uint TAG_BOOL = 3;
    private const uint TAG_NULL = 4;
    private const uint TAG_ARRAY = 5;
    private const uint TAG_OBJECT = 6;
    private const uint TAG_END = 7;
    private const uint TAG_UUID = 8;

    public SerializerType Type => SerializerType.Cjson;

    public T Deserialize<T>(ReadOnlySpan<byte> bytes)
    {
        throw new NotImplementedException();
    }

    public ReadOnlySpan<byte> Serialize<T>(T item)
    {
        throw new NotImplementedException();
    }

    private class State
    {
        public readonly ReaderWriterLockSlim Lock = new();
        public int StateToken { get; set; }
        public TagsMatcher TagsMatcher { get; set; }
        public PayloadType PayloadType { get; set; }
        public CtagsWCache CtagsWCache { get; set; }
    }

    private class TagsMatcher
    {
        public List<string> Tags { get; set; }
        public Dictionary<string, uint> Names { get; set; }

        public void Read(CJsonReader reader, bool skip)
        {
            int tagsCount = (int)reader.GetVarUInt();

            if (!skip)
            {
                Tags = new List<string>(tagsCount);
                Names = new(tagsCount);

                for (uint i = 0; i < tagsCount; i++)
                {
                    string tag = reader.GetVString();
                    Tags.Add(tag);
                    Names[tag] = i;
                }
            }
            else
            {
                for (int i = 0; i < tagsCount; i++)
                {
                    reader.GetVString();
                }
            }
        }

        public void WriteUpdated(CJsonWriter _writer)
        {
            _writer.PutVarUInt((ulong)Tags.Count);

            foreach (string tag in Tags)
            {
                _writer.PutVString(tag);
            }
        }

        public string TagToName(int tag)
        {
            tag = tag & ((1 << 12) - 1);

            if (tag == 0)
            {
                return "";
            }

            if (tag - 1 >= Tags.Count)
            {
                throw new InvalidOperationException($"Internal error - unknown tag {tag}\nKnown tags: {string.Join(", ", Names.Keys)}");
            }

            return Tags[tag - 1];
        }

        public uint NameToTag(string name, bool canAdd)
        {
            if (Names.TryGetValue(name, out uint tag))
            {
                return tag + 1;
            }

            if (canAdd)
            {
                Names ??= [];

                tag = (uint)Tags.Count;
                Names[name] = tag;
                Tags.Add(name);

                return tag + 1;
            }

            return 0;
        }
    }

    private class Encoder
    {
        private readonly CJsonWriter _writer = new();
        private readonly State _state = new();
        private bool _tmUpdated;
        private TagsMatcher _tagsMatcher = new();

        private Dictionary<Type, (PropertyInfo[] Properties, Dictionary<Type, Attribute> Attributes)> _typeDetails = new();
        private Dictionary<PropertyInfo, Dictionary<Type, Attribute>> _propertyAttributes = new();
        private static readonly HashSet<Type> _notJsonObjectReferenceTypes =
            [
                typeof(string),
                typeof(IEnumerable),
                typeof(IDictionary<,>)
            ];

        public (int stateToken, Exception? error) Encode<T>(T obj)
        {
            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            if (!type.IsPrimitive &&
                !_typeDetails.ContainsKey(type) &&
                !_notJsonObjectReferenceTypes.Contains(type) &&
                _notJsonObjectReferenceTypes.All(t => !t.IsAssignableFrom(type)))
            {
                _typeDetails[type] = (
                    Properties: type.GetProperties(BindingFlags.Public | BindingFlags.Instance),
                    Attributes: type.GetCustomAttributes().ToDictionary(a => a.GetType(), a => a)
                    );
            }
            _state.Lock.EnterWriteLock();
            try
            {
                var pos = _writer.Position;
                _writer.PutVarUInt(TAG_END);
                _writer.PutUInt32(0);
                _tmUpdated = false;
                try
                {
                    EncodeValue(ref obj, type, null, 0, [0, 10]);
                }
                catch (Exception e)
                {
                    return (0, e);
                }

                if (_tmUpdated)
                {
                    _writer.PutUInt32((uint)(_writer.Position - pos));
                    _tagsMatcher.WriteUpdated(_writer);
                }
                else
                {
                    _writer.TruncateStart(sizeof(uint) + 1);
                }

                return (_state.StateToken, null);
            }
            finally
            {
                _state.Lock.ExitWriteLock();
            }
        }

        public void EncodeValue<T>(ref T obj, Type type, PropertyInfo propertyInfo, uint ctagName, List<int> idx)
        {
            _propertyAttributes.TryGetValue(propertyInfo, out var propertyAttributes);
            var ignoreWhenDefault = propertyInfo != null &&
                    propertyAttributes.TryGetValue(typeof(JsonIgnoreAttribute), out Attribute attr) &&
                    attr is JsonIgnoreAttribute jsonIgnoreAttr &&
                    (jsonIgnoreAttr.Condition == JsonIgnoreCondition.WhenWritingNull || jsonIgnoreAttr.Condition == JsonIgnoreCondition.WhenWritingDefault);
            if (obj == null)
            {
                if (!ignoreWhenDefault)
                {
                    _writer.PutCTag(CTag.Create(TAG_NULL, ctagName, 0));
                }
                return;
            }

            if (type == typeof(int) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(long))
            {
                var val = (long)(object)obj;
                if (val != 0 || !ignoreWhenDefault)
                {
                    _writer.PutCTag(CTag.Create(TAG_VARINT, ctagName, 0));
                    _writer.PutVarInt(val);
                }
            }
            else if (
                type == typeof(uint) ||
                type == typeof(ushort) ||
                type == typeof(ulong) ||
                type == typeof(uint))
            {
                var val = (ulong)(object)obj;
                if (val != 0 || !ignoreWhenDefault)
                {
                    _writer.PutCTag(CTag.Create(TAG_VARINT, ctagName, 0));
                    _writer.PutVarInt((long)val);
                }
            }
            else if (
                type == typeof(float) ||
                type == typeof(double))
            {
                var val = (double)(object)obj;
                if (val != 0 || !ignoreWhenDefault)
                {
                    _writer.PutCTag(CTag.Create(TAG_DOUBLE, ctagName, 0));
                    _writer.PutDouble(val);
                }
            }
            else if (
                type == typeof(decimal))
            {
                var val = decimal.ToDouble((decimal)(object)obj);
                if (val != 0 || !ignoreWhenDefault)
                {
                    _writer.PutCTag(CTag.Create(TAG_DOUBLE, ctagName, 0));
                    _writer.PutDouble(val);
                }
            }
            else if (
                type == typeof(bool))
            {
                int vv = (bool)(object)obj ? 1 : 0;
                if (vv != 0 || !ignoreWhenDefault)
                {
                    _writer.PutCTag(CTag.Create(TAG_BOOL, ctagName, 0));
                    _writer.PutVarUInt((uint)vv);
                }
            }
            else if (
                type == typeof(Guid))
            {
                var val = (Guid)(object)obj;
                _writer.PutCTag(CTag.Create(TAG_UUID, ctagName, 0));
                _writer.PutUuid(val);
            }
            else if (
                type == typeof(string) ||
                type.IsValueType)
            {
                var valString = (string)(object)obj;
                if (valString.Length != 0 || !ignoreWhenDefault)
                {
                    _writer.PutCTag(CTag.Create(TAG_STRING, ctagName, 0));
                    _writer.PutVString(valString);
                }
            }
            else if (obj is DateTime dateTime)
            {
                _writer.PutCTag(CTag.Create(TAG_STRING, ctagName, 0));
                _writer.PutVString(dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"));
            }
            else if (obj is DateOnly dateOnly)
            {
                _writer.PutCTag(CTag.Create(TAG_STRING, ctagName, 0));
                _writer.PutVString(dateOnly.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"));
            }
            else if (obj is DateTimeOffset dateTimeOffset)
            {
                _writer.PutCTag(CTag.Create(TAG_STRING, ctagName, 0));
                _writer.PutVString(dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"));
            }
            else if (obj is IDictionary map)
            {
                _writer.PutCTag(CTag.Create(TAG_OBJECT, ctagName, 0));
                EncodeMap(map, idx);
                _writer.PutCTag(CTag.Create(TAG_END, 0, 0));
            }
            else if (type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)) is Type enumType)
            {
                var itemType = enumType.GetGenericArguments().First();
                if (_encodeSliceMethods.TryGetValue(itemType, out var encodeSliceMethod))
                    encodeSliceMethod.Invoke(this, [obj, 0, ignoreWhenDefault, idx]);
                else
                {
                    var enumerable = (IEnumerable<object>)(object)obj;
                    EncodeSlice(ref enumerable, 0, ignoreWhenDefault, idx);
                }
            }
            else
            {
                _writer.PutCTag(CTag.Create(TAG_OBJECT, ctagName, 0));
                EncodeStruct(obj, idx);
                _writer.PutCTag(CTag.Create(TAG_END, 0, 0));
            }
        }

        private uint Name2Tag(string name)
        {
            uint tagName = _tagsMatcher.NameToTag(name, false);

            if (tagName == 0)
            {
                if (!_tmUpdated)
                {
                    _tagsMatcher = new TagsMatcher
                    {
                        Tags = _state.TagsMatcher.Tags,
                        Names = new(_state.TagsMatcher.Names)
                    };

                    _tmUpdated = true;
                }

                tagName = _tagsMatcher.NameToTag(name, true);
            }

            return tagName;
        }

        public void EncodeMap<T>(T map, List<int> idx)
            where T : IDictionary
        {
            var keys = map.Keys;

            foreach (var key in keys)
            {
                object value = map[key];
                var keyNameStr = key.ToString();
                var ctagName = Name2Tag(keyNameStr);
                EncodeValue(ref value, value?.GetType(), null, ctagName, idx);
            }
        }

        private static readonly Dictionary<Type, MethodInfo> _encodeSliceMethods = new Type[]{
            typeof(byte),typeof(int),typeof(sbyte),typeof(short),typeof(long),
            typeof(uint),typeof(ushort),typeof(ulong),typeof(uint),
            typeof(float),typeof(double),typeof(decimal),typeof(Guid),
            typeof(string),typeof(DateTime),typeof(DateOnly),typeof(DateTimeOffset)
        }.ToDictionary(itemType => itemType, itemType => typeof(Encoder).GetMethod(nameof(EncodeSlice)).MakeGenericMethod(itemType));

        public void EncodeSlice<TItem>(ref IEnumerable<TItem> enumerable, uint ctagName, bool ignoreWhenDefault, int[] idx)
        {
            ReadOnlySpan<TItem> array = enumerable switch
            {
                TItem[] arr => arr,
                List<TItem> list => CollectionsMarshal.AsSpan(list),
                ReadOnlyMemory<TItem> mem => mem.Span,
                _ => enumerable.ToArray()
            };
            int length = array.Length;
            if (length == 0 && ignoreWhenDefault)
            {
                return;
            }

            var itemType = typeof(TItem);
            if (itemType == typeof(byte))
            {
                _writer.PutCTag(CTag.Create(TAG_STRING, ctagName, 0));
                _writer.PutVString(Convert.ToBase64String(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TItem, byte>(ref MemoryMarshal.GetReference(array)), length)));
            }
            else
            {
                _writer.PutCTag(CTag.Create(TAG_ARRAY, ctagName, 0));

                var subTag = default(TItem) switch
                {
                    int or
                    short or
                    long or
                    sbyte or
                    ushort or
                    ulong or
                    uint or
                    nint => TAG_VARINT,
                    float or double or decimal => TAG_DOUBLE,
                    Guid => TAG_STRING,
                    string or DateTime or DateOnly or DateTimeOffset => TAG_STRING,
                    bool => TAG_BOOL,
                    _ when typeof(TItem).IsValueType => TAG_STRING,
                    _ => TAG_OBJECT
                };

                _writer.PutCArrayTag(CArrayTag.Create((uint)length, subTag));

                if (itemType == typeof(int) ||
                    itemType == typeof(sbyte) ||
                    itemType == typeof(short) ||
                    itemType == typeof(long) ||
                    itemType == typeof(uint) ||
                    itemType == typeof(ushort) ||
                    itemType == typeof(ulong) ||
                    itemType == typeof(uint))
                {
                    for (var i = 0; i < length; i++)
                    {
                        _writer.PutVarInt(Convert.ToInt64(array[i]));
                    }
                }
                else if (
                    itemType == typeof(float) ||
                    itemType == typeof(double) ||
                    itemType == typeof(decimal)
                    )
                {
                    for (var i = 0; i < length; i++)
                    {
                        _writer.PutDouble(Convert.ToDouble(array[i]));
                    }
                }
                else if (
                    itemType == typeof(Guid)
                    )
                {
                    for (var i = 0; i < length; i++)
                    {
                        _writer.PutUuid((Guid)(object)array[i]);
                    }
                }
                else if (
                    itemType == typeof(string) ||
                    itemType == typeof(DateTime) ||
                    itemType == typeof(DateOnly) ||
                    itemType == typeof(DateTimeOffset) ||
                    itemType.IsValueType
                    )
                {
                    for (var i = 0; i < length; i++)
                    {
                        _writer.PutVString(Convert.ToString(array[i]));
                    }
                }
                else if (itemType == typeof(bool))
                {
                    for (var i = 0; i < length; i++)
                    {
                        _writer.PutVarUInt((ulong)((bool)(object)array[i] ? 1 : 0));
                    }
                }
                else
                {
                    if (subTag != TAG_OBJECT)
                    {
                        throw new InvalidOperationException($"Internal error can't serialize array of type {itemType.FullName}");
                    }

                    for (int i = 0; i < length; i++)
                    {
                        var element = array[i];
                        EncodeValue(ref element, itemType, null, 0, idx);
                    }
                }
            }
        }

        public void EncodeStruct<T>(T obj, List<int> idx)
        {
            var type = typeof(T);
            if (!_typeDetails.TryGetValue(type, out var typeDetail))
            {
                _typeDetails[type] = typeDetail = (
                    Properties: type.GetProperties(BindingFlags.Public | BindingFlags.Instance),
                    Attributes: type.GetCustomAttributes().ToDictionary(a => a.GetType()));
            }

            for (var propertyIndex = 0; propertyIndex < typeDetail.Properties.Length; propertyIndex++)
            {
                PropertyInfo property = typeDetail.Properties[propertyIndex];
                if (!_propertyAttributes.TryGetValue(property, out var propertyAttributes))
                {
                    _propertyAttributes[property] = propertyAttributes = type.GetCustomAttributes().ToDictionary(a => a.GetType());
                }

                if (propertyAttributes.TryGetValue(typeof(JsonIgnoreAttribute), out Attribute attr) &&
                    attr is JsonIgnoreAttribute jsonIgnoreAttr &&
                    (jsonIgnoreAttr.Condition == JsonIgnoreCondition.Always))
                    continue;

                List<int> iidx = idx;
                CtagsWCacheEntry ce;

                if (iidx == null)
                {
                    ce = new CtagsWCacheEntry();
                }
                else
                {
                    iidx.Add(propertyIndex);
                    ce = _state.CtagsWCache.Lookup(iidx);
                }

                uint ctagName = ce.CTagName;
                if (ctagName == 0)
                {
                    var name = propertyAttributes.TryGetValue(typeof(JsonPropertyNameAttribute), out attr) &&
                        attr is JsonPropertyNameAttribute jsonPropNameAttr ? jsonPropNameAttr.Name : property.Name;
                    ctagName = Name2Tag(name);
                    if (_tmUpdated)
                    {
                        ce = new CtagsWCacheEntry
                        {
                            CTagName = ctagName
                        };
                    }
                }

                var propertyValue = property.GetValue(obj);
                EncodeValue(ref propertyValue, property.PropertyType, property, ctagName, iidx);
            }
        }
    }

    private class Decoder
    {
        // ... Existing members ...

        private const int MaxIndexes = 256;

        public static (FieldInfo, bool) FieldByTag(Type t, string tag)
        {
            if (t.IsPointer)
            {
                t = t.GetElementType();
            }

            for (int i = 0; i < t.GetFields().Length; i++)
            {
                FieldInfo result = t.GetFields()[i];
                if (result.GetCustomAttribute<JsonFieldAttribute>() is { } ftag && !string.IsNullOrEmpty(ftag.Name))
                {
                    if (tag == ftag.Name)
                    {
                        return (result, true);
                    }
                }
                else if (result.IsDefined(typeof(JsonFieldAttribute), false))
                {
                    if (result.GetValue(null) is { } value && value is Type fieldType && fieldType.IsClass)
                    {
                        if (FieldByTag(fieldType, tag) is (FieldInfo subResult, true))
                        {
                            result.SetValue(value, new object[] { i }.Concat(subResult.FieldHandle.GetFieldOffset()).ToArray());
                            return (result, true);
                        }
                    }
                }
                else if (result.Name == tag)
                {
                    return (result, true);
                }
            }

            return (null, false);
        }

        public static void CreateEmbedByIdx(ref object v, int[] idx)
        {
            var vRef = v as object[];
            vRef = vRef[idx[0]] as object[];
            v = vRef[0];

            if (v.GetType().IsClass && v == null)
            {
                v = Activator.CreateInstance(v.GetType());
            }

            if (idx.Length > 2)
            {
                CreateEmbedByIdx(ref v, idx[1..]);
            }
        }

        public bool SkipStruct(ref PayloadIface pl, ref Serializer rdser, ref int[] fieldsoutcnt, Ctag tag)
        {
            var ctagType = tag.Type;

            if (ctagType == CtagType.End)
            {
                return false;
            }

            var ctagField = tag.Field;

            if (ctagField >= 0)
            {
                var cnt = ref fieldsoutcnt[ctagField];
                switch (ctagType)
                {
                    case CtagType.Array:
                        var count = (int)rdser.GetVarUInt();
                        cnt += count;
                        break;
                    default:
                        cnt++;
                        break;
                }
            }
            else
            {
                switch (ctagType)
                {
                    case CtagType.Object:
                        while (SkipStruct(ref pl, ref rdser, ref fieldsoutcnt, rdser.GetCTag())) { }
                        break;
                    case CtagType.Array:
                        var atag = rdser.GetCArrayTag();
                        var count = atag.Count;
                        var subtag = atag.Tag;

                        for (int i = 0; i < count; i++)
                        {
                            switch (subtag)
                            {
                                case CtagType.Object:
                                    SkipStruct(ref pl, ref rdser, ref fieldsoutcnt, rdser.GetCTag());
                                    break;
                                default:
                                    rdser.SkipTag(subtag);
                                    break;
                            }
                        }
                        break;
                    default:
                        rdser.SkipTag(ctagType);
                        break;
                }
            }

            return true;
        }

        public void SkipTag(ref Serializer rdser, int tagType)
        {
            switch (tagType)
            {
                case TagType.Double:
                    rdser.GetDouble();
                    break;
                case TagType.VarInt:
                    rdser.GetVarInt();
                    break;
                case TagType.Bool:
                    rdser.GetVarUInt();
                    break;
                case TagType.Null:
                    break;
                case TagType.String:
                    rdser.GetVString();
                    break;
                case TagType.UUID:
                    rdser.GetUuid();
                    break;
                default:
                    throw new InvalidOperationException($"Can't skip tagType {TagTypeName(tagType)}");
            }
        }

        public long AsInt(ref Serializer rdser, int tagType)
        {
            switch (tagType)
            {
                case TagType.Bool:
                    return rdser.GetVarUInt();
                case TagType.VarInt:
                    return rdser.GetVarInt();
                case TagType.Double:
                    return (long)rdser.GetDouble();
                default:
                    throw new InvalidOperationException($"Can't convert tagType {TagTypeName(tagType)} to int");
            }
        }

        public double AsFloat(ref Serializer rdser, int tagType)
        {
            switch (tagType)
            {
                case TagType.VarInt:
                    return rdser.GetVarInt();
                case TagType.Double:
                    return rdser.GetDouble();
                default:
                    throw new InvalidOperationException($"Can't convert tagType {TagTypeName(tagType)} to float");
            }
        }

        public string AsString(ref Serializer rdser, int tagType)
        {
            switch (tagType)
            {
                case TagType.String:
                    return rdser.GetVString();
                case TagType.UUID:
                    return rdser.GetUuid();
                default:
                    throw new InvalidOperationException($"Can't convert tagType {TagTypeName(tagType)} to string");
            }
        }

        private const int MaxInt = int.MaxValue;
        private const int MinInt = int.MinValue;

        public object AsIface(ref Serializer rdser, int tagType)
        {
            switch (tagType)
            {
                case TagType.VarInt:
                    long v = rdser.GetVarInt();
                    return (v < MinInt || v > MaxInt) ? v : (int)v;
                case TagType.Double:
                    return rdser.GetDouble();
                case TagType.Bool:
                    return rdser.GetVarInt() != 0;
                case TagType.String:
                    return rdser.GetVString();
                case TagType.Null:
                    return null;
                case TagType.UUID:
                    return rdser.GetUuid();
                default:
                    throw new InvalidOperationException($"Can't convert tagType {TagTypeName(tagType)} to iface");
            }
        }

        public void MkSlice<T>(ref T[] v, int count)
        {
            switch (v)
            {
                case ref List<string> a:
                    a = new List<string>(count);
                    break;
                case ref List<int> a:
                    a = new List<int>(count);
                    break;
                case ref List<long> a:
                    a = new List<long>(count);
                    break;
                case ref List<int> a:
                    a = new List<int>(count);
                    break;
                case ref List < int16 > a:
                    a = new List<int16>(count);
                    break;
                case ref List < int8 > a:
                    a = new List<int8>(count);
                    break;
                case ref List<uint> a:
                    a = new List<uint>(count);
                    break;
                case ref List < uint64 > a:
                    a = new List<uint64>(count);
                    break;
                case ref List < uint32 > a:
                    a = new List<uint32>(count);
                    break;
                case ref List < uint16 > a:
                    a = new List<uint16>(count);
                    break;
                case ref List < uint8 > a:
                    a = new List<uint8>(count);
                    break;
                case ref List<double> a:
                    a = new List<double>(count);
                    break;
                case ref List<float> a:
                    a = new List<float>(count);
                    break;
                case ref List<bool> a:
                    a = new List<bool>(count);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported type: {v.GetType().Name}");
            }
        }

        public object MkValue(int ctagType)
        {
            switch (ctagType)
            {
                case TagType.String:
                case TagType.UUID:
                    return "";
                case TagType.VarInt:
                    return 0;
                case TagType.Double:
                    return 0.0;
                case TagType.Bool:
                    return false;
                case TagType.Object:
                    return new Dictionary<string, object>();
                case TagType.Array:
                    return new List<object>();
                default:
                    throw new InvalidOperationException($"Invalid ctagType={ctagType}");
            }
        }

        public void DecodeSlice(ref PayloadIface pl, ref Serializer rdser, ref object v, int[] fieldsoutcnt, int[] cctagsPath)
        {
            CArrayTag atag = rdser.GetCArrayTag();
            int count = atag.Count();
            Ctag subtag = atag.Tag();

            var origV = v as Array;
            IntPtr ptr = IntPtr.Zero;

            Type elementType;
            bool isPtr = false;

            switch (v)
            {
                case List<string> a:
                    elementType = typeof(string);
                    break;
                case List<int> a:
                    elementType = typeof(int);
                    break;
                case List<long> a:
                    elementType = typeof(long);
                    break;
                // ... Add cases for other types ...

                default:
                    throw new InvalidOperationException($"Unsupported type: {v.GetType().Name}");
            }

            var k = elementType.IsArray ? elementType.GetElementType().GetElementType().Kind : elementType.GetElementType().Kind;

            switch (k)
            {
                case TypeCode.Int32:
                    var slInt = v as int[];
                    for (int i = 0; i < count; i++)
                    {
                        slInt[i] = (int)AsInt(ref rdser, subtag);
                    }
                    break;
                case TypeCode.UInt32:
                    var slUInt = v as uint[];
                    for (int i = 0; i < count; i++)
                    {
                        slUInt[i] = (uint)AsInt(ref rdser, subtag);
                    }
                    break;
                case TypeCode.Int64:
                    var slInt64 = v as long[];
                    for (int i = 0; i < count; i++)
                    {
                        slInt64[i] = AsInt(ref rdser, subtag);
                    }
                    break;
                // ... Add cases for other types ...

                case TypeCode.Object:
                    if (k == TypeCode.Object && v.GetType().GetElementType().IsClass)
                    {
                        isPtr = true;
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Can't set array to {k}");
            }

            if (subtag != CtagType.Object)
            {
                for (int i = 0; i < count; i++)
                {
                    var currentValue = v as Array;
                    if (currentValue == null)
                    {
                        currentValue = (Array)Activator.CreateInstance(v.GetType(), count);
                    }

                    if (isPtr)
                    {
                        var method = typeof(Decoder).GetMethod(nameof(AsIface), BindingFlags.NonPublic | BindingFlags.Instance);
                        var genericMethod = method.MakeGenericMethod(elementType.GetElementType());
                        var value = genericMethod.Invoke(this, new object[] { rdser, subtag });
                        currentValue.SetValue(value, i);
                    }
                    else
                    {
                        var method = typeof(Decoder).GetMethod($"As{elementType.GetElementType().Name}", BindingFlags.NonPublic | BindingFlags.Instance);
                        var value = method.Invoke(this, new object[] { rdser, subtag });
                        currentValue.SetValue(value, i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    DecodeValue(ref pl, ref rdser, ref v, fieldsoutcnt, cctagsPath);
                }
            }

            if (k == TypeCode.Object)
            {
                origV.SetValue(v, 0);
                v = origV;
            }
        }

        public bool DecodeValue<T>(PayloadIface pl, ref CJsonReader rdser, ref T v, int[] fieldsoutcnt, List<int> cctagsPath)
        {
            var ctag = rdser.GetCTag();
            var ctagType = ctag.Type;


            if (ctagType == TagType.End)
                return false;
            if (ctagType == TagType.Null)
                return true;            

            int ctagField = ctag.Field;
            int ctagName = ctag.Name;

            var valueType = typeof(T);
            
            List<int> idx = null;

            dynamic mv = v;
            bool isMap = false;

            if (ctagName != 0)
            {
                cctagsPath.Add(ctagName);

                if (valueType == typeof(object) || (valueType == typeof(Dictionary<string, object>) && valueType.GenericTypeArguments[1] == typeof(object)))
                {
                    if (v == null)
                    {
                        v = (T)(object)new Dictionary<string, object>();
                    }

                    mv = v;
                    v = (T)mkValue(ctagType);
                    isMap = true;
                }
                else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>) && valueType.GenericTypeArguments[1] == typeof(object))
                {
                    if (v == null)
                    {
                        v = (T)Activator.CreateInstance(valueType);
                    }

                    mv = v;
                    v = (T)Activator.CreateInstance(valueType.GenericTypeArguments[0]);
                    isMap = true;
                }
                else if (valueType == typeof(object))
                {
                    if (v == null)
                    {
                        v = (T)(object)new Dictionary<string, object>();
                    }

                    mv = v;
                    v = (T)mkValue(ctagType);
                    isMap = true;
                }
            }

            if (ctagField >= 0)
            {
                int cnt = fieldsoutcnt[ctagField];

                switch (ctagType)
                {
                    case CTagType.TAG_ARRAY:
                        int count = (int)rdser.GetVarUInt();
                        pl.getArray(ctagField, cnt, count, v);
                        cnt += count;
                        break;
                    default:
                        pl.getValue(ctagField, cnt, v);
                        cnt++;
                        break;
                }
            }
            else
            {
                switch (ctagType)
                {
                    case CTagType.TAG_ARRAY:
                        decodeSlice(pl, rdser, ref v, fieldsoutcnt, cctagsPath);
                        break;
                    case CTagType.TAG_OBJECT:
                        while (DecodeValue(pl, rdser, ref v, fieldsoutcnt, cctagsPath))
                        {
                        }

                        break;
                    case CTagType.TAG_STRING:
                    case CTagType.TAG_UUID:
                        string str = (ctagType == CTagType.TAG_UUID) ? rdser.GetUuid() : rdser.GetVString();

                        switch (typeCode)
                        {
                            case TypeCode.String:
                                v = (T)(object)str;
                                break;
                            case TypeCode.Object:
                                byte[] b = Convert.FromBase64String(str);

                                if (valueType == typeof(byte[]))
                                {
                                    v = (T)(object)b;
                                }
                                else
                                {
                                    throw new NotSupportedException($"Unsupported type: {valueType.Name}");
                                }

                                break;
                            case TypeCode.DateTime:
                                DateTime tm = DateTime.ParseExact(str, "yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                                v = (T)(object)tm;
                                break;
                            default:
                                throw new NotSupportedException($"Unsupported type: {valueType.Name}");
                        }

                        break;
                    default:
                        switch (typeCode)
                        {
                            case TypeCode.Single:
                            case TypeCode.Double:
                                v = (T)(object)asFloat(rdser, ctagType);
                                break;
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                                v = (T)(object)asInt(rdser, ctagType);
                                break;
                            case TypeCode.UInt32:
                                v = (T)(object)(uint)asInt(rdser, ctagType);
                                break;
                            case TypeCode.Boolean:
                                v = (T)(object)(asInt(rdser, ctagType) != 0);
                                break;
                            case TypeCode.Object:
                                v = (T)asIface(rdser, ctagType);
                                break;
                            default:
                                throw new NotSupportedException($"Unsupported type: {valueType.Name}");
                        }

                        break;
                }
            }

            if (isMap)
            {
                if (valueType == typeof(object))
                {
                    valueType = v.GetType();
                }

                if (mv.GetType().GenericTypeArguments[1].IsPointer)
                {
                    dynamic mvValue = v;

                    if (mv.GetType().GenericTypeArguments[1].IsGenericType && mv.GetType().GenericTypeArguments[1].GetGenericTypeDefinition() == typeof(Dictionary<,>) && mv.GetType().GenericTypeArguments[1].GenericTypeArguments[1].IsPointer)
                    {
                        mvValue = Activator.CreateInstance(mv.GetType().GenericTypeArguments[1]);
                        dynamic key = cctagsPath[^1];
                        mvValue.Add(key, v);
                    }
                    else
                    {
                        dynamic key = cctagsPath[^1];
                        mvValue.Add(key, v);
                    }
                }
                else
                {
                    dynamic key = cctagsPath[^1];
                    mv.SetItem(key, v);
                }
            }

            return true;
        }

        public T Decode<T>(ReadOnlySpan<byte> cjson)
        {
            var reader = new CJsonReader(cjson);
            int[] fieldsoutcnt = new int[MaxIndexes];
            List<int> ctagsPath = new List<int>(8);
            var obj = Activator.CreateInstance<T>();
            DecodeValue<T>(null, ref reader, ref obj,fieldsoutcnt, ctagsPath);
            return obj;
        }
    }
}