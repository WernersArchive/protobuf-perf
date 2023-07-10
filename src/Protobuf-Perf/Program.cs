using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;

namespace ConsoleApp
{
    public class Program
    {
        private const int LongPayload = 1000;
        private const int Operations = 50000;
        private static void Main(string[] args)
        {
            Console.WriteLine("Protobuf-Net Performance Investigations v1.0.0");
            Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription + " on " + System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier + " with " + Environment.ProcessorCount.ToString() + " cores");
            Console.WriteLine();
            Serializer.PrepareSerializer<Test>();
            var list = new List<byte[]>();
            for (int i = 0; i < Operations; i++)
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize<Test>(ms, Test.Create(i.ToString(), LongPayload));
                    list.Add(ms.ToArray());
                }
            }
            var output = new Test[list.Count];
            //Warmup (without measuring)
            list.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            {
                output[i] = Deserialize<Test>(x);
                return true;
            }).All(_ => _);

            var watch = new Stopwatch();
            int index = 0;
            Console.Write("without AsParallel (Deserialize): ");
            watch.Restart();
            foreach (var x in list)
            {
                output[index++] = Deserialize<Test>(x);
            }
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            Console.Write($"   with AsParallel (Deserialize with DegreeOfParallelism=default): ");
            watch.Restart();
            list.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            {
                output[i] = Deserialize<Test>(x);
                return true;
            }).All(_ => _);
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");
            int[] degreeOfParallelism = new int[] { 2, 4, 6, 8, 10, 12, 16 };

            foreach (int parallelism in degreeOfParallelism)
            {
                Console.Write($"   with AsParallel (Deserialize with DegreeOfParallelism={parallelism}): ");
                watch.Restart();
                list.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).WithDegreeOfParallelism(parallelism).Select((x, i) =>
                {
                    output[i] = Deserialize<Test>(x);
                    return true;
                }).All(_ => _);
                Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");
            }

            Console.Write("   with Parallel.ForEach (Deserialize): ");
            watch.Restart();
            Parallel.ForEach(list, () => 0, (x, pls, index, s) =>
            {
                output[(int)index] = Deserialize<Test>(x);
                return 0;
            }, _ => { });
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            Console.Write("without AsParallel (DummyWork): ");
            watch.Restart();
            list.Select((x, i) =>
            {
                output[i] = DummyWork<Test>(x);
                return true;
            }).All(_ => _);
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");

            Console.Write("   with AsParallel (DummyWork): ");
            watch.Restart();
            list.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            {
                output[i] = DummyWork<Test>(x);
                return true;
            }).All(_ => _);
            Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");
        }

        private static T Deserialize<T>(byte[] buffer)
        {
            return Serializer.Deserialize<T>((ReadOnlySpan<byte>)buffer);
        }

        private static T DummyWork<T>(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                for (int i = 0; i < 10000; i++) { var x = 100 / 10; }
                return default(T);
            }
        }

        [ProtoContract]
        public sealed class Test
        {
            public static Test Create(string suffix, int numberOfLongs )
            {
                long[] list = new long[numberOfLongs];
                for (int i = 0; i < numberOfLongs; i++)
                {
                    list[i] = Random.Shared.Next();
                };

                return new Test()
                {
                    Longs = new List<long>(list),
                    Value = "some text " + suffix,
                };
            }

            [ProtoMember(1)]
            public string Value;

            /// <summary>
            ///
            /// </summary>
            [global::ProtoBuf.ProtoMember(2, Name = @"longs", DataFormat = global::ProtoBuf.DataFormat.ZigZag, Options = global::ProtoBuf.MemberSerializationOptions.Packed)]
            public global::System.Collections.Generic.List<long> Longs;
        }
    }
}