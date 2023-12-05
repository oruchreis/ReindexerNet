#pragma warning disable S1135 // Track uses of "TODO" tags
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable RCS1018 // Add accessibility modifiers.
#pragma warning disable S101 // Types should be named in PascalCase
using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using int8_t = System.SByte;
using int32_t = System.Int32;
using int64_t = System.Int64;
using uint64_t = System.UInt64;
using uintptr_t = System.UIntPtr;

namespace ReindexerNet.Embedded.Internal;

[StructLayout(LayoutKind.Sequential)]
struct reindexer_config
{
    int64_t allocator_cache_limit;
    float allocator_max_cache_part;
}

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

    public static void PinBufferFor(ReadOnlySpan<byte> byteSpan1, ReadOnlySpan<byte> byteSpan2,
        Action<reindexer_buffer, reindexer_buffer> pinnedAction)
    {
        unsafe
        {
            fixed (byte* pData1 = &MemoryMarshal.GetReference(byteSpan1))
            fixed (byte* pData2 = &MemoryMarshal.GetReference(byteSpan2))
            {
                pinnedAction(
                    new reindexer_buffer
                    {
                        data = (IntPtr)pData1,
                        len = byteSpan1.Length
                    },
                    new reindexer_buffer
                    {
                        data = (IntPtr)pData2,
                        len = byteSpan2.Length
                    }
                    );
            }
        }
    }
}

internal sealed class ReindexerBufferHandle : IDisposable
{
    private readonly GCHandle _gcHandle;
    private readonly MemoryHandle _memoryHandle;

    public ReindexerBufferHandle(byte[] byteArray)
    {
        _gcHandle = GCHandle.Alloc(byteArray, GCHandleType.Pinned);
        Buffer = new reindexer_buffer
        {
            data = _gcHandle.AddrOfPinnedObject(),
            len = byteArray?.Length ?? 0
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

    public static implicit operator reindexer_buffer(ReindexerBufferHandle handle)
    {
        return handle.Buffer;
    }

    public void Dispose()
    {
        _gcHandle.Free();
        _memoryHandle.Dispose();
    }
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
            return [];

        unsafe
        {
            return new ReadOnlySpan<byte>((byte*)rb.data, rb.len);
        }
    }

    public static implicit operator string(reindexer_resbuffer rb)
    {
        unsafe
        {
            var ptr = (IntPtr)(byte*)rb.data;
            var result = Marshal.PtrToStringAnsi(ptr);
            ReindexerBinding.reindexer_malloc_free(ptr);
            return result;
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
struct reindexer_error
{
    public IntPtr what; //const char*
    public int code;
}

[StructLayout(LayoutKind.Sequential)]
struct reindexer_string
{
    public IntPtr p;//void* => reindexer içeride const char*'a cast ediyor.
    public int32_t n; //bunu içeride Span.Slice gibi kullanıyor. Belki p ReadOnlySpan<char> da olabilir.
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public int8_t[] reserved;

    public static ReindexerStringHandle From(byte[] byteArray)
    {
        return new ReindexerStringHandle(byteArray);
    }

    public static ReindexerStringHandle From(string utf8Str)
    {
        return new ReindexerStringHandle(utf8Str != null ? Encoding.UTF8.GetBytes(utf8Str) : null);
    }
}

internal sealed class ReindexerStringHandle : IDisposable
{
    private readonly GCHandle _gcHandle;
    private readonly MemoryHandle _memoryHandle;

    public ReindexerStringHandle(byte[] byteArray)
    {
        _gcHandle = GCHandle.Alloc(byteArray, GCHandleType.Pinned);
        RxString = new reindexer_string
        {
            p = _gcHandle.AddrOfPinnedObject(),
            n = byteArray?.Length ?? 0
        };
    }

    public ReindexerStringHandle(Memory<byte> byteMem)
    {
        _memoryHandle = byteMem.Pin();
        unsafe
        {
            RxString = new reindexer_string
            {
                p = (IntPtr)_memoryHandle.Pointer,
                n = byteMem.Length
            };
        }
    }

    public reindexer_string RxString { get; private set; }

    public static implicit operator reindexer_string(ReindexerStringHandle handle)
    {
        return handle.RxString;
    }

    public void Dispose()
    {
        _gcHandle.Free();
        _memoryHandle.Dispose();
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

#pragma warning restore S101 // Types should be named in PascalCase
#pragma warning restore RCS1018 // Add accessibility modifiers.
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore S1135 // Track uses of "TODO" tags