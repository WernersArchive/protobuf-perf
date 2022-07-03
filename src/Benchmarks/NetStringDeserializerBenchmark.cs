using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using ProtoBuf.Meta;

namespace Benchmarks
{
    public class NetStringDeserializerBenchmark : NetDeserializerBenchmark
    {
        [Benchmark]
        public void StringMessageDeserializerParallel()
        {
            var output = new StringMessage[this.Iterations];
            var parallelDoings = this.ProtobufNetTestData.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism);
            if (DegreeOfParallelism.HasValue)
            {
                parallelDoings = parallelDoings.WithDegreeOfParallelism(DegreeOfParallelism.Value);
            }
            parallelDoings.Select((x, i) =>
            {
                output[i] = NetDeserialize<StringMessage>(x, InitializeInstance ? new StringMessage() : null);
                return true;
            }).All(_ => _);
        }

        [Benchmark]
        public void StringMessageDeserializerSerial()
        {
            var output = new StringMessage[this.Iterations];
            int index = 0;
            foreach (var x in this.ProtobufNetTestData)
            {
                output[index++] = NetDeserialize<StringMessage>(x, this.InitializeInstance ? new StringMessage() : null);
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
                rt.Add(typeof(StringMessage), true);
            }
            else
            {
                rt = RuntimeTypeModel.Default;
            }
            rt[typeof(StringMessage)].CompileInPlace();
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
                    ThreadRTM.Value!.Serialize<StringMessage>(ms, StringMessage.Create(i.ToString()));
                    ProtobufNetTestData.Add(ms.ToArray());
                }
            }
        }
    }
}