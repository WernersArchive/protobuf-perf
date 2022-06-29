using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BenchmarkDotNet.Attributes;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Benchmarks
{
    public class ProtoReaderStateBenchmark
    {

        [Params(1000*1000)]
        public long Iterations { get; set; } = 1;

        [Params(true, false)]
        public bool ShortCall { get; set; }

        private readonly byte[] DataAsArray;

        public ProtoReaderStateBenchmark()
        {
            for (int zz = 0; zz < this.Iterations; zz++)
            {
                this.IterationItems.Add(zz);
            }
            using (var ms = new MemoryStream())
            {
                this.ThreadRTM.Value!.Serialize<Test>(ms, Test.Create(99.ToString()));
                DataAsArray = ms.ToArray();
            }
        }

        internal readonly ThreadLocal<RuntimeTypeModel> ThreadRTM = new ThreadLocal<RuntimeTypeModel>(() =>
        {
            RuntimeTypeModel rt;
            //TODO LockFree impl removed
            //rt = RuntimeTypeModel.Create($"ThreadID={Environment.CurrentManagedThreadId}", lockFree: IsLockFree);
            rt = RuntimeTypeModel.Create($"ThreadID={Environment.CurrentManagedThreadId}");
            rt.Add(typeof(Test), true);
            rt[typeof(Test)].CompileInPlace();
            return rt;
        });

        private List<long> IterationItems { get; } = new List<long>();

        /// <summary>
        /// 1.8 sec/1 mio net5.0
        /// 1.7 sec/1 mio net6.0
        /// </summary>
        [Benchmark()]
        public void ExecuteAsSpanInParallel()
        {
            ProtoReader.ShortCall = this.ShortCall;
            IterationItems.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).Select((x, i) =>
            {
                var m = ProtoReaderStateBenchmark.StateDeserialize<Test>(this.DataAsArray, ThreadRTM.Value!, false, new Test());
                return true;
            }).All(_ => _);
        }

        /// <summary>
        /// 4.9 sec/1 mio net5.0
        /// 4.7 sec/1 mio net6.0
        /// </summary>
        [Benchmark()]
        public void ExecuteAsSpanInSeria()
        {
            ProtoReader.ShortCall = this.ShortCall;
            IterationItems.Select((x, i) =>
            {
                var m = ProtoReaderStateBenchmark.StateDeserialize<Test>(this.DataAsArray, ThreadRTM.Value!, false, new Test());
                return true;
            }).All(_ => _);
        }

        public static T StateDeserialize<T>(byte[] buffer, RuntimeTypeModel model, bool noOp, T initvalue)
        {
            using (var state = ProtoReader.State.Create(buffer, model))
            {
                if (!noOp)
                {
                    int fieldNumber;
                    int loops = 0;
                    while ((fieldNumber = state.ReadFieldHeader()) > 0)
                    {
                        loops += 1;
                        if (state.WireType == WireType.String)
                        {
                            state.ReadString();
                        }
                        else
                        {
                            Debug.Fail("not in this sample");
                        };
                    }
                    Debug.Assert(loops == 2);
                }
            }
            return initvalue;
        }
    }
}