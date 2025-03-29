using MemoryPack;
using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Bus.Serialization.MemoryPack
{
    public sealed class MemoryPackMessageSerialization : IMessageSerialization
    {
        private readonly MemoryPackSerializerOptions? options;

        public MemoryPackMessageSerialization(MemoryPackSerializerOptions? options = null)
        {
            this.options = options;
        }

        public TMessage FromBytes<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(ReadOnlyMemory<byte> buffer)
        {
            return MemoryPackSerializer.Deserialize<TMessage>(buffer.Span, options)!;
        }

        public object FromBytes(ReadOnlyMemory<byte> buffer, Type type)
        {
            return MemoryPackSerializer.Deserialize(type, buffer.Span, options)!;
        }

        public ReadOnlyMemory<byte> ToBytes<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(TMessage message)
        {
            return MemoryPackSerializer.Serialize(message, options)!;
        }

        public ReadOnlyMemory<byte> ToBytes(object message, Type type)
        {
            return MemoryPackSerializer.Serialize(type, message, options)!;
        }
    }
}
