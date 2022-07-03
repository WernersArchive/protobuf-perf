using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using ProtoBuf;

namespace Benchmarks
{
    [ProtoContract]
    public sealed class StringMessage : IMessage<StringMessage>
    {
        public static MessageParser<StringMessage> Parser { get; } = new MessageParser<StringMessage>(() => new StringMessage());

        public static StringMessage Create(string? suffix = null)
        {
            return new StringMessage()
            {
                Value = String.Format("some text {0}", suffix)
            };
        }

        [ProtoMember(10)]
        public string? Value;

        void IMessage<StringMessage>.MergeFrom(StringMessage message)
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
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        void IMessage.WriteTo(CodedOutputStream output)
        {
            if (this.Value?.Length != 0)
            {
                output.WriteRawTag(10);
                output.WriteString(this.Value);
            }
        }

        int IMessage.CalculateSize()
        {
            throw new NotImplementedException();
        }

        bool IEquatable<StringMessage>.Equals(StringMessage? other)
        {
            throw new NotImplementedException();
        }

        StringMessage IDeepCloneable<StringMessage>.Clone()
        {
            throw new NotImplementedException();
        }

        MessageDescriptor IMessage.Descriptor => throw new NotImplementedException();
    }
}