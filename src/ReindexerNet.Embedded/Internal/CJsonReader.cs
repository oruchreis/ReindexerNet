using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ReindexerNet.Embedded.Internal
{
    ref struct CJsonReader
    {
        public const int ResultsFormatMask = 0xF;
        public const int ResultsPure = 0x0;
        public const int ResultsPtrs = 0x1;
        public const int ResultsCJson = 0x2;
        public const int ResultsJson = 0x3;

        public const int ResultsWithPayloadTypes = 0x10;
        public const int ResultsWithItemID = 0x20;
        public const int ResultsWithPercents = 0x40;
        public const int ResultsWithNsID = 0x80;
        public const int ResultsWithJoined = 0x100;


        private readonly ReadOnlySpan<byte> _buffer;
        private int _flags { get; set; }
        private int _pos { get; set; }

        public CJsonReader(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
            _flags = 0;
            _pos = 0;
        }

        public RawResultItemParams ReadRawItemParams()
        {
            var v = new RawResultItemParams();
            if ((_flags & ResultsWithItemID) != 0)
            {
                v.id = (int)GetVarUInt();
                v.version = (int)GetVarUInt();
            }

            if ((_flags & ResultsWithNsID) != 0)
            {
                v.nsid = (int)GetVarUInt();
            }

            if ((_flags & ResultsWithPercents) != 0)
            {
                v.proc = (int)GetVarUInt();
            }

            switch (_flags & ResultsFormatMask)
            {
                case ResultsPure:
                case ResultsPtrs:
                    v.cptr = (UIntPtr)GetUInt64();
                    break;
                case ResultsJson:
                case ResultsCJson:
                    v.data = GetBytes();
                    break;
            }
            return v;
        }

        public RawResultQueryParams ReadRawQueryParams(/*Action<int> updatePayloadType = null*/)
        {
            var v = new RawResultQueryParams();
            v.aggResults = new List<byte[]>();
            v.flags = (int)GetVarUInt();
            v.totalcount = (int)GetVarUInt();
            v.qcount = (int)GetVarUInt();
            v.count = (int)GetVarUInt();

            if ((v.flags & ResultsWithPayloadTypes) != 0)
            {
                var ptCount = (int)GetVarUInt();
                for (var i = 0; i < ptCount; i++)
                {
                    var nsid = (int)GetVarUInt();
                    var nsName = GetVString();
                    //if (updatePayloadType == null)
                    //{
                    //    throw new Exception("Internal error: Got payload types from raw query params, but there are no updatePayloadType");
                    //}
                    //updatePayloadType(nsid);

                    ReadPayloadType();
                }
            }
            ReadExtraResults(ref v);
            _flags = v.flags;

            return v;
        }

        public void ReadExtraResults(ref RawResultQueryParams v)
        {
            while (true)
            {
                var tag = (QueryResultTag)GetVarUInt();
                if (tag == QueryResultTag.End)
                {
                    break;
                }

                var data = GetBytes();
                switch (tag)
                {
                    case QueryResultTag.Explain:
                        v.explainResults = data;
                        break;
                    case QueryResultTag.Aggregation:
                        v.aggResults.Add(data.ToArray());
                        break;
                }
            }
        }

        public void ReadPayloadType()
        {
            var stateToken = GetVarUInt();
            var version = GetVarUInt();
                    
         //   skip := state.Version >= version && state.StateToken == stateToken

	        //if !skip {
		       // state.StateData = &StateData{Version: version, StateToken: stateToken}
	        //}


	        //state.tagsMatcher.Read(s, skip)
            var tagsCount = (int) GetVarUInt();
	        //if !skip {
		       // tm.Tags = make([]string, tagsCount, tagsCount)
		       // tm.Names = make(map[string]int)

		       // for i := 0; i < tagsCount; i++ {
			      //  tm.Tags[i] = ser.GetVString()
			      //  tm.Names[tm.Tags[i]] = i
		       // }
	        //} else {
		        for (var i = 0; i < tagsCount; i++) {
			        var tag = GetVString();
		        }
	        //}


	        //state.payloadType.Read(s, skip)
            var pStringHdrOffset = (UIntPtr) GetVarUInt();
	        var fieldsCount = (int)GetVarUInt();
	        //fields := make([]payloadFieldType, fieldsCount, fieldsCount)
            var fields = new PayloadFieldType[fieldsCount];
	        for (var i = 0; i < fieldsCount; i++) {
                var payloadFieldType = new PayloadFieldType();
		        payloadFieldType.Type = (int)GetVarUInt();
		        payloadFieldType.Name = GetVString();
		        payloadFieldType.Offset = (UIntPtr)GetVarUInt();
		        payloadFieldType.Size = (UIntPtr)GetVarUInt();
		        payloadFieldType.IsArray = GetVarUInt() != 0;

                fields[i] = payloadFieldType;
		        var jsonPathCnt = GetVarUInt();
		        for (; jsonPathCnt != 0; jsonPathCnt--) {
			        GetVString();
		        }
	        }
	        //if !skip {
		       // pt.Fields = fields
	        //}
        }

        private
#if !NETSTANDARD2_0 && !NET472
            readonly
#else
            static
#endif
            void checkbound(int pos, int need, int len)
        {
            if (pos + need > len)
            {
                throw new InternalBufferOverflowException($"Binary buffer underflow. Need more {need} bytes, pos={pos},len={len}");
            }
        }

        private
#if !NETSTANDARD2_0 && !NET472
            readonly
#else
            static
#endif
            int scan_varint(int len, ReadOnlySpan<byte> data)
        {
            int i;
            if (len > 10) len = 10;
            for (i = 0; i < len; i++)
                if ((data[i] & 0x80) == 0) break;
            if (i == len) return 0;
            return i + 1;
        }

        private
#if !NETSTANDARD2_0 && !NET472
            readonly
#else
            static
#endif
            uint parse_uint32(uint len, ReadOnlySpan<byte> data)
        {
            uint rv = data[0] & (uint)sbyte.MaxValue;
            if (len > 1U)
            {
                rv |= (uint)((data[1] & sbyte.MaxValue) << 7);
                if (len > 2U)
                {
                    rv |= (uint)((data[2] & sbyte.MaxValue) << 14);
                    if (len > 3U)
                    {
                        rv |= (uint)((data[3] & sbyte.MaxValue) << 21);
                        if (len > 4U)
                            rv |= (uint)data[4] << 28;
                    }
                }
            }
            return rv;
        }

        private
#if !NETSTANDARD2_0 && !NET472
            readonly
#else
            static
#endif
            ulong parse_uint64(uint len, ReadOnlySpan<byte> data)
        {
            ulong rv;
            if (len < 5U)
            {
                rv = parse_uint32(len, data);
            }
            else
            {
                ulong num2 = (ulong)(data[0] & sbyte.MaxValue | (long)(data[1] & sbyte.MaxValue) << 7 | (long)(data[2] & sbyte.MaxValue) << 14 | (long)(data[3] & sbyte.MaxValue) << 21);
                uint shift = 28;
                for (uint index = 4; index < len; ++index)
                {
                    num2 |= (ulong)(data[(int)index] & sbyte.MaxValue) << (int)shift;
                    shift += 7U;
                }
                rv = num2;
            }
            return rv;
        }

        public ulong GetVarUInt()
        {
            var l = scan_varint(_buffer.Length - _pos, _buffer.Slice(_pos));
            if (l == 0)
            {
                throw new InvalidDataException($"Binary buffer broken - scan_varint failed: pos={_pos},len={_buffer.Length}");
            }
            checkbound(_pos, l, _buffer.Length);
            _pos += l;
            return parse_uint64((uint)l, _buffer.Slice(_pos - l));
        }

        private long ReadIntBits(int size)
        {
            long v = 0;
            for (var i = size - 1; i >= 0; i--)
            {
                v = ((long)(_buffer[i + _pos]) & 0xFF) | (v << 8);
            }
            _pos += size;
            return v;
        }

        private ulong ReadUIntBits(int size)
        {
            ulong v = 0;
            for (var i = size - 1; i >= 0; i--)
            {
                v = ((ulong)(_buffer[i + _pos]) & 0xFF) | (v << 8);
            }
            _pos += size;
            return v;
        }

        private uint GetUInt32()
        {
            uint ret;
            checkbound(_pos, sizeof(uint), _buffer.Length);
            ret = (uint)ReadIntBits(sizeof(uint));
            return ret;
        }

        private ulong GetUInt64()
        {
            ulong ret;
            checkbound(_pos, sizeof(ulong), _buffer.Length);
            ret = ReadUIntBits(sizeof(ulong));
            return ret;
        }

        private ReadOnlySpan<byte> GetBytes()
        {
            var l = (int)GetUInt32();
            if (_pos + l > _buffer.Length)
            {
                throw new InternalBufferOverflowException($"Internal error: serializer need {l} bytes, but only {_buffer.Length - _pos} available");
            }

            var v = _buffer.Slice(_pos, l);
            _pos += l;
            return v;
        }

        private string GetVString()
        {
            var l = (int)GetVarUInt();
            if (_pos + l > _buffer.Length)
            {
                throw new InternalBufferOverflowException($"Internal error: serializer need {l} bytes, but only {_buffer.Length - _pos} available");
            }

            var v = Encoding.UTF8.GetString(_buffer.Slice(_pos, l)
#if NETSTANDARD2_0 || NET472
                .ToArray()
#endif
                );
            _pos += l;
            return v;
        }
    }

    internal ref struct RawResultItemParams
    {
        public int id;
        public int version;
        public int nsid;
        public int proc;
        public UIntPtr cptr;
        public ReadOnlySpan<byte> data;
    }

    internal class RawResultsExtraParam
    {
        public int Tag;
        public string Name;
        public byte[] Data;
    }

    internal ref struct RawResultQueryParams
    {
        public int flags;
        public int totalcount;
        public int qcount;
        public int count;
        public List<byte[]> aggResults;
        public ReadOnlySpan<byte> explainResults;
    }

    internal class PayloadFieldType
    {
        internal int Type;
        internal string Name;
        internal UIntPtr Offset;
        internal UIntPtr Size;
        internal bool IsArray;
    }
}
