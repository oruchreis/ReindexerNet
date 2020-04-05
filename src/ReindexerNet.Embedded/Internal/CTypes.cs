#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable RCS1018 // Add accessibility modifiers.
#pragma warning disable S101 // Types should be named in PascalCase
using uintptr_t = System.UIntPtr;
using uint64_t = System.UInt64;
using int64_t = System.Int64;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System;
using System.Text;
using System.Threading;
using System.Buffers;

namespace ReindexerNet.Embedded.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    struct reindexer_buffer
    {
        public IntPtr data;//uint8_t*
        public int len;

        public static implicit operator ReadOnlySpan<byte>(reindexer_buffer rb)
        {
            unsafe
            {
                return new ReadOnlySpan<byte>((void*)rb.data, rb.len);
            }
        }

        public static ReindexerBufferHandle From(byte[] byteArray)
        {
            return new ReindexerBufferHandle(byteArray);
        }

        public static void PinBufferFor(ReadOnlySpan<byte> byteSpan, Action<reindexer_buffer> pinnedAction)
        {
            unsafe
            {
                fixed (byte* pData = &MemoryMarshal.GetReference(byteSpan))
                {
                    pinnedAction(new reindexer_buffer
                    {
                        data = (IntPtr)pData,
                        len = byteSpan.Length
                    });
                }
            }
        }
    }

    class ReindexerBufferHandle : IDisposable
    {
        private readonly GCHandle _gcHandle;
        private readonly MemoryHandle _memoryHandle;

        public ReindexerBufferHandle(byte[] byteArray)
        {
            _gcHandle = GCHandle.Alloc(byteArray, GCHandleType.Pinned);
            Buffer = new reindexer_buffer
            {
                data = _gcHandle.AddrOfPinnedObject(),
                len = byteArray.Length
            };
        }

        public ReindexerBufferHandle(Memory<byte> byteMem)
        {
            _memoryHandle = byteMem.Pin();
            unsafe
            {
                Buffer = new reindexer_buffer
                {
                    data = (IntPtr)_memoryHandle.Pointer,
                    len = byteMem.Length
                };
            }
        }

        public reindexer_buffer Buffer { get; private set; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _gcHandle.Free();
                    _memoryHandle.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ReindexerBufferHandle()
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

    [StructLayout(LayoutKind.Sequential)]
    struct reindexer_resbuffer
    {
        public uintptr_t results_ptr;
        public uintptr_t data;
        public int len;

        public void Free()
        {
            ReindexerBinding.FreeBuffer(this);
        }

        public static implicit operator ReadOnlySpan<byte>(reindexer_resbuffer rb)
        {
            if (rb.data == UIntPtr.Zero || rb.len == 0)
                return ReadOnlySpan<byte>.Empty;

            unsafe
            {

                return new ReadOnlySpan<byte>((byte*)rb.data, rb.len);
            }
        }

        public static implicit operator string(reindexer_resbuffer rb)
        {
            unsafe
            {
                return Marshal.PtrToStringAnsi((IntPtr)(byte*)rb.data);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct reindexer_error
    {
        public string what; //const char*
        public int code;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct reindexer_string
    {
        public string p;//void* => reindexer içeride const char*'a cast ediyor.
        public int n; //bunu içeride Span.Slice gibi kullanıyor. Belki p ReadOnlySpan<char> da olabilir.

        public static implicit operator string(reindexer_string rs)
        {
            return rs.p.Substring(0, rs.n);
        }

        public static implicit operator reindexer_string(string str)
        {
            return new reindexer_string { p = str, n = str.Length };
        }

        public static implicit operator reindexer_string(byte[] byteArray)
        {
            return Encoding.UTF8.GetString(byteArray); //todo: pinning string is not a good idea, so can't convert "p" to IntPtr. Search for another solution.
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct reindexer_ret
    {
        public reindexer_resbuffer @out;
        public int err_code;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct reindexer_tx_ret
    {
        public uintptr_t tx_id;
        public reindexer_error err;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct reindexer_ctx_info
    {
        public uint64_t ctx_id;  // 3 most significant bits will be used as flags and discarded
        public int64_t exec_timeout; //todo: DateTimeOffset olabilir, araştır.
    }

    enum ctx_cancel_type
    {
        cancel_expilicitly,
        cancel_on_timeout
    }

    enum LogLevel
    {
        LogNone, LogError, LogWarning, LogInfo, LogTrace
    }

    enum DataFormat { FormatJson, FormatCJson }

    enum QueryResultTag
    {
        End = 0,
        Aggregation = 1,
        Explain = 2
    }
}
#pragma warning restore S101 // Types should be named in PascalCase
#pragma warning restore RCS1018 // Add accessibility modifiers.
#pragma warning restore IDE1006 // Naming Styles
