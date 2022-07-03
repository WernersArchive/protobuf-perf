using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Benchmarks;
using Google.Protobuf;
using ProtoBuf.Meta;

#pragma warning disable CS0162 // Unreachable code detected

namespace ConsoleApp
{
    public partial class Program
    {

        private static void Main(string[] args)
        {
            var instance = new Program();
            instance.InitializeFromArguments(args);
            instance.Execute();
            //Console.WriteLine($"  Framework={System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            //Console.WriteLine($"  Runtime={System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}");
            //Console.WriteLine($"  ProcessorCount={System.Environment.ProcessorCount}");
            //Console.WriteLine($"  Objects={instObjectsForTesting.ToString("##,###,###,##0")}");
            //Console.WriteLine();
            //Console.WriteLine($"  ThreadLocalTypeModel={ThreadLocalTypeModel}");
            //Console.WriteLine($"  IsLockFree={IsLockFree}");
            //Console.WriteLine($"  InitializeInstance={InitializeInstance}");
            //Console.WriteLine();

            ////UTF Preparations

            //var Utf8TestData = new List<byte[]>();

            //if (!SkipUTF8)
            //{
            //    for (int i = 0; i < ObjectsForTesting; i++)
            //    {
            //        using (var ms = new MemoryStream())
            //        {
            //            var bytes = UTF8.Value.GetBytes("some text " + i.ToString());
            //            Utf8TestData.Add(bytes);
            //        }
            //    }
            //}

            ////protobuf-net preparations

            //var protobufNetTestData = new List<byte[]>();
            //for (int i = 0; i < ObjectsForTesting; i++)
            //{
            //    using (var ms = new MemoryStream())
            //    {
            //        ThreadRTM.Value.Serialize<Test>(ms, Test.Create(i.ToString()));
            //        protobufNetTestData.Add(ms.ToArray());
            //    }
            //}

            ////Google.Protobuf preparations
            //var googleProtobufTestData = new List<byte[]>();
            //if (!SkipGoogle)
            //{
            //    for (int i = 0; i < ObjectsForTesting; i++)
            //    {
            //        using (var ms = new MemoryStream())
            //        {
            //            Test.Create(i.ToString()).WriteTo(ms);
            //            var buffer = ms.ToArray();
            //            googleProtobufTestData.Add(buffer);
            //        }
            //    }
            //}

            //if (!SkipWarmup)
            //{
            //    var warmupOutput = new Test[ObjectsForTesting];

            //    //Warmup ProtoBuf-Net (without measuring)
            //    protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        warmupOutput[i] = NetDeserialize<Test>(x, InitializeInstance ? new Test() : null);
            //        return true;
            //    }).All(_ => _);
            //}
            //var output = new Test[ObjectsForTesting];

            //int firstMeasureColumn = 75;
            ////Start measurements for ProtoBuf-Net

            //int[] degreeOfParallelism = new int[] { 2, 4, 6, 8, 10, 12 };

            //if (Environment.ProcessorCount > 8)
            //{
            //    degreeOfParallelism = new int[] { 2, 4, 6, 8, 10, 12, 16, 20 };
            //}

            //var watch = new Stopwatch();
            //int index = 0;
            //if (!SkipNet)
            //{
            //    Console.Write("NET: without AsParallel (Deserialize):".PadRight(firstMeasureColumn));
            //    watch.Restart();
            //    foreach (var x in protobufNetTestData)
            //    {
            //        output[index++] = NetDeserialize<Test>(x, InitializeInstance ? new Test() : null);
            //    }
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            //    foreach (int parallelism in degreeOfParallelism)
            //    {
            //        Console.Write($"   NET: with AsParallel (Deserialize with DegreeOfParallelism={parallelism}):".PadRight(firstMeasureColumn));
            //        watch.Restart();
            //        protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).WithDegreeOfParallelism(parallelism).Select((x, i) =>
            //        {
            //            output[i] = NetDeserialize<Test>(x, InitializeInstance ? new Test() : null);
            //            return true;
            //        }).All(_ => _);
            //        Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));
            //    }

            //    Console.Write($"   NET: with AsParallel (Deserialize with DegreeOfP.=default):".PadRight(firstMeasureColumn));
            //    watch.Restart();
            //    protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        output[i] = NetDeserialize<Test>(x, InitializeInstance ? new Test() : null);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            //    Console.Write("   NET: with Parallel.ForEach (Deserialize):".PadRight(firstMeasureColumn));
            //    watch.Restart();
            //    Parallel.ForEach(protobufNetTestData, () => 0, (x, pls, index, s) =>
            //    {
            //        output[(int)index] = NetDeserialize<Test>(x, InitializeInstance ? new Test() : null);
            //        return 0;
            //    }, _ => { });
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));
            //}
            //if (!SkipState)
            //{
            //    watch.Restart();
            //    Console.Write($"   State: NO-OP with AsParallel:".PadRight(firstMeasureColumn));
            //    protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        var m = ProtoReaderStateBenchmark.StateDeserialize<Test>(x, ThreadRTM.Value, true, InitializeInstance ? new Test() : null);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            //    watch.Restart();
            //    Console.Write($"   State: with AsParallel:".PadRight(firstMeasureColumn));
            //    protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        var m = ProtoReaderStateBenchmark.StateDeserialize<Test>(x, ThreadRTM.Value, false, InitializeInstance ? new Test() : null);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));
            //}

            //if (!SkipUTF8)
            //{
            //    System.Threading.Thread.Sleep(3000);

            //    watch.Restart();

            //    Console.Write($"   UTF8: NOT Parallel:".PadRight(firstMeasureColumn));
            //    for (int ii = 0; ii < ObjectsForTesting; ii++)
            //    {
            //        var x = Utf8TestData[ii];
            //        var m = Utf8Deserialize<Test>(x, false, InitializeInstance ? new Test() : null);
            //    }

            //    //Utf8TestData.Select((x, i) =>
            //    //{
            //    //    var m = Utf8Deserialize<Test>(x, false, InitializeInstance ? new Test() : null);
            //    //    return true;
            //    //}).All(_ => _);

            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            //    //            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(buffer);

            //    watch.Restart();
            //    Console.Write($"   UTF8-SPAN: NOT Parallel:".PadRight(firstMeasureColumn));
            //    var theSpan = new ReadOnlySpan<byte>(Utf8TestData.First());
            //    ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(Utf8TestData.First());

            //    for (int iii = 0; iii < ObjectsForTesting; iii++)
            //    {
            //        var m = Utf8SpanDeserialize<Test>(span, InitializeInstance ? new Test() : null);
            //    }

            //    //Utf8TestData.Select((x, i) =>
            //    //{
            //    //    var m = Utf8SpanDeserialize<Test>(span, InitializeInstance ? new Test() : null);
            //    //    return true;
            //    //}).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));
            //}

            //if (!SkipGoogle)
            //{
            //    var googleOutput = new Test[ObjectsForTesting];
            //    //Warmup Google.Protobuf (without measuring)
            //    googleProtobufTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        googleOutput[i] = GoogleDeserialize<Test>(x);
            //        return true;
            //    }).All(_ => _);

            //    //Start measurements for Google.Protobuf

            //    index = 0;
            //    Console.Write("Google: without AsParallel (Deserialize): ");
            //    watch.Restart();
            //    foreach (var x in googleProtobufTestData)
            //    {
            //        output[index++] = GoogleDeserialize<Test>(x);
            //    }
            //    Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            //    Console.Write($"   Google: with AsParallel (Deserialize with DegreeOfParallelism=default): ");
            //    watch.Restart();
            //    googleProtobufTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        output[i] = GoogleDeserialize<Test>(x);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            //    foreach (int parallelism in degreeOfParallelism)
            //    {
            //        Console.Write($"   Google: with AsParallel (Deserialize with DegreeOfParallelism={parallelism}): ");
            //        watch.Restart();
            //        googleProtobufTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).WithDegreeOfParallelism(parallelism).Select((x, i) =>
            //        {
            //            output[i] = GoogleDeserialize<Test>(x);
            //            return true;
            //        }).All(_ => _);
            //        Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");
            //    }

            //    Console.Write("   Google: with Parallel.ForEach (Deserialize): ");
            //    watch.Restart();
            //    Parallel.ForEach(googleProtobufTestData, () => 0, (x, pls, index, s) =>
            //    {
            //        output[(int)index] = GoogleDeserialize<Test>(x);
            //        return 0;
            //    }, _ => { });
            //    Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");
            //}
            //if (!SkipOthers)
            //{
            //    Console.Write("without AsParallel (DummyWork):".PadRight(firstMeasureColumn));
            //    watch.Restart();
            //    protobufNetTestData.Select((x, i) =>
            //    {
            //        output[i] = DummyWork<Test>(x);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            //    Console.Write("   with AsParallel (DummyWork):".PadRight(firstMeasureColumn));
            //    watch.Restart();
            //    protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        output[i] = DummyWork<Test>(x);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            //    Console.Write("without AsParallel (ConstructionWork):".PadRight(firstMeasureColumn));
            //    watch.Restart();
            //    protobufNetTestData.Select((x, i) =>
            //    {
            //        output[i] = ConstructionWork(x);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            //    Console.Write("   with AsParallel (ConstructionWork):".PadRight(firstMeasureColumn));
            //    watch.Restart();
            //    protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        output[i] = ConstructionWork(x);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            //    Console.Write("   with AsParallel (GenericConstructionWork):".PadRight(firstMeasureColumn));
            //    watch.Restart();
            //    protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        output[i] = GenericConstructionWork<Test>(x);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));
            //}
        }

    }
}