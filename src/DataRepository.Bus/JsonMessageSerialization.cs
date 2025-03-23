using DataRepository.Bus.Serialization;
using System.Text.Json;

namespace DataRepository.Bus
{
    public sealed class JsonMessageSerialization : IMessageSerialization
    {
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public JsonMessageSerialization(JsonSerializerOptions jsonSerializerOptions)
            => this.jsonSerializerOptions = jsonSerializerOptions;

        public object FromBytes(ReadOnlyMemory<byte> buffer, Type type)
            => JsonSerializer.Deserialize(buffer.Span, type, jsonSerializerOptions)!;

        public TMessage FromBytes<TMessage>(ReadOnlyMemory<byte> buffer)
            => JsonSerializer.Deserialize<TMessage>(buffer.Span, jsonSerializerOptions)!;

        public ReadOnlyMemory<byte> ToBytes<TMessage>(TMessage message)
            => JsonSerializer.SerializeToUtf8Bytes(message, jsonSerializerOptions);

        public ReadOnlyMemory<byte> ToBytes(object message, Type type)
            => JsonSerializer.SerializeToUtf8Bytes(message, type, jsonSerializerOptions);
    }
}
