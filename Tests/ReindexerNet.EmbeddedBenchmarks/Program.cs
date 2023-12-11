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
    public static void Main(string[] args)
    {
#if DEBUG
        var b = new SelectSinglePK{ N = 1000 };
        b.RealmSetup();
        b.Realm();
        b.RealmClean();
#else
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#endif
    }
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