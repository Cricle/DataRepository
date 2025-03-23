namespace DataRepository.Bus.Serialization
{
    public interface IMessageSerialization
    {
        ReadOnlyMemory<byte> ToBytes<TMessage>(TMessage message);

        ReadOnlyMemory<byte> ToBytes(object message,Type type);

        TMessage FromBytes<TMessage>(ReadOnlyMemory<byte> buffer);

        object FromBytes(ReadOnlyMemory<byte> buffer,Type type);
    }
}
