using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Bus.Serialization
{
    public interface IMessageSerialization
    {
        ReadOnlyMemory<byte> ToBytes<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(TMessage message);

        ReadOnlyMemory<byte> ToBytes(object message,Type type);

        TMessage FromBytes<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(ReadOnlyMemory<byte> buffer);

        object FromBytes(ReadOnlyMemory<byte> buffer,Type type);
    }
}
