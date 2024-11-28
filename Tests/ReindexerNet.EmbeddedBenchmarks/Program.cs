using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
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

public class AntiVirusFriendlyConfig : ManualConfig
{
    public AntiVirusFriendlyConfig()
    {
        AddJob(
            Job.MediumRun
            .WithToolchain(InProcessNoEmitToolchain.Instance));
    }
}