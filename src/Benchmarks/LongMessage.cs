using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using ProtoBuf;

namespace Benchmarks
{
    /// <summary>
    /// we implement 3 variables from type long = 2*64Bit => 16 Byte
    /// </summary>
    [ProtoContract]
    public sealed class LongMessage : IMessage<LongMessage>
    {
        public static MessageParser<LongMessage> Parser { get; } = new MessageParser<LongMessage>(() => new LongMessage());

        private static Random Default = new Random();

        public static LongMessage Create()
        {
            return new LongMessage()
            {
                Value1 = Default.Next(),
                Value2 = Default.Next(),
                Value3 = Default.Next()
            };
        }

        [ProtoMember(10)]
        public long? Value1;

        [ProtoMember(11)]
        public long? Value2;

        [ProtoMember(12)]
        public long? Value3;

        [ProtoMember(13)]
        public long? Value4;

        [ProtoMember(14)]
        public long? Value5;

        [ProtoMember(15)]
        public long? Value6;

        [ProtoMember(16)]
        public long? Value7;


        void IMessage<LongMessage>.MergeFrom(LongMessage message)
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
                            this.Value1 = input.ReadInt64();
                            break;
                        }
                    case 11:
                        {
                            this.Value2 = input.ReadInt64();
                            break;
                        }
                    case 12:
                        {
                            this.Value3 = input.ReadInt64();
                            break;
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        void IMessage.WriteTo(CodedOutputStream output)
        {
            if (this.Value1.HasValue)
            {
                output.WriteRawTag(10);
                output.WriteInt64(this.Value1.Value);
            }
            if (this.Value2.HasValue)
            {
                output.WriteRawTag(11);
                output.WriteInt64(this.Value2.Value);
            }
            if (this.Value3.HasValue)
            {
                output.WriteRawTag(12);
                output.WriteInt64(this.Value3.Value);
            }
        }

        int IMessage.CalculateSize()
        {
            throw new NotImplementedException();
        }

        bool IEquatable<LongMessage>.Equals(LongMessage? other)
        {
            throw new NotImplementedException();
        }

        LongMessage IDeepCloneable<LongMessage>.Clone()
        {
            throw new NotImplementedException();
        }

        MessageDescriptor IMessage.Descriptor => throw new NotImplementedException();
    }
}