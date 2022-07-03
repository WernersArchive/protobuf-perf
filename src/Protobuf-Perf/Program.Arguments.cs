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
        public bool ThreadLocalTypeModel { get; set; } = true;
        public bool IsLockFree { get; set; } = true;

        public int ObjectsForTesting { get; set; } = 2000000;

        public bool InitializeInstance { get; set; } = true;
        public bool NoLogo { get; set; } = false;


    }
}