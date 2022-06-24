using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using ProtoBuf;
using ProtoBuf.Meta;

#pragma warning disable CS0162 // Unreachable code detected

namespace ConsoleApp
{
    public partial class Program
    {
        private const int ObjectsForTesting = 2000000;
        private const bool SkipGoogle = true;
        private const bool SkipOthers = false;

        private static string ToMeasureString(Stopwatch watch, long operations, int padleft = 14)
        {
            // watch.ElapsedMilliseconds.ToString() + "ms"
            double opsPerSecond = operations / watch.Elapsed.TotalSeconds / 1000;
            string s = opsPerSecond.ToString("####0 K-Ops/Sec");
            if (padleft > 0)
            {
                s = s.PadLeft(padleft, ' ');
            }
            return s;
        }

        public static bool ThreadLocalTypeModel { get; set; } = true;

        private static void Main(string[] args)
        {
            Console.WriteLine("Protobuf-Net Performance Investigations v1.4.0");
            Console.WriteLine("  " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription + " on " + System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier + " with " + Environment.ProcessorCount.ToString() + " cores");
            Console.WriteLine($"  Objects: {ObjectsForTesting.ToString("##,###,###,##0")}");
            Console.WriteLine();
            foreach (string s in args)
            {
                var splits = s.Split(new char[] { '=' });
                if (splits.Length == 2)
                {
                    if (string.Equals(splits[0], "ThreadLocalTypeModel", StringComparison.OrdinalIgnoreCase))
                    {
                        ThreadLocalTypeModel = bool.Parse(splits[1]);
                    }
                }
            }

            if (ThreadLocalTypeModel)
            {
                Console.WriteLine("  One TypeModel per Thread");
            }
            else
            {
                Console.WriteLine("  One SHARED TypeModel for all Threads");
            }
            Console.WriteLine();

            //protobuf-net preparations
            Serializer.PrepareSerializer<Test>();
            var protobufNetTestData = new List<byte[]>();
            for (int i = 0; i < ObjectsForTesting; i++)
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize<Test>(ms, Test.Create(i.ToString()));
                    protobufNetTestData.Add(ms.ToArray());
                }
            }

            //Google.Protobuf preparations
            var googleProtobufTestData = new List<byte[]>();
            if (!SkipGoogle)
            {
                for (int i = 0; i < ObjectsForTesting; i++)
                {
                    using (var ms = new MemoryStream())
                    {
                        Test.Create(i.ToString()).WriteTo(ms);
                        var buffer = ms.ToArray();
                        googleProtobufTestData.Add(buffer);
                    }
                }
            }
            var warmupOutput = new Test[ObjectsForTesting];

            //Warmup ProtoBuf-Net (without measuring)
            protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            {
                warmupOutput[i] = NetDeserialize<Test>(x);
                return true;
            }).All(_ => _);

            var output = new Test[ObjectsForTesting];

            int firstMeasureColumn = 70;
            //Start measurements for ProtoBuf-Net

            int[] degreeOfParallelism = new int[] { 2, 4, 6, 8, 10, 12 };

            if (Environment.ProcessorCount > 8)
            {
                degreeOfParallelism = new int[] { 2, 4, 6, 8, 10, 12, 16, 20 };
            }

            var watch = new Stopwatch();
            int index = 0;
            Console.Write("NET: without AsParallel (Deserialize):".PadRight(firstMeasureColumn));
            watch.Restart();
            foreach (var x in protobufNetTestData)
            {
                output[index++] = NetDeserialize<Test>(x);
            }
            Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            foreach (int parallelism in degreeOfParallelism)
            {
                Console.Write($"   NET: with AsParallel (Deserialize with DegreeOfParallelism={parallelism}):".PadRight(firstMeasureColumn));
                watch.Restart();
                protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).WithDegreeOfParallelism(parallelism).Select((x, i) =>
                {
                    output[i] = NetDeserialize<Test>(x);
                    return true;
                }).All(_ => _);
                Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));
            }

            Console.Write($"   NET: with AsParallel (Deserialize with DegreeOfP.=default):".PadRight(firstMeasureColumn));
            watch.Restart();
            protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            {
                output[i] = NetDeserialize<Test>(x);
                return true;
            }).All(_ => _);
            Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            Console.Write("   NET: with Parallel.ForEach (Deserialize):".PadRight(firstMeasureColumn));
            watch.Restart();
            Parallel.ForEach(protobufNetTestData, () => 0, (x, pls, index, s) =>
            {
                output[(int)index] = NetDeserialize<Test>(x);
                return 0;
            }, _ => { });
            Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

            if (!SkipGoogle)
            {
                //Warmup Google.Protobuf (without measuring)
                googleProtobufTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
                {
                    warmupOutput[i] = GoogleDeserialize<Test>(x);
                    return true;
                }).All(_ => _);

                //Start measurements for Google.Protobuf

                index = 0;
                Console.Write("Google: without AsParallel (Deserialize): ");
                watch.Restart();
                foreach (var x in googleProtobufTestData)
                {
                    output[index++] = GoogleDeserialize<Test>(x);
                }
                Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

                Console.Write($"   Google: with AsParallel (Deserialize with DegreeOfParallelism=default): ");
                watch.Restart();
                googleProtobufTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
                {
                    output[i] = GoogleDeserialize<Test>(x);
                    return true;
                }).All(_ => _);
                Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

                foreach (int parallelism in degreeOfParallelism)
                {
                    Console.Write($"   Google: with AsParallel (Deserialize with DegreeOfParallelism={parallelism}): ");
                    watch.Restart();
                    googleProtobufTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).WithDegreeOfParallelism(parallelism).Select((x, i) =>
                    {
                        output[i] = GoogleDeserialize<Test>(x);
                        return true;
                    }).All(_ => _);
                    Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");
                }

                Console.Write("   Google: with Parallel.ForEach (Deserialize): ");
                watch.Restart();
                Parallel.ForEach(googleProtobufTestData, () => 0, (x, pls, index, s) =>
                {
                    output[(int)index] = GoogleDeserialize<Test>(x);
                    return 0;
                }, _ => { });
                Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");
            }
            if (!SkipOthers)
            {
                Console.Write("without AsParallel (DummyWork):".PadRight(firstMeasureColumn));
                watch.Restart();
                protobufNetTestData.Select((x, i) =>
                {
                    output[i] = DummyWork<Test>(x);
                    return true;
                }).All(_ => _);
                Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

                Console.Write("   with AsParallel (DummyWork):".PadRight(firstMeasureColumn));
                watch.Restart();
                protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
                {
                    output[i] = DummyWork<Test>(x);
                    return true;
                }).All(_ => _);
                Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

                Console.Write("without AsParallel (ConstructionWork):".PadRight(firstMeasureColumn));
                watch.Restart();
                protobufNetTestData.Select((x, i) =>
                {
                    output[i] = ConstructionWork(x);
                    return true;
                }).All(_ => _);
                Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

                Console.Write("   with AsParallel (ConstructionWork):".PadRight(firstMeasureColumn));
                watch.Restart();
                protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
                {
                    output[i] = ConstructionWork(x);
                    return true;
                }).All(_ => _);
                Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));

                Console.Write("   with AsParallel (GenericConstructionWork):".PadRight(firstMeasureColumn));
                watch.Restart();
                protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
                {
                    output[i] = GenericConstructionWork<Test>(x);
                    return true;
                }).All(_ => _);
                Console.WriteLine(ToMeasureString(watch, ObjectsForTesting));
            }
        }

        private static ThreadLocal<RuntimeTypeModel> ThreadRTM = new ThreadLocal<RuntimeTypeModel>(() =>
        {
            if (ThreadLocalTypeModel)
            {
                var rt = RuntimeTypeModel.Create($"ThreadID={Environment.CurrentManagedThreadId}");
                rt.Add(typeof(Test), true);
                //Console.WriteLine($"RuntimeTypeModel: {rt.ToString()}");
                return rt;
            }
            else
            {
                return RuntimeTypeModel.Default;
            }
        });

        private static T NetDeserialize<T>(byte[] buffer)
        {
            var model = ThreadRTM.Value; //RuntimeTypeModel.Default
            return model.Deserialize<T>((ReadOnlySpan<byte>)buffer, default(T), null);
        }

        private static Test GoogleDeserialize<T>(byte[] buffer)
        {
            return Test.Parser.ParseFrom(buffer);
        }

        private static T DummyWork<T>(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                for (int i = 0; i < 10000; i++)
                {
                    var x = 100 / 10;
                }
                return default(T);
            }
        }

        private static Test ConstructionWork(byte[] buffer)
        {
            for (int i = 0; i < 1000; i++)
            {
                var x = new Test();
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