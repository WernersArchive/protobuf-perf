using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using ProtoBuf;

namespace ConsoleApp
{
    public partial class Program
    {
        private const int ObjectsForTesting = 1000000;

        private static void Main(string[] args)
        {
            Console.WriteLine("Protobuf-Net Performance Investigations v1.2.0");
            Console.WriteLine("  " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription + " on " + System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier + " with " + Environment.ProcessorCount.ToString() + " cores");
            Console.WriteLine($"  Objects: {ObjectsForTesting}");
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
            for (int i = 0; i < ObjectsForTesting; i++)
            {
                using (var ms = new MemoryStream())
                {
                    Test.Create(i.ToString()).WriteTo(ms);
                    var buffer = ms.ToArray();
                    googleProtobufTestData.Add(buffer);
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

            //Start measurements for ProtoBuf-Net
            int[] degreeOfParallelism = new int[] { 2, 4, 6, 8, 10, 12, 16 };
            var watch = new Stopwatch();
            int index = 0;
            Console.Write("NET: without AsParallel (Deserialize): ");
            watch.Restart();
            foreach (var x in protobufNetTestData)
            {
                output[index++] = NetDeserialize<Test>(x);
            }
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            Console.Write($"   NET: with AsParallel (Deserialize with DegreeOfParallelism=default): ");
            watch.Restart();
            protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            {
                output[i] = NetDeserialize<Test>(x);
                return true;
            }).All(_ => _);
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            foreach (int parallelism in degreeOfParallelism)
            {
                Console.Write($"   NET: with AsParallel (Deserialize with DegreeOfParallelism={parallelism}): ");
                watch.Restart();
                protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).WithDegreeOfParallelism(parallelism).Select((x, i) =>
                {
                    output[i] = NetDeserialize<Test>(x);
                    return true;
                }).All(_ => _);
                Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");
            }

            Console.Write("   NET: with Parallel.ForEach (Deserialize): ");
            watch.Restart();
            Parallel.ForEach(protobufNetTestData, () => 0, (x, pls, index, s) =>
            {
                output[(int)index] = NetDeserialize<Test>(x);
                return 0;
            }, _ => { });
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            System.Threading.Thread.Sleep(300); // Rest a little bit!

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

            Console.Write("without AsParallel (DummyWork): ");
            watch.Restart();
            protobufNetTestData.Select((x, i) =>
            {
                output[i] = DummyWork<Test>(x);
                return true;
            }).All(_ => _);
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            Console.Write("   with AsParallel (DummyWork): ");
            watch.Restart();
            protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            {
                output[i] = DummyWork<Test>(x);
                return true;
            }).All(_ => _);
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            Console.Write("without AsParallel (ConstructionWork): ");
            watch.Restart();
            protobufNetTestData.Select((x, i) =>
            {
                output[i] = ConstructionWork(x);
                return true;
            }).All(_ => _);
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            Console.Write("   with AsParallel (ConstructionWork): ");
            watch.Restart();
            protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            {
                output[i] = ConstructionWork(x);
                return true;
            }).All(_ => _);
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            Console.Write("   with AsParallel (GenericConstructionWork): ");
            watch.Restart();
            protobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            {
                output[i] = GenericConstructionWork<Test>(x);
                return true;
            }).All(_ => _);
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");
        }

        private static T NetDeserialize<T>(byte[] buffer)
        {
            return Serializer.Deserialize<T>((ReadOnlySpan<byte>)buffer);
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