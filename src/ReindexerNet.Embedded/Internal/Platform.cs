using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ReindexerNet.Embedded.Internal
{
    static class Platform
    {
        public static readonly bool Is64Bit = IntPtr.Size == 8;
        public static readonly bool IsLinux;
        public static readonly bool IsMacOSX;
        public static readonly bool IsWindows;
        public static readonly bool IsMono;
        public static readonly bool IsNetCore;

#pragma warning disable S3963 // "static" fields should be initialized inline
        static Platform()
        {
#if NETSTANDARD2_1 || NETSTANDARD2_0
            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            IsMacOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            IsNetCore = RuntimeInformation.FrameworkDescription.StartsWith(".NET Core");
#else
            var platform = Environment.OSVersion.Platform;

            // PlatformID.MacOSX is never returned, commonly used trick is to identify Mac is by using uname.
            IsMacOSX = (platform == PlatformID.Unix && GetUname() == "Darwin");
            IsLinux = (platform == PlatformID.Unix && !IsMacOSX);
            IsWindows = (platform == PlatformID.Win32NT || platform == PlatformID.Win32S || platform == PlatformID.Win32Windows);
            IsNetCore = false;
#endif
            IsMono = Type.GetType("Mono.Runtime") != null;

        }
#pragma warning restore S3963 // "static" fields should be initialized inline

        [DllImport("libc")]
        static extern int uname(IntPtr buf);

        static string GetUname()
        {
            var buffer = Marshal.AllocHGlobal(8192);
            try
            {
                if (uname(buffer) == 0)
                {
                    return Marshal.PtrToStringAnsi(buffer);
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }
    }
}
