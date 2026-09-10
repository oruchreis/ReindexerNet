using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using BenchmarkDotNet.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReindexerNetBenchmark.EmbeddedBenchmarks;

public class Program
{

#if DEBUG
    public static async Task Main(string[] args)
    {
        var b = new SelectSinglePK { N = 1000 };
        await b.RealmSetupAsync();
        b.Realm();
        b.RealmClean();
    }
#else
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
#endif
}

/// <summary>
/// Runs benchmarks in-process (no subprocess spawn) to avoid Windows Defender
/// killing the short-lived child processes that BenchmarkDotNet normally creates.
/// Uses the same iteration parameters as the [SimpleJob] it replaces.
/// Column providers, logger, and validator are added explicitly because ManualConfig
/// does not inherit DefaultConfig's infrastructure.
/// </summary>
public class AntiVirusFriendlyConfig : ManualConfig
{
    public AntiVirusFriendlyConfig()
    {
        AddJob(
            Job.Default
                .WithToolchain(InProcessNoEmitToolchain.Instance)
                .WithLaunchCount(0)
                .WithWarmupCount(0)
                .WithIterationCount(1));
        AddColumnProvider(DefaultColumnProviders.Instance);
        AddLogger(ConsoleLogger.Default);
        AddValidator(JitOptimizationsValidator.DontFailOnError);
    }
}