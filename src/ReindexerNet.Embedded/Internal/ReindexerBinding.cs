using System;
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

[assembly: InternalsVisibleTo("ReindexerNet.EmbeddedTest")]
namespace ReindexerNet.Embedded.Internal
{
    internal static class ReindexerBinding
    {
        private const string BindingLibrary = "reindexer_embedded_server";
#if NET472
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeLibrary(IntPtr hModule);
#endif

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable S101 // Types should be named in PascalCase
        #region reindexer_c.h
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = nameof(init_reindexer))]
        public static extern uintptr_t init_reindexer_native();
        private static ConcurrentDictionary<uintptr_t, bool> _instances = new ConcurrentDictionary<uintptr_t, bool>();
        public static uintptr_t init_reindexer()
        {
            var newInstance = init_reindexer_native();
            _instances[newInstance] = true;
            return newInstance;
        }
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = nameof(destroy_reindexer))]
        public static extern void destroy_reindexer_native(uintptr_t rx);
        public static void destroy_reindexer(uintptr_t rx)
        {
            _instances.TryRemove(rx, out _);
            destroy_reindexer_native(rx);
        }
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_connect(uintptr_t rx, reindexer_string dsn, ConnectOpts opts, reindexer_string client_vers);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_ping(uintptr_t rx);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_enable_storage(uintptr_t rx, reindexer_string path, reindexer_ctx_info ctx_info);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_init_system_namespaces(uintptr_t rx);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_open_namespace(uintptr_t rx, reindexer_string nsName, StorageOpts opts, reindexer_ctx_info ctx_info);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_drop_namespace(uintptr_t rx, reindexer_string nsName, reindexer_ctx_info ctx_info);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_truncate_namespace(uintptr_t rx, reindexer_string nsName, reindexer_ctx_info ctx_info);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_rename_namespace(uintptr_t rx, reindexer_string srcNsName, reindexer_string dstNsName, reindexer_ctx_info ctx_info);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_close_namespace(uintptr_t rx, reindexer_string nsName, reindexer_ctx_info ctx_info);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_add_index(uintptr_t rx, reindexer_string nsName, reindexer_string indexDefJson, reindexer_ctx_info ctx_info);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_update_index(uintptr_t rx, reindexer_string nsName, reindexer_string indexDefJson, reindexer_ctx_info ctx_info);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_drop_index(uintptr_t rx, reindexer_string nsName, reindexer_string index, reindexer_ctx_info ctx_info);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_tx_ret reindexer_start_transaction(uintptr_t rx, reindexer_string nsName);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_modify_item_packed_tx(uintptr_t rx, uintptr_t tr, reindexer_buffer args, reindexer_buffer data);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_update_query_tx(uintptr_t rx, uintptr_t tr, reindexer_buffer @in);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_delete_query_tx(uintptr_t rx, uintptr_t tr, reindexer_buffer @in);

        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = nameof(reindexer_commit_transaction))]
        private static extern reindexer_ret reindexer_commit_transaction_native(uintptr_t rx, uintptr_t tr, reindexer_ctx_info ctx_info);
        public static reindexer_ret reindexer_commit_transaction(uintptr_t rx, uintptr_t tr, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_commit_transaction_native(rx, tr, ctx_info);
        }

        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_rollback_transaction(uintptr_t rx, uintptr_t tr);

        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = nameof(reindexer_modify_item_packed))]
        public static extern reindexer_ret reindexer_modify_item_packed_native(uintptr_t rx, reindexer_buffer args, reindexer_buffer data, reindexer_ctx_info ctx_info);
        public static reindexer_ret reindexer_modify_item_packed(uintptr_t rx, reindexer_buffer args, reindexer_buffer data, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_modify_item_packed_native(rx, args, data, ctx_info);
        }

        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = nameof(reindexer_select))]
        public static extern reindexer_ret reindexer_select_native(uintptr_t rx, reindexer_string query, int as_json, int32_t[] pt_versions /* int32_t* */, int pt_versions_count, reindexer_ctx_info ctx_info);
        public static reindexer_ret reindexer_select(uintptr_t rx, reindexer_string query, int as_json, int32_t[] pt_versions /* int32_t* */, int pt_versions_count, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_select_native(rx, query, as_json, pt_versions, pt_versions_count, ctx_info);
        }

        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = nameof(reindexer_select_query))]
        public static extern reindexer_ret reindexer_select_query_native(uintptr_t rx, reindexer_buffer @in, int as_json, int32_t[] pt_versions /* int32_t* */, int pt_versions_count, reindexer_ctx_info ctx_info);
        public static reindexer_ret reindexer_select_query(uintptr_t rx, reindexer_buffer @in, int as_json, int32_t[] pt_versions /* int32_t* */, int pt_versions_count, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_select_query_native(rx, @in, as_json, pt_versions, pt_versions_count, ctx_info);
        }

        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = nameof(reindexer_delete_query))]
        public static extern reindexer_ret reindexer_delete_query_native(uintptr_t rx, reindexer_buffer @in, reindexer_ctx_info ctx_info);
        public static reindexer_ret reindexer_delete_query(uintptr_t rx, reindexer_buffer @in, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_delete_query_native(rx, @in, ctx_info);
        }

        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = nameof(reindexer_update_query))]
        public static extern reindexer_ret reindexer_update_query_native(uintptr_t rx, reindexer_buffer @in, reindexer_ctx_info ctx_info);
        public static reindexer_ret reindexer_update_query(uintptr_t rx, reindexer_buffer @in, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_update_query_native(rx, @in, ctx_info);
        }

        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_free_buffer(reindexer_resbuffer @in);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_free_buffers(reindexer_resbuffer[] @in /* reindexer_resbuffer* */, int count);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_commit(uintptr_t rx, reindexer_string nsName);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_put_meta(uintptr_t rx, reindexer_string ns, reindexer_string key, reindexer_string data, reindexer_ctx_info ctx_info);

        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = nameof(reindexer_get_meta))]
        public static extern reindexer_ret reindexer_get_meta_native(uintptr_t rx, reindexer_string ns, reindexer_string key, reindexer_ctx_info ctx_info);
        public static reindexer_ret reindexer_get_meta(uintptr_t rx, reindexer_string ns, reindexer_string key, reindexer_ctx_info ctx_info)
        {
            _responseBufferConcurrenyLimit.Wait();
            return reindexer_get_meta_native(rx, ns, key, ctx_info);
        }

        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reindexer_cancel_context(reindexer_ctx_info ctx_info, ctx_cancel_type how);

        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern void reindexer_enable_logger([MarshalAs(UnmanagedType.FunctionPtr)]LogWriterAction logWriter);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern void reindexer_disable_logger();
        #endregion

        #region server_c.h
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = nameof(init_reindexer_server))]
        public static extern uintptr_t init_reindexer_server_native();
        private static ConcurrentDictionary<uintptr_t, bool> _serverInstances = new ConcurrentDictionary<uintptr_t, bool>();
        public static uintptr_t init_reindexer_server()
        {
            var newInstance = init_reindexer_server_native();
            _serverInstances[newInstance] = true;
            return newInstance;
        }
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = nameof(destroy_reindexer_server))]
        public static extern void destroy_reindexer_server_native(uintptr_t psvc);
        public static void destroy_reindexer_server(uintptr_t psvc)
        {
            _serverInstances.TryRemove(psvc, out _);
            destroy_reindexer_server_native(psvc);
        }
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error start_reindexer_server(uintptr_t psvc, reindexer_string config);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error stop_reindexer_server(uintptr_t psvc);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = nameof(get_reindexer_instance))]
        public static extern reindexer_error get_reindexer_instance_native(uintptr_t psvc, reindexer_string dbname, reindexer_string user, reindexer_string pass, ref uintptr_t rx);
        public static reindexer_error get_reindexer_instance(uintptr_t psvc, reindexer_string dbname, reindexer_string user, reindexer_string pass, ref uintptr_t rx)
        {
            var result = get_reindexer_instance_native(psvc, dbname, user, pass, ref rx);
            _instances[rx] = true;
            return result;
        }
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern int check_server_ready(uintptr_t psvc);
        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern reindexer_error reopen_log_files(uintptr_t psvc);
        #endregion

        [DllImport(BindingLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern void malloc_free(IntPtr ptr);
#pragma warning restore S101 // Types should be named in PascalCase
#pragma warning restore IDE1006 // Naming Styles

        public const string ReindexerVersion = "v2.9.0";
#pragma warning disable S3963 // "static" fields should be initialized inline
        static ReindexerBinding()
        {
#if NET472
            AppDomain.CurrentDomain.DomainUnload += AppDomain_DomainUnload;
            LoadWindowsLibrary(BindingLibrary);
#else
            var ctx = new CustomAssemblyLoadContext();
            ctx.LoadUnmanagedLibrary(BindingLibrary);
#endif

            _bufferGc.Start();
        }
#pragma warning restore S3963 // "static" fields should be initialized inline

#if NET472
        private static string _dllPath;
        static void LoadWindowsLibrary(string libName)
        {
            string libFile = libName + ".dll";
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string arch = Environment.Is64BitProcess ? "-x64" : "-x86";
            string platform = $"win{arch}";
            var paths = new HashSet<string>
                {
                    Path.Combine(assemblyDirectory, "bin", "runtimes", platform, "native", libFile),
                    Path.Combine(assemblyDirectory, "runtimes", platform, "native", libFile),
                    Path.Combine(assemblyDirectory, libFile),

                    Path.Combine(rootDirectory, "bin", "runtimes", platform, "native", libFile),
                    Path.Combine(rootDirectory, "runtimes", platform, "native", libFile),
                    Path.Combine(rootDirectory, libFile),

                    Path.Combine(assemblyDirectory, "bin", libFile),
                    Path.Combine(rootDirectory, "bin", libFile)
                };

            foreach (var path in paths)
            {
                if (path == null)
                {
                    continue;
                }

                if (File.Exists(path))
                {
                    var addr = LoadLibrary(path);
                    if (addr == IntPtr.Zero)
                    {
                        throw new FileNotFoundException("LoadLibrary failed: " + path);
                    }
                    _dllPath = path;
                    return; //addr
                }
            }

            throw new FileNotFoundException("LoadLibrary failed: unable to locate library " + libFile + ". Searched: " + paths.Aggregate((a, b) => a + "; " + b));
        }

        private static void AppDomain_DomainUnload(object sender, EventArgs e)
        {
            try
            {
                AppDomain.CurrentDomain.DomainUnload -= AppDomain_DomainUnload;
                ReindexerEmbedded.DisableLogger();

                _bufferGcCancelToken.Cancel();
                _bufferGc.Join();

                //to unlock file.
                foreach (ProcessModule mod in Process.GetCurrentProcess().Modules)
                {
                    if (mod.FileName == _dllPath)
                    {
                        FreeLibrary(mod.BaseAddress);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
#else
        private class CustomAssemblyLoadContext : AssemblyLoadContext
        {
            private static readonly string[] _searchBinPaths = new[]{
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                Directory.GetCurrentDirectory(),
                "bin",
                ""
                };

            internal void LoadUnmanagedLibrary(string absolutePath)
            {
                LoadUnmanagedDll(absolutePath);
                Unloading += CustomAssemblyLoadContext_Unloading;
            }

            private void CustomAssemblyLoadContext_Unloading(AssemblyLoadContext obj)
            {
                Unloading -= CustomAssemblyLoadContext_Unloading;
                ReindexerEmbedded.DisableLogger();

                _bufferGcCancelToken.Cancel();
                _bufferGc.Join();
            }

            protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
            {
                string arch = Environment.Is64BitProcess ? "-x64" : "-x86";
                var fullPath = "";
                string platformPath;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    platformPath = Path.Combine("runtimes", "osx" + arch, "native", $"lib{unmanagedDllName}.dylib");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    platformPath = Path.Combine("runtimes", "linux" + arch, "native", $"lib{unmanagedDllName}.so");
                }
                else // RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                {
                    platformPath = Path.Combine("runtimes", "win" + arch, "native", $"{unmanagedDllName}.dll");
                }
                
                foreach (var searchBinPath in _searchBinPaths)
                {
                    fullPath = Path.Combine(searchBinPath, platformPath);
                    if (File.Exists(fullPath))
                        break;
                }

                if (string.IsNullOrEmpty(fullPath))
                    throw new FileNotFoundException($"Couldn't find {platformPath} in these search paths: {string.Join(" ,", _searchBinPaths)}", unmanagedDllName);
                
                Debug.WriteLine($"Trying to load native library from '{fullPath}'");

                try
                {
                    return LoadUnmanagedDllFromPath(fullPath);
                }
                catch (Exception e)
                {
                    throw new DllNotFoundException($"Couldn't load file '{unmanagedDllName}' in these search paths {string.Join(" ,", _searchBinPaths.Select(searchBinPath => Path.Combine(searchBinPath, platformPath)))}", e);
                }
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                throw new NotImplementedException();
            }
        }
#endif

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
