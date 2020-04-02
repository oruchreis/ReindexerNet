using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ReindexerNet.Embedded.Internal
{
    internal class CJsonWriter : IDisposable
    {
        private readonly ArrayPool<byte> _pool;
        private byte[] _buffer;
        private int _pos;
        public CJsonWriter()
        {
            _pool = ArrayPool<byte>.Shared;
            _buffer = _pool.Rent(100);
        }

        public ReadOnlySpan<byte> CurrentBuffer => _buffer.AsSpan().Slice(0, _pos);

        private void EnsureRemainingSize(int length)
        {
            if (_pos + length > _buffer.Length)
            {
                var oldBufferRef = _buffer;
                _buffer = _pool.Rent(_buffer.Length*2);
                _pool.Return(oldBufferRef);
            }
        }

        public void PutVarUInt(ulong v)
        {
            EnsureRemainingSize(10);
            var len = uint64_pack(v, _buffer.AsSpan().Slice(_pos));
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

            var strArr = v.AsSpan();
            EnsureRemainingSize(10 + strArr.Length);
            var currentPos = _buffer.AsSpan().Slice(_pos);
            var lenSize = (int)uint32_pack((uint)strArr.Length, currentPos);
            currentPos = currentPos.Slice(lenSize);
            for (int i = 0; i < strArr.Length; i++)
            {
                currentPos[i] = (byte)strArr[i];
            }
            _pos += lenSize + strArr.Length;
        }

        private static uint uint32_pack(uint value, Span<byte> @out)
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

        private static uint uint64_pack(ulong value, Span<byte> @out)
        {
            uint hi = (uint)(value >> 32);
            uint lo = (uint)value;
            uint rv;
            if (hi == 0U)
            {
                rv = uint32_pack(lo, @out);
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

        protected virtual void Dispose(bool disposing)
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
}
