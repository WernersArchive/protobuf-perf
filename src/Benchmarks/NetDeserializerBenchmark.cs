using System;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;
using ProtoBuf.Meta;

namespace Benchmarks
{
    public abstract class NetDeserializerBenchmark
    {
        public bool InitializeInstance { get; set; } = true;

        [Params(1000 * 1000)]
        public long Iterations { get; set; } = 1;

        public bool ThreadLocalTypeModel { get; set; } = true;

        internal List<byte[]> ProtobufNetTestData { get; private set; } = new List<byte[]>();

        public abstract void Setup();

        public string SettingsTrace()
        {
            if (this.DegreeOfParallelism.HasValue)
            {
                return "DegreeOfParallelism=" + this.DegreeOfParallelism.Value.ToString();
            }
            else
            {
                return "DegreeOfParallelism=default";
            }
        }

        public NetDeserializerBenchmark()
        {
            this.ThreadRTM = new ThreadLocal<RuntimeTypeModel>(this.ThreadFactory);
        }

        protected abstract RuntimeTypeModel ThreadFactory();

        public ThreadLocal<RuntimeTypeModel> ThreadRTM { get; }

        protected T NetDeserialize<T>(byte[] buffer, T? initvalue)
        {
            var model = this.ThreadRTM.Value!; //RuntimeTypeModel.Default
            return model.Deserialize<T>((ReadOnlySpan<byte>)buffer, initvalue, typeof(T));
        }

        public int? DegreeOfParallelism { get; set; } = null;
    }
}