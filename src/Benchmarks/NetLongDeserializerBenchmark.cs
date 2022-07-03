using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using ProtoBuf.Meta;

namespace Benchmarks
{
    public class NetLongDeserializerBenchmark : NetDeserializerBenchmark
    {
        [Benchmark]
        public void LongMessageDeserializerParallel()
        {
            var output = new LongMessage[this.Iterations];
            var parallelDoings = this.ProtobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism);
            if (DegreeOfParallelism.HasValue)
            {
                parallelDoings = parallelDoings.WithDegreeOfParallelism(DegreeOfParallelism.Value);
            }
            parallelDoings.Select((x, i) =>
            {
                output[i] = NetDeserialize<LongMessage>(x, InitializeInstance ? new LongMessage() : null);
                return true;
            }).All(_ => _);
        }
        [Benchmark]
        public void LongMessageDeserializerSerial()
        {
            var output = new LongMessage[this.Iterations];
            int index = 0;
            foreach (var x in this.ProtobufNetTestData)
            {
                output[index++] = NetDeserialize<LongMessage>(x, this.InitializeInstance ? new LongMessage() : null);
            }
        }

        protected override RuntimeTypeModel ThreadFactory()
        {
            RuntimeTypeModel rt;
            if (ThreadLocalTypeModel)
            {
                //TODO LockFree impl removed
                //rt = RuntimeTypeModel.Create($"ThreadID={Environment.CurrentManagedThreadId}", lockFree: IsLockFree);
                rt = RuntimeTypeModel.Create($"ThreadID={Environment.CurrentManagedThreadId}");
                rt.Add(typeof(LongMessage), true);
            }
            else
            {
                rt = RuntimeTypeModel.Default;
            }
            rt[typeof(LongMessage)].CompileInPlace();
            return rt;
        }

        [GlobalSetup()]
        public override void Setup()
        {
            Debug.Assert(this.Iterations > 0);
            for (int i = 0; i < Iterations; i++)
            {
                using (var ms = new MemoryStream())
                {
                    ThreadRTM.Value!.Serialize<LongMessage>(ms, LongMessage.Create());
                    ProtobufNetTestData.Add(ms.ToArray());
                }
            }
        }
    }
}