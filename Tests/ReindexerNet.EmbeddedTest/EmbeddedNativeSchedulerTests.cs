using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReindexerNet.Embedded;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReindexerNet.EmbeddedTest;

[TestClass]
public class EmbeddedNativeSchedulerTests
{
    [TestMethod]
    public async Task ThrowModeRejectsWhenQueueIsFull()
    {
        using var scheduler = new EmbeddedNativeScheduler(new ReindexerEmbeddedOptions
        {
            MaxNativeConcurrency = 1,
            NativeQueueCapacity = 1,
            NativeQueueFullMode = EmbeddedNativeQueueFullMode.Throw
        });
        using var workerStarted = new ManualResetEventSlim();
        using var releaseWorker = new ManualResetEventSlim();

        var running = scheduler.Run(() =>
        {
            workerStarted.Set();
            releaseWorker.Wait();
        }, CancellationToken.None);

        Assert.IsTrue(workerStarted.Wait(TimeSpan.FromSeconds(5)));

        var queued = scheduler.Run(() => { }, CancellationToken.None);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await scheduler.Run(() => { }, CancellationToken.None));

        releaseWorker.Set();
        await running;
        await queued;
    }

    [TestMethod]
    public async Task WaitModeBackpressuresWithoutCompletingQueuedWork()
    {
        using var scheduler = new EmbeddedNativeScheduler(new ReindexerEmbeddedOptions
        {
            MaxNativeConcurrency = 1,
            NativeQueueCapacity = 1,
            NativeQueueFullMode = EmbeddedNativeQueueFullMode.Wait
        });
        using var workerStarted = new ManualResetEventSlim();
        using var releaseWorker = new ManualResetEventSlim();
        var completed = 0;

        var running = scheduler.Run(() =>
        {
            workerStarted.Set();
            releaseWorker.Wait();
        }, CancellationToken.None);

        Assert.IsTrue(workerStarted.Wait(TimeSpan.FromSeconds(5)));

        var queued = scheduler.Run(() => Interlocked.Increment(ref completed), CancellationToken.None);
        var waiting = scheduler.Run(() => Interlocked.Increment(ref completed), CancellationToken.None);

        await Task.Delay(50);

        Assert.IsFalse(waiting.IsCompleted);

        releaseWorker.Set();
        await running;
        await queued;
        await waiting;
        Assert.AreEqual(2, completed);
    }

    [TestMethod]
    public async Task PreCanceledRunDoesNotExecuteAction()
    {
        using var scheduler = new EmbeddedNativeScheduler(new ReindexerEmbeddedOptions());
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var executed = false;

        await Assert.ThrowsExceptionAsync<OperationCanceledException>(
            async () => await scheduler.Run(() => executed = true, cts.Token));

        Assert.IsFalse(executed);
    }
}
