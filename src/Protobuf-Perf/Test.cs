using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using ProtoBuf;

namespace ConsoleApp
{
    public partial class Program
    {
        [ProtoContract]
        public sealed class Test : IMessage<Test>
        {
            public static MessageParser<Test> Parser { get; } = new MessageParser<Test>(() => new Test());

            public static Test Create(string suffix, int numberOfLongs = 25)
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

            [ProtoMember(10)]
            public string Value;

            [global::ProtoBuf.ProtoMember(20, Name = @"longs", DataFormat = global::ProtoBuf.DataFormat.ZigZag, Options = global::ProtoBuf.MemberSerializationOptions.Packed)]
            public global::System.Collections.Generic.List<long> Longs;

            void IMessage<Test>.MergeFrom(Test message)
            {
                throw new NotImplementedException();
            }

            void IMessage.MergeFrom(CodedInputStream input)
            {
                uint tag;
                while ((tag = input.ReadTag()) != 0)
                {
                    switch (tag)
                    {
                        case 10:
                            {
                                this.Value = input.ReadString();
                                break;
                            }
                        case 20:
                            {
                                int size = input.ReadInt32();
                                this.Longs = new List<long>();
                                for (int i = 0; i < size; i++)
                                {
                                    this.Longs.Add(input.ReadInt64());
                                }
                                break;
                            }
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            void IMessage.WriteTo(CodedOutputStream output)
            {
                if (this.Value.Length != 0)
                {
                    output.WriteRawTag(10);
                    output.WriteString(this.Value);
                }

                if (this.Longs.Count > 0)
                {
                    output.WriteTag(20);
                    output.WriteInt32(this.Longs.Count);
                    foreach (var l in Longs)
                    {
                        output.WriteInt64(l);
                    }
                }
            }

            int IMessage.CalculateSize()
            {
                throw new NotImplementedException();
            }

            bool IEquatable<Test>.Equals(Test other)
            {
                throw new NotImplementedException();
            }

            Test IDeepCloneable<Test>.Clone()
            {
                throw new NotImplementedException();
            }

            MessageDescriptor IMessage.Descriptor => throw new NotImplementedException();
        }
    }
}