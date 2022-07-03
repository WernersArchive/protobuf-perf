﻿using System;
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
        public Program()
        {
            //this.ThreadRTM = new ThreadLocal<RuntimeTypeModel>(ThreadFactory);
        }

        public void InitializeFromArguments(string[] args)
        {
            foreach (string s in args)
            {
                var splits = s.Split(new char[] { '=' });
                if (splits.Length == 2)
                {
                    if (string.Equals(splits[0], "InitializeInstance", StringComparison.OrdinalIgnoreCase))
                    {
                        this.InitializeInstance = bool.Parse(splits[1]);
                    }

                    if (string.Equals(splits[0], "ThreadLocalTypeModel", StringComparison.OrdinalIgnoreCase))
                    {
                        this.ThreadLocalTypeModel = bool.Parse(splits[1]);
                    }

                    if (string.Equals(splits[0], "Objects", StringComparison.OrdinalIgnoreCase))
                    {
                        this.ObjectsForTesting = int.Parse(splits[1]) * 1000;
                    }

                    if (string.Equals(splits[0], "IsLockFree", StringComparison.OrdinalIgnoreCase))
                    {
                        this.IsLockFree = bool.Parse(splits[1]);
                    }
                }
            }
        }

        private const bool SkipState = true;
        private const bool SkipNet = true;
        private const bool SkipUTF8 = false;
        private const bool SkipGoogle = true;
        private const bool SkipWarmup = false;

        /// <summary>
        /// means dummy and construction
        /// </summary>
        private const bool SkipOthers = true;

        private static string ToMeasureString(Stopwatch watch, long operations, int padleft = 17)
        {
            // watch.ElapsedMilliseconds.ToString() + "ms"
            double opsPerSecond = operations / watch.Elapsed.TotalSeconds / 1000;
            string s = opsPerSecond.ToString("##,##0 K-Ops/Sec");
            if (padleft > 0)
            {
                s = s.PadLeft(padleft, ' ');
            }
            return s;
        }

        private bool HasWrittenLogo = false;

        private void ShowLogo()
        {
            Debug.Assert(!HasWrittenLogo);
            Console.WriteLine("Protobuf-Net Performance Investigations v2.0.0-alpha02");
            Console.WriteLine();
            HasWrittenLogo = true;
        }

        internal void ExecuteBenchmark(Action benchmark, long items, string name)
        {
            //1. warmup!
            benchmark();
            Console.Write($"    {name}".PadRight(75));
            var sw = Stopwatch.StartNew();
            benchmark();
            sw.Stop();
            Console.WriteLine(ToMeasureString(sw, items));
        }

        private void Execute()
        {
            if ((!this.NoLogo) && (!HasWrittenLogo))
            {
                ShowLogo();
            }

            Console.WriteLine($"  Framework={System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"  Runtime={System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}");
            Console.WriteLine($"  ProcessorCount={System.Environment.ProcessorCount}");
            Console.WriteLine($"  Objects={ObjectsForTesting.ToString("##,###,###,##0")}");
            Console.WriteLine();
            Console.WriteLine($"  ThreadLocalTypeModel={ThreadLocalTypeModel}");
            Console.WriteLine($"  IsLockFree={IsLockFree}");
            Console.WriteLine($"  InitializeInstance={InitializeInstance}");
            Console.WriteLine();

            //var benchmark = new ProtoReaderStateBenchmark
            //{
            //    Iterations = ObjectsForTesting,
            //    ShortCall = false
            //};
            //benchmark.Setup();
            //ExecuteBenchmark(benchmark.ExecuteAsSpanInParallel, ObjectsForTesting, nameof(benchmark.ExecuteAsSpanInParallel));
            //ExecuteBenchmark(benchmark.ExecuteAsSpanInSerial, ObjectsForTesting, nameof(benchmark.ExecuteAsSpanInSerial));

            var netLongBenchmark = new NetLongDeserializerBenchmark()
            {
                Iterations = ObjectsForTesting,
                InitializeInstance = true,
                DegreeOfParallelism = null
            };

            int?[] parallelism = new int?[] { 2, 4, 6, 8, null };
            if (Environment.ProcessorCount > 8)
            {
                parallelism = new int?[] { 2, 4, 6, 8, 12, 16, 20, null };
            }

            netLongBenchmark.Setup();
            ExecuteBenchmark(netLongBenchmark.LongMessageDeserializerSerial, ObjectsForTesting, $"{nameof(netLongBenchmark.LongMessageDeserializerSerial)}");
            foreach (var degree in parallelism)
            {
                netLongBenchmark.DegreeOfParallelism = degree;
                ExecuteBenchmark(netLongBenchmark.LongMessageDeserializerParallel, ObjectsForTesting, $"{nameof(netLongBenchmark.LongMessageDeserializerParallel)}  ({netLongBenchmark.SettingsTrace()})");
            }

            var netStringBenchmark = new NetStringDeserializerBenchmark()
            {
                Iterations = ObjectsForTesting,
                InitializeInstance = true,
                DegreeOfParallelism = null
            };

            netStringBenchmark.Setup();
            ExecuteBenchmark(netStringBenchmark.StringMessageDeserializerSerial, ObjectsForTesting, $"{nameof(netStringBenchmark.StringMessageDeserializerSerial)}");

            foreach (var degree in parallelism)
            {
                netStringBenchmark.DegreeOfParallelism = degree;
                ExecuteBenchmark(netStringBenchmark.StringMessageDeserializerParallel, ObjectsForTesting, $"{nameof(netStringBenchmark.StringMessageDeserializerParallel)}  ({netStringBenchmark.SettingsTrace()})");
            }

            return;

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
            //        ThreadRTM.Value.Serialize<StringMessage>(ms, StringMessage.Create(i.ToString()));
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
            //            StringMessage.Create(i.ToString()).WriteTo(ms);
            //            var buffer = ms.ToArray();
            //            googleProtobufTestData.Add(buffer);
            //        }
            //    }
            //}

            //if (!SkipWarmup)
            //{
            //    var warmupOutput = new StringMessage[ObjectsForTesting];

            //    //Warmup ProtoBuf-Net (without measuring)
            //    protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        warmupOutput[i] = NetDeserialize<StringMessage>(x, InitializeInstance ? new StringMessage() : null);
            //        return true;
            //    }).All(_ => _);
            //}
            //var output = new StringMessage[ObjectsForTesting];

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
            //        output[index++] = NetDeserialize<StringMessage>(x, InitializeInstance ? new StringMessage() : null);
            //    }
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            //    foreach (int parallelism in degreeOfParallelism)
            //    {
            //        Console.Write($"   NET: with AsParallel (Deserialize with DegreeOfParallelism={parallelism}):".PadRight(firstMeasureColumn));
            //        watch.Restart();
            //        protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).WithDegreeOfParallelism(parallelism).Select((x, i) =>
            //        {
            //            output[i] = NetDeserialize<StringMessage>(x, InitializeInstance ? new StringMessage() : null);
            //            return true;
            //        }).All(_ => _);
            //        Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));
            //    }

            //    Console.Write($"   NET: with AsParallel (Deserialize with DegreeOfP.=default):".PadRight(firstMeasureColumn));
            //    watch.Restart();
            //    protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        output[i] = NetDeserialize<StringMessage>(x, InitializeInstance ? new StringMessage() : null);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            //    Console.Write("   NET: with Parallel.ForEach (Deserialize):".PadRight(firstMeasureColumn));
            //    watch.Restart();
            //    Parallel.ForEach(protobufNetTestData, () => 0, (x, pls, index, s) =>
            //    {
            //        output[(int)index] = NetDeserialize<StringMessage>(x, InitializeInstance ? new StringMessage() : null);
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
            //        var m = ProtoReaderStateBenchmark.StateDeserialize<StringMessage>(x, ThreadRTM.Value, true, InitializeInstance ? new StringMessage() : null);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            //    watch.Restart();
            //    Console.Write($"   State: with AsParallel:".PadRight(firstMeasureColumn));
            //    protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        var m = ProtoReaderStateBenchmark.StateDeserialize<StringMessage>(x, ThreadRTM.Value, false, InitializeInstance ? new StringMessage() : null);
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
            //        var m = Utf8Deserialize<StringMessage>(x, false, InitializeInstance ? new StringMessage() : null);
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
            //        var m = Utf8SpanDeserialize<StringMessage>(span, InitializeInstance ? new StringMessage() : null);
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
            //    var googleOutput = new StringMessage[ObjectsForTesting];
            //    //Warmup Google.Protobuf (without measuring)
            //    googleProtobufTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        googleOutput[i] = GoogleDeserialize<StringMessage>(x);
            //        return true;
            //    }).All(_ => _);

            //    //Start measurements for Google.Protobuf

            //    index = 0;
            //    Console.Write("Google: without AsParallel (Deserialize): ");
            //    watch.Restart();
            //    foreach (var x in googleProtobufTestData)
            //    {
            //        output[index++] = GoogleDeserialize<StringMessage>(x);
            //    }
            //    Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            //    Console.Write($"   Google: with AsParallel (Deserialize with DegreeOfParallelism=default): ");
            //    watch.Restart();
            //    googleProtobufTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        output[i] = GoogleDeserialize<StringMessage>(x);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            //    foreach (int parallelism in degreeOfParallelism)
            //    {
            //        Console.Write($"   Google: with AsParallel (Deserialize with DegreeOfParallelism={parallelism}): ");
            //        watch.Restart();
            //        googleProtobufTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).WithDegreeOfParallelism(parallelism).Select((x, i) =>
            //        {
            //            output[i] = GoogleDeserialize<StringMessage>(x);
            //            return true;
            //        }).All(_ => _);
            //        Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");
            //    }

            //    Console.Write("   Google: with Parallel.ForEach (Deserialize): ");
            //    watch.Restart();
            //    Parallel.ForEach(googleProtobufTestData, () => 0, (x, pls, index, s) =>
            //    {
            //        output[(int)index] = GoogleDeserialize<StringMessage>(x);
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
            //        output[i] = DummyWork<StringMessage>(x);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            //    Console.Write("   with AsParallel (DummyWork):".PadRight(firstMeasureColumn));
            //    watch.Restart();
            //    protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            //    {
            //        output[i] = DummyWork<StringMessage>(x);
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
            //        output[i] = GenericConstructionWork<StringMessage>(x);
            //        return true;
            //    }).All(_ => _);
            //    Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));
            //}
        }

        //private RuntimeTypeModel ThreadFactory()
        //{
        //    RuntimeTypeModel rt;
        //    if (ThreadLocalTypeModel)
        //    {
        //        //TODO LockFree impl removed
        //        //rt = RuntimeTypeModel.Create($"ThreadID={Environment.CurrentManagedThreadId}", lockFree: IsLockFree);
        //        rt = RuntimeTypeModel.Create($"ThreadID={Environment.CurrentManagedThreadId}");
        //        rt.Add(typeof(Test), true);
        //    }
        //    else
        //    {
        //        rt = RuntimeTypeModel.Default;
        //    }
        //    rt[typeof(Test)].CompileInPlace();
        //    return rt;

        //}

        //private readonly ThreadLocal<RuntimeTypeModel> ThreadRTM;

        //private T NetDeserialize<T>(byte[] buffer, T initvalue)
        //{
        //    var model = ThreadRTM.Value; //RuntimeTypeModel.Default
        //    return model.Deserialize<T>((ReadOnlySpan<byte>)buffer, initvalue, typeof(T));
        //}

        private static ThreadLocal<System.Text.UTF8Encoding> UTF8 = new ThreadLocal<System.Text.UTF8Encoding>(() =>
        {
            return new System.Text.UTF8Encoding();
        });

        private static T Utf8SpanDeserialize<T>(ReadOnlySpan<byte> span, T initvalue)
        {
            var s = UTF8.Value.GetString(span);
            return initvalue;
        }

        private static T Utf8Deserialize<T>(byte[] buffer, bool noOp, T initvalue)
        {
            var s = UTF8.Value.GetString(buffer, 0, buffer.Length);
            return initvalue;
        }

        private static StringMessage GoogleDeserialize<T>(byte[] buffer)
        {
            return StringMessage.Parser.ParseFrom(buffer);
        }

        private static T DummyWork<T>(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                for (int i = 0; i < 10000; i++)
                {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
                    var x = 100 / 10;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
                }
                return default(T);
            }
        }

        private static StringMessage ConstructionWork(byte[] buffer)
        {
            for (int i = 0; i < 1000; i++)
            {
                var x = new StringMessage();
            }
            return null;
        }

        private static T GenericConstructionWork<T>(byte[] buffer) where T : class, new()
        {
            for (int i = 0; i < 1000; i++)
            {
                var x = new T();
            }
            return default(T);
        }
    }
}