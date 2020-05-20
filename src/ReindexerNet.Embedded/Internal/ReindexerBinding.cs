﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
#if NET472
using System.Diagnostics;
#else
using System.Runtime.Loader;
#endif

using int32_t = System.Int32;
using uintptr_t = System.UIntPtr;
using System.Linq;
using System.Diagnostics;
using System.Security;

[assembly: InternalsVisibleTo("ReindexerNet.EmbeddedTest")]
namespace ReindexerNet.Embedded.Internal
{
    internal static class ReindexerBinding
    {
        private const string BindingLibrary = "reindexer_embedded_server";
        private static class Windows
        {
            [DllImport("kernel32.dll")]
            internal static extern IntPtr LoadLibrary(string filename);

            [DllImport("kernel32.dll")]
            internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32.dll")]
            internal static extern bool FreeLibrary(IntPtr hModule);
        }

        private static class Linux
        {
            [DllImport("libdl.so")]
            internal static extern IntPtr dlopen(string filename, int flags);

            [DllImport("libdl.so")]
            internal static extern IntPtr dlerror();

            [DllImport("libdl.so")]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport("libdl.so")]
            internal static extern int dlclose(IntPtr handle);
        }

        private static class MacOSX
        {
            [DllImport("libSystem.dylib")]
            internal static extern IntPtr dlopen(string filename, int flags);

            [DllImport("libSystem.dylib")]
            internal static extern IntPtr dlerror();

            [DllImport("libSystem.dylib")]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport("libSystem.dylib")]
            internal static extern int dlclose(IntPtr handle);
        }

        private static class Mono
        {
            [DllImport("__Internal")]
            internal static extern IntPtr dlopen(string filename, int flags);

            [DllImport("__Internal")]
            internal static extern IntPtr dlerror();

            [DllImport("__Internal")]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport("__Internal")]
            internal static extern int dlclose(IntPtr handle);
        }

        private static class CoreCLR
        {
            [DllImport("libcoreclr.so")]
            internal static extern IntPtr dlopen(string filename, int flags);

            [DllImport("libcoreclr.so")]
            internal static extern IntPtr dlerror();

            [DllImport("libcoreclr.so")]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);

            [DllImport("libcoreclr.so")]
            internal static extern int dlclose(IntPtr handle);
        }

        // flags for dlopen
        const int RTLD_LAZY = 1;
        const int RTLD_GLOBAL = 8;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable S101 // Types should be named in PascalCase

        #region reindexer_c.h
        public static readonly Delegate.init_reindexer init_reindexer_native;
        private static ConcurrentDictionary<uintptr_t, bool> _instances = new ConcurrentDictionary<uintptr_t, bool>();
        public static uintptr_t init_reindexer()
        {
            var newInstance = init_reindexer_native();
            _instances[newInstance] = true;
            return newInstance;
        }
        private static readonly Delegate.destroy_reindexer destroy_reindexer_native;
        public static void destroy_reindexer(uintptr_t rx)
        {
            _instances.TryRemove(rx, out _);
            destroy_reindexer_native(rx);
        }

        public static readonly Delegate.reindexer_connect reindexer_connect;
        public static readonly Delegate.reindexer_ping reindexer_ping;
        public static readonly Delegate.reindexer_enable_storage reindexer_enable_storage;
        public static readonly Delegate.reindexer_init_system_namespaces reindexer_init_system_namespaces;
        public static readonly Delegate.reindexer_open_namespace reindexer_open_namespace;
        public static readonly Delegate.reindexer_drop_namespace reindexer_drop_namespace;
        public static readonly Delegate.reindexer_truncate_namespace reindexer_truncate_namespace;
        public static readonly Delegate.reindexer_rename_namespace reindexer_rename_namespace;
        public static readonly Delegate.reindexer_close_namespace reindexer_close_namespace;
        public static readonly Delegate.reindexer_add_index reindexer_add_index;
        public static readonly Delegate.reindexer_update_index reindexer_update_index;
        public static readonly Delegate.reindexer_drop_index reindexer_drop_index;
        public static readonly Delegate.reindexer_start_transaction reindexer_start_transaction;
        public static readonly Delegate.reindexer_modify_item_packed_tx reindexer_modify_item_packed_tx;
        public static readonly Delegate.reindexer_update_query_tx reindexer_update_query_tx;
        public static readonly Delegate.reindexer_delete_query_tx reindexer_delete_query_tx;
        private static readonly Delegate.reindexer_commit_transaction reindexer_commit_transaction_native;
        public static reindexer_ret reindexer_commit_transaction(uintptr_t rx, uintptr_t tr, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_commit_transaction_native(rx, tr, ctx_info);
        }

        public static readonly Delegate.reindexer_rollback_transaction reindexer_rollback_transaction;
        private static readonly Delegate.reindexer_modify_item_packed reindexer_modify_item_packed_native;
        public static reindexer_ret reindexer_modify_item_packed(uintptr_t rx, reindexer_buffer args, reindexer_buffer data, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_modify_item_packed_native(rx, args, data, ctx_info);
        }
        private static readonly Delegate.reindexer_select reindexer_select_native;
        public static reindexer_ret reindexer_select(uintptr_t rx, reindexer_string query, int as_json, int32_t[] pt_versions /* int32_t* */, int pt_versions_count, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_select_native(rx, query, as_json, pt_versions, pt_versions_count, ctx_info);
        }
        private static readonly Delegate.reindexer_select_query reindexer_select_query_native;
        public static reindexer_ret reindexer_select_query(uintptr_t rx, reindexer_buffer @in, int as_json, int32_t[] pt_versions /* int32_t* */, int pt_versions_count, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_select_query_native(rx, @in, as_json, pt_versions, pt_versions_count, ctx_info);
        }
        private static readonly Delegate.reindexer_delete_query reindexer_delete_query_native;
        public static reindexer_ret reindexer_delete_query(uintptr_t rx, reindexer_buffer @in, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_delete_query_native(rx, @in, ctx_info);
        }
        private static readonly Delegate.reindexer_update_query reindexer_update_query_native;
        public static reindexer_ret reindexer_update_query(uintptr_t rx, reindexer_buffer @in, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_update_query_native(rx, @in, ctx_info);
        }
        public static readonly Delegate.reindexer_free_buffer reindexer_free_buffer;
        public static readonly Delegate.reindexer_free_buffers reindexer_free_buffers;
        public static readonly Delegate.reindexer_commit reindexer_commit;
        public static readonly Delegate.reindexer_put_meta reindexer_put_meta;
        private static readonly Delegate.reindexer_get_meta reindexer_get_meta_native;
        public static reindexer_ret reindexer_get_meta(uintptr_t rx, reindexer_string ns, reindexer_string key, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_get_meta_native(rx, ns, key, ctx_info);
        }
        public static readonly Delegate.reindexer_cancel_context reindexer_cancel_context;
        public static readonly Delegate.reindexer_enable_logger reindexer_enable_logger;
        public static readonly Delegate.reindexer_disable_logger reindexer_disable_logger;
        #endregion

        #region server_c.h
        private static readonly Delegate.init_reindexer_server init_reindexer_server_native;
        private static ConcurrentDictionary<uintptr_t, bool> _serverInstances = new ConcurrentDictionary<uintptr_t, bool>();
        public static uintptr_t init_reindexer_server()
        {
            var newInstance = init_reindexer_server_native();
            _serverInstances[newInstance] = true;
            return newInstance;
        }
        private static readonly Delegate.destroy_reindexer_server destroy_reindexer_server_native;
        public static void destroy_reindexer_server(uintptr_t psvc)
        {
            _serverInstances.TryRemove(psvc, out _);
            destroy_reindexer_server_native(psvc);
        }
        public static readonly Delegate.start_reindexer_server start_reindexer_server;
        public static readonly Delegate.stop_reindexer_server stop_reindexer_server;
        private static readonly Delegate.get_reindexer_instance get_reindexer_instance_native;
        public static reindexer_error get_reindexer_instance(uintptr_t psvc, reindexer_string dbname, reindexer_string user, reindexer_string pass, ref uintptr_t rx)
        {
            var result = get_reindexer_instance_native(psvc, dbname, user, pass, ref rx);
            _instances[rx] = true;
            return result;
        }
        public static readonly Delegate.check_server_ready check_server_ready;
        public static readonly Delegate.reopen_log_files reopen_log_files;
        #endregion

        public static readonly Delegate.malloc_free malloc_free;

        public static class Delegate
        {
            #region reindexer_c.h            
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate uintptr_t init_reindexer();
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate void destroy_reindexer(uintptr_t rx);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_connect(uintptr_t rx, reindexer_string dsn, ConnectOpts opts, reindexer_string client_vers);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_ping(uintptr_t rx);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_enable_storage(uintptr_t rx, reindexer_string path, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_init_system_namespaces(uintptr_t rx);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_open_namespace(uintptr_t rx, reindexer_string nsName, StorageOpts opts, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_drop_namespace(uintptr_t rx, reindexer_string nsName, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_truncate_namespace(uintptr_t rx, reindexer_string nsName, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_rename_namespace(uintptr_t rx, reindexer_string srcNsName, reindexer_string dstNsName, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_close_namespace(uintptr_t rx, reindexer_string nsName, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_add_index(uintptr_t rx, reindexer_string nsName, reindexer_string indexDefJson, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_update_index(uintptr_t rx, reindexer_string nsName, reindexer_string indexDefJson, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_drop_index(uintptr_t rx, reindexer_string nsName, reindexer_string index, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_tx_ret reindexer_start_transaction(uintptr_t rx, reindexer_string nsName);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_modify_item_packed_tx(uintptr_t rx, uintptr_t tr, reindexer_buffer args, reindexer_buffer data);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_update_query_tx(uintptr_t rx, uintptr_t tr, reindexer_buffer @in);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_delete_query_tx(uintptr_t rx, uintptr_t tr, reindexer_buffer @in);

            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_ret reindexer_commit_transaction(uintptr_t rx, uintptr_t tr, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_rollback_transaction(uintptr_t rx, uintptr_t tr);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_ret reindexer_modify_item_packed(uintptr_t rx, reindexer_buffer args, reindexer_buffer data, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_ret reindexer_select(uintptr_t rx, reindexer_string query, int as_json, int32_t[] pt_versions /* int32_t* */, int pt_versions_count, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_ret reindexer_select_query(uintptr_t rx, reindexer_buffer @in, int as_json, int32_t[] pt_versions /* int32_t* */, int pt_versions_count, reindexer_ctx_info ctx_info);

            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_ret reindexer_delete_query(uintptr_t rx, reindexer_buffer @in, reindexer_ctx_info ctx_info);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_ret reindexer_update_query(uintptr_t rx, reindexer_buffer @in, reindexer_ctx_info ctx_info);

            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_free_buffer(reindexer_resbuffer @in);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_free_buffers(reindexer_resbuffer[] @in /* reindexer_resbuffer* */, int count);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_commit(uintptr_t rx, reindexer_string nsName);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_put_meta(uintptr_t rx, reindexer_string ns, reindexer_string key, reindexer_string data, reindexer_ctx_info ctx_info);

            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_ret reindexer_get_meta(uintptr_t rx, reindexer_string ns, reindexer_string key, reindexer_ctx_info ctx_info);

            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reindexer_cancel_context(reindexer_ctx_info ctx_info, ctx_cancel_type how);

            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate void reindexer_enable_logger([MarshalAs(UnmanagedType.FunctionPtr)]LogWriterAction logWriter);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate void reindexer_disable_logger();
            #endregion

            #region server_c.h
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate uintptr_t init_reindexer_server();
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate void destroy_reindexer_server(uintptr_t psvc);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error start_reindexer_server(uintptr_t psvc, reindexer_string config);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error stop_reindexer_server(uintptr_t psvc);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error get_reindexer_instance(uintptr_t psvc, reindexer_string dbname, reindexer_string user, reindexer_string pass, ref uintptr_t rx);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate int check_server_ready(uintptr_t psvc);
            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate reindexer_error reopen_log_files(uintptr_t psvc);
            #endregion

            [SuppressUnmanagedCodeSecurity, UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public delegate void malloc_free(IntPtr ptr);
        }

#pragma warning restore S101 // Types should be named in PascalCase
#pragma warning restore IDE1006 // Naming Styles

        public const string ReindexerVersion = "v2.9.0";

        private static IntPtr NativeLibraryAddr;

#pragma warning disable S3963 // "static" fields should be initialized inline
        static ReindexerBinding()
        {
#if NET472
            AppDomain.CurrentDomain.DomainUnload += (_, _) => ReindexerBindingUnloading();
#else
            (AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly()) ?? AssemblyLoadContext.Default).Unloading += (_)=> ReindexerBindingUnloading();
#endif
            LoadAndBindNativeLibrary();

            _bufferGc.Start();
        }
#pragma warning restore S3963 // "static" fields should be initialized inline

        private static readonly HashSet<string> _searchBinPaths = new HashSet<string>{
                AppDomain.CurrentDomain?.BaseDirectory ?? "",
                Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location ?? "."),
                Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? "."),
                Directory.GetCurrentDirectory(),
                "bin",
                ""
                };

        private static void LoadAndBindNativeLibrary()
        {
            NativeLibraryAddr = LoadNativeLibrary();
            var delegateTypes = typeof(Delegate).GetNestedTypes().Where(nt => typeof(System.Delegate).IsAssignableFrom(nt)).ToList();
            var functionMembers = typeof(ReindexerBinding).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => typeof(System.Delegate).IsAssignableFrom(f.FieldType))
                .ToDictionary(f => f.FieldType, f => f);
            foreach (var delegateType in delegateTypes)
            {
                var nativeSymbol = LoadSymbol(NativeLibraryAddr, delegateType.Name);
                if (nativeSymbol == default)
                    throw new MissingMethodException($"The native method '{delegateType.Name}' does not exist");

                functionMembers[delegateType].SetValue(null, Marshal.GetDelegateForFunctionPointer(nativeSymbol, delegateType));
            }
        }

        private static IntPtr LoadNativeLibrary()
        {
            string arch = Environment.Is64BitProcess ? "-x64" : "-x86";
            var fullPath = "";
            string platformPath;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platformPath = Path.Combine("runtimes", "osx" + arch, "native", $"lib{BindingLibrary}.dylib");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platformPath = Path.Combine("runtimes", "linux" + arch, "native", $"lib{BindingLibrary}.so");
            }
            else // RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            {
                platformPath = Path.Combine("runtimes", "win" + arch, "native", $"{BindingLibrary}.dll");
            }

            foreach (var searchBinPath in _searchBinPaths)
            {
                fullPath = Path.Combine(searchBinPath, platformPath);
                if (File.Exists(fullPath))
                    break;
            }

            if (string.IsNullOrEmpty(fullPath))
                throw new FileNotFoundException($"Couldn't find {platformPath} in these search paths: {string.Join(" ,", _searchBinPaths)}", BindingLibrary);

            Debug.WriteLine($"Trying to load native library from '{fullPath}'");

            try
            {
                var addr = PlatformSpecificLoadLibrary(fullPath, out var errorMsg);
                if (errorMsg != null)
                    throw new InvalidOperationException(errorMsg);

                Debug.WriteLine($"The native library loaded from '{fullPath}'");
                return addr;
            }
            catch (Exception e)
            {
                throw new DllNotFoundException($"Couldn't load file '{BindingLibrary}' in these search paths {string.Join(" ,", _searchBinPaths.Select(searchBinPath => Path.Combine(searchBinPath, platformPath)))}", e);
            }
        }

        private static IntPtr PlatformSpecificLoadLibrary(string libraryPath, out string errorMsg)
        {
            if (Platform.IsWindows)
            {
                errorMsg = null;
                return Windows.LoadLibrary(libraryPath);
            }
            if (Platform.IsLinux)
            {
                if (Platform.IsMono)
                {
                    return LoadLibraryPosix(Mono.dlopen, Mono.dlerror, libraryPath, out errorMsg);
                }
                if (Platform.IsNetCore)
                {
                    return LoadLibraryPosix(CoreCLR.dlopen, CoreCLR.dlerror, libraryPath, out errorMsg);
                }
                return LoadLibraryPosix(Linux.dlopen, Linux.dlerror, libraryPath, out errorMsg);
            }
            if (Platform.IsMacOSX)
            {
                return LoadLibraryPosix(MacOSX.dlopen, MacOSX.dlerror, libraryPath, out errorMsg);
            }
            throw new InvalidOperationException("Unsupported platform.");
        }

        private static bool PlatformSpecificFreeLibrary(IntPtr libraryAddr)
        {
            if (Platform.IsWindows)
            {
                return Windows.FreeLibrary(libraryAddr);
            }
            if (Platform.IsLinux)
            {
                if (Platform.IsMono)
                {
                    return Mono.dlclose(libraryAddr) != 0;
                }
                if (Platform.IsNetCore)
                {
                    return CoreCLR.dlclose(libraryAddr) != 0;
                }
                return Linux.dlclose(libraryAddr) != 0;
            }
            if (Platform.IsMacOSX)
            {
                return MacOSX.dlclose(libraryAddr) != 0;
            }
            throw new InvalidOperationException("Unsupported platform.");
        }

        private static IntPtr LoadLibraryPosix(Func<string, int, IntPtr> dlopenFunc, Func<IntPtr> dlerrorFunc, string libraryPath, out string errorMsg)
        {
            errorMsg = null;
            IntPtr ret = dlopenFunc(libraryPath, RTLD_GLOBAL + RTLD_LAZY);
            if (ret == IntPtr.Zero)
            {
                errorMsg = Marshal.PtrToStringAnsi(dlerrorFunc());
            }
            return ret;
        }

        private static IntPtr LoadSymbol(IntPtr handle, string symbolName)
        {
            if (Platform.IsWindows)
            {
                // See http://stackoverflow.com/questions/10473310 for background on this.
                if (Platform.Is64Bit)
                {
                    return Windows.GetProcAddress(handle, symbolName);
                }
                else
                {
                    IntPtr candidate = Windows.GetProcAddress(handle, symbolName);
                    if (candidate != IntPtr.Zero)
                    {
                        return candidate;
                    }

                    // Yes, we could potentially predict the size... but it's a lot simpler to just try
                    // all the candidates. Most functions have a suffix of @0, @4 or @8 so we won't be trying
                    // many options - and if it takes a little bit longer to fail if we've really got the wrong
                    // library, that's not a big problem. This is only called once per function in the native library.
                    symbolName = "_" + symbolName + "@";
                    for (int stackSize = 0; stackSize < 128; stackSize += 4)
                    {
                        candidate = Windows.GetProcAddress(handle, symbolName + stackSize);
                        if (candidate != IntPtr.Zero)
                        {
                            return candidate;
                        }
                    }
                    // Fail.
                    return IntPtr.Zero;
                }
            }
            if (Platform.IsLinux)
            {
                if (Platform.IsMono)
                {
                    return Mono.dlsym(handle, symbolName);
                }
                if (Platform.IsNetCore)
                {
                    return CoreCLR.dlsym(handle, symbolName);
                }
                return Linux.dlsym(handle, symbolName);
            }
            if (Platform.IsMacOSX)
            {
                return MacOSX.dlsym(handle, symbolName);
            }
            throw new InvalidOperationException("Unsupported platform.");
        }

        private static void ReindexerBindingUnloading()
        {
            try
            {
                ReindexerEmbedded.DisableLogger();

                _bufferGcCancelToken.Cancel();
                _bufferGc.Join();

                //to unlock file.
                if (PlatformSpecificFreeLibrary(NativeLibraryAddr))
                    Debug.WriteLine($"The native library is unloaded successfully.");
                else
                    Debug.WriteLine($"The native library couldn't unload.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static readonly SemaphoreSlim _responseBufferConcurrenyLimit = new SemaphoreSlim(65534); //kMaxConcurentQueries 
        private static readonly Thread _bufferGc = new Thread(ResponseBufferGarbageWorker) { Name = "ReindexerResponseBufferGarbageWorker", IsBackground = true };
        private const int _bufferGcIntervalMs = 5000;
        private static readonly ConcurrentQueue<reindexer_resbuffer> _responseBuffersToFree = new ConcurrentQueue<reindexer_resbuffer>();
        private static readonly CancellationTokenSource _bufferGcCancelToken = new CancellationTokenSource();

        public static void FreeBuffer(reindexer_resbuffer buffer)
        {
            if (buffer.results_ptr != UIntPtr.Zero)
                _responseBuffersToFree.Enqueue(buffer);

            if (_responseBuffersToFree.Count >= 65000) //hard limit
            {
                lock (_responseBuffersToFree)
                {
                    if (_responseBuffersToFree.Count >= 65000) //double lock check
                    {
                        FreeAllBuffers();
                    }
                }
            }
        }

        private static void FreeAllBuffers()
        {
            var bufferToFrees = new List<reindexer_resbuffer>();
            while (_responseBuffersToFree.TryDequeue(out var bufferToFree))
            {
                bufferToFrees.Add(bufferToFree);
            }
            if (bufferToFrees.Count > 0)
            {
                ReindexerBinding.reindexer_free_buffers(bufferToFrees.ToArray(), bufferToFrees.Count);
                _responseBufferConcurrenyLimit.Release(bufferToFrees.Count);
                bufferToFrees.Clear();
            }
        }

        private static void ResponseBufferGarbageWorker()
        {
            while (!_bufferGcCancelToken.IsCancellationRequested)
            {
                lock (_responseBuffersToFree)
                {
                    FreeAllBuffers();
                }
                WaitHandle.WaitAny(new[] { _bufferGcCancelToken.Token.WaitHandle }, _bufferGcIntervalMs);
            }

            FreeAllBuffers();

            foreach (var rx in _instances.Keys)
            {
                try
                {
                    destroy_reindexer(rx); //to unlock dbs
                }
                catch
                {
                    //ignored because of runtime is unloading.
                }
            }

            foreach (var psvc in _serverInstances.Keys)
            {
                try
                {
                    stop_reindexer_server(psvc);
                    destroy_reindexer_server(psvc); //to unlock dbs
                }
                catch
                {
                    //ignored because of runtime is unloading.
                }
            }
        }
    }
}
