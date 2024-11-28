using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static ReindexerNet.Internal.Bindings;

[assembly: InternalsVisibleTo("ReindexerNet.Embedded"), InternalsVisibleTo("ReindexerNet.EmbeddedBenchmarks")]

namespace ReindexerNet.Internal;

//TODO: Will be public when ensure stability.
internal sealed class CJsonWriter : IDisposable
{
    private readonly ArrayPool<byte> _pool;
    private byte[] _buffer;
    private int _pos;
    public CJsonWriter(int bufferSize = 100)
    {
        _pool = ArrayPool<byte>.Shared;
        _buffer = _pool.Rent(bufferSize);
    }

    public ReadOnlySpan<byte> CurrentBuffer => _buffer.AsSpan().Slice(0, _pos);
    public int Position => _pos;

    private void EnsureRemainingSize(int length)
    {
        if (_pos + length > _buffer.Length)
        {
            var oldBufferRef = _buffer;
            _buffer = _pool.Rent(
                Math.Max(
                    _buffer.Length + (_buffer.Length >> 1),  // increase 50%
                    _buffer.Length + length) // if 50% is not enough
                );
            try
            {
                Memory<byte> oldBufferMemory = oldBufferRef.AsMemory(0, oldBufferRef.Length);
                oldBufferMemory.Span.CopyTo(_buffer.AsSpan());
            }
            finally
            {
                _pool.Return(oldBufferRef);
            }
        }
    }

    public void Truncate(int pos)
    {
        _buffer = _buffer[..pos];
    }

    public void TruncateStart(int pos)
    {
        _buffer = _buffer[pos..];
        _pos = 0;//?
    }

    public void PutUInt32(uint v)
    {
        EnsureRemainingSize(sizeof(uint));
        MemoryMarshal.Write(_buffer.AsSpan(_pos), ref v);
        _pos += sizeof(uint);
    }

    public void PutUInt64(ulong v)
    {
        EnsureRemainingSize(sizeof(ulong));
        MemoryMarshal.Write(_buffer.AsSpan(_pos), ref v);
        _pos += sizeof(ulong);
    }

    public void PutDouble(double v)
    {
        EnsureRemainingSize(sizeof(double));
        MemoryMarshal.Write(_buffer.AsSpan(_pos), ref v);
        _pos += sizeof(double);
    }

    public void PutVarInt(long v)
    {
        EnsureRemainingSize(10);
        var len = Sint64_pack(v, _buffer.AsSpan().Slice(_pos));
        _pos += (int)len;
    }

    public void PutVarUInt(ulong v)
    {
        EnsureRemainingSize(10);
        var len = Uint64_pack(v, _buffer.AsSpan().Slice(_pos));
        _pos += (int)len;
    }

    public void PutVarCUInt(int v)
    {
        PutVarUInt((ulong)v);
    }

    public void PutVString(string v)
    {
        if (v == null)
        {
            _buffer[_pos++] = 0;
            return;
        }


#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        Span<byte> strArr = stackalloc byte[v.Length * 2];
        var strByteLength = Encoding.UTF8.GetBytes(v, strArr);
        strArr = strArr[..strByteLength];
#else
        var strArrArr = new byte[v.Length*2];
        var strByteLength = Encoding.UTF8.GetBytes(v, 0, v.Length, strArrArr, 0);
        var strArr = strArrArr.AsSpan(0, strByteLength);
#endif
        EnsureRemainingSize(10 + strByteLength);
        var currentPos = _buffer.AsSpan()[_pos..];
        var lenSize = (int)Uint32_pack((uint)strByteLength, currentPos);
        currentPos = currentPos[lenSize..];
        strArr.CopyTo(currentPos);
        _pos += lenSize + strByteLength;
    }

    public void PutUuid(Guid uuid)
    {
        byte[] bytes = uuid.ToByteArray();
        PutUInt64(BitConverter.ToUInt64(bytes, 0));  // First 8 bytes
        PutUInt64(BitConverter.ToUInt64(bytes, 8));  // Next 8 bytes
    }

    public void PutCTag(CTag cTag)
    {
        PutVarUInt(cTag.Value);
    }

    public void PutCArrayTag(CArrayTag cArrayTag)
    {
        PutUInt32(cArrayTag.Value);
    }

    public void Append(CJsonWriter other)
    {
#if NETSTANDARD2_0 || NET472
        var otherSpan = other._buffer.AsSpan().Slice(0, other._pos);
#else
        var otherSpan = other._buffer.AsSpan()[..other._pos];
#endif
        EnsureRemainingSize(otherSpan.Length);
        otherSpan.CopyTo(_buffer.AsSpan(_pos));
        _pos += otherSpan.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Zigzag64(long v)
    {
        if (v < 0)
            return (ulong)(-v) * 2 - 1;
        else
            return (ulong)v * 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Sint64_pack(long value, Span<byte> @out)
    {
        return Uint64_pack(Zigzag64(value), @out);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Uint32_pack(uint value, Span<byte> @out)
    {
        uint rv = 0;
        if (value >= 128U)
        {
            @out[(int)rv] = (byte)((int)value | 128);
            ++rv;
            value >>= 7;
            if (value >= 128U)
            {
                @out[(int)rv] = (byte)((int)value | 128);
                ++rv;
                value >>= 7;
                if (value >= 128U)
                {
                    @out[(int)rv] = (byte)((int)value | 128);
                    ++rv;
                    value >>= 7;
                    if (value >= 128U)
                    {
                        @out[(int)rv] = (byte)((int)value | 128);
                        ++rv;
                        value >>= 7;
                    }
                }
            }
        }
        @out[(int)rv * 1] = (byte)value;
        return rv + 1U;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Uint64_pack(ulong value, Span<byte> @out)
    {
        uint hi = (uint)(value >> 32);
        uint lo = (uint)value;
        uint rv;
        if (hi == 0U)
        {
            rv = Uint32_pack(lo, @out);
        }
        else
        {
            @out[0] = (byte)((int)lo | 128);
            @out[1] = (byte)((int)(lo >> 7) | 128);
            @out[2] = (byte)((int)(lo >> 14) | 128);
            @out[3] = (byte)((int)(lo >> 21) | 128);
            if (hi < 8U)
            {
                @out[4] = (byte)((int)hi << 4 | (int)(lo >> 28));
                rv = 5U;
            }
            else
            {
                @out[4] = (byte)(((int)hi & 7) << 4 | (int)(lo >> 28) | 128);
                uint num4 = hi >> 3;
                uint num5 = 5;
                for (; num4 >= 128U; num4 >>= 7)
                {
                    @out[(int)num5 * 1] = (byte)((int)num4 | 128);
                    ++num5;
                }
                @out[(int)num5 * 1] = (byte)num4;
                rv = num5 + 1U;
            }
        }
        return rv;
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _pool.Return(_buffer);
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~CJsonBuffer()
    // {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }

    #endregion

}
