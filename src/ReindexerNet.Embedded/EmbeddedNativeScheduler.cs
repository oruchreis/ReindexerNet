using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ReindexerNet.Embedded;

internal sealed class EmbeddedNativeScheduler : IDisposable
{
    private readonly ConcurrentQueue<IEmbeddedNativeWorkItem> _queue = new();
    private readonly SemaphoreSlim _items = new(0);
    private readonly SemaphoreSlim _queueSlots;
    private readonly Thread[] _workers;
    private readonly EmbeddedNativeQueueFullMode _queueFullMode;
    private int _disposed;

    public EmbeddedNativeScheduler(ReindexerEmbeddedOptions options)
    {
        options ??= new ReindexerEmbeddedOptions();
        if (options.MaxNativeConcurrency <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), options.MaxNativeConcurrency, "MaxNativeConcurrency must be greater than zero.");
        }

        if (!Enum.IsDefined(typeof(EmbeddedNativeQueueFullMode), options.NativeQueueFullMode))
        {
            throw new ArgumentOutOfRangeException(nameof(options), options.NativeQueueFullMode, "NativeQueueFullMode is not supported.");
        }

        _queueFullMode = options.NativeQueueFullMode;
        _queueSlots = options.NativeQueueCapacity > 0
            ? new SemaphoreSlim(options.NativeQueueCapacity)
            : null;
        _workers = new Thread[options.MaxNativeConcurrency];
        for (var i = 0; i < _workers.Length; i++)
        {
            _workers[i] = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Name = $"ReindexerNet embedded native worker {i + 1}"
            };
            _workers[i].Start();
        }
    }

    public Task Run(Action action, CancellationToken cancellationToken)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return Run<object>(() =>
        {
            action();
            return null;
        }, cancellationToken);
    }

    public async Task<T> Run<T>(Func<T> action, CancellationToken cancellationToken)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        if (_queueSlots != null)
        {
            if (_queueFullMode == EmbeddedNativeQueueFullMode.Throw)
            {
                if (!_queueSlots.Wait(0))
                {
                    throw new InvalidOperationException("The embedded Reindexer native operation queue is full.");
                }
            }
            else
            {
                await _queueSlots.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        var slotAcquired = _queueSlots != null;
        try
        {
            ThrowIfDisposed();
            var item = new EmbeddedNativeWorkItem<T>(action, cancellationToken);
            _queue.Enqueue(item);
            slotAcquired = false;
            _items.Release();
            return await item.Task.ConfigureAwait(false);
        }
        finally
        {
            if (slotAcquired)
            {
                _queueSlots.Release();
            }
        }
    }

    private void WorkerLoop()
    {
        while (true)
        {
            _items.Wait();
            if (Volatile.Read(ref _disposed) != 0 && _queue.IsEmpty)
            {
                return;
            }

            if (!_queue.TryDequeue(out var item))
            {
                continue;
            }

            _queueSlots?.Release();
            item.Execute();
        }
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            throw new ObjectDisposedException(nameof(EmbeddedNativeScheduler));
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        while (_queue.TryDequeue(out var item))
        {
            _queueSlots?.Release();
            item.Cancel();
        }

        for (var i = 0; i < _workers.Length; i++)
        {
            _items.Release();
        }

        foreach (var worker in _workers)
        {
            if (!ReferenceEquals(worker, Thread.CurrentThread))
            {
                worker.Join();
            }
        }

        _items.Dispose();
        _queueSlots?.Dispose();
    }

    private interface IEmbeddedNativeWorkItem
    {
        void Execute();
        void Cancel();
    }

    private sealed class EmbeddedNativeWorkItem<T> : IEmbeddedNativeWorkItem
    {
        private readonly Func<T> _action;
        private readonly CancellationToken _cancellationToken;
        private readonly TaskCompletionSource<T> _completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public EmbeddedNativeWorkItem(Func<T> action, CancellationToken cancellationToken)
        {
            _action = action;
            _cancellationToken = cancellationToken;
        }

        public Task<T> Task => _completionSource.Task;

        public void Execute()
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                _completionSource.TrySetCanceled(_cancellationToken);
                return;
            }

            try
            {
                _completionSource.TrySetResult(_action());
            }
            catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested)
            {
                _completionSource.TrySetCanceled(_cancellationToken);
            }
            catch (Exception e)
            {
                _completionSource.TrySetException(e);
            }
        }

        public void Cancel()
        {
            _completionSource.TrySetException(new ObjectDisposedException(nameof(EmbeddedNativeScheduler)));
        }
    }
}
