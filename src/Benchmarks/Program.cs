using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    public static class Program
    {
        private static IConfig GetGlobalConfig()
        {
            var result = DefaultConfig.Instance;
            result.AddJob(Job.ShortRun.AsDefault());
            return result;
        }

        private static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("ProtoBuf-Net Benchmarks v0.2.0");
            Console.ResetColor();
            Console.WriteLine("");
            BenchmarkSwitcher.FromTypes(new System.Type[] { typeof(ProtoReaderStateBenchmark) }).Run(args, GetGlobalConfig());
        }
    }
}