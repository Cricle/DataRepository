using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DataRepository.Bus.Serialization
{
    public class JsonMessageSerialization : IMessageSerialization
    {
        internal readonly JsonSerializerOptions? options;

        public JsonMessageSerialization(JsonSerializerOptions? options = null) => this.options = options;

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        public TMessage FromBytes<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(ReadOnlyMemory<byte> buffer)
            => JsonSerializer.Deserialize<TMessage>(buffer.Span, options)!;

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        public object FromBytes(ReadOnlyMemory<byte> buffer, Type type)
            => JsonSerializer.Deserialize(buffer.Span, type, options)!;

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        public ReadOnlyMemory<byte> ToBytes<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TMessage>(TMessage message)
            => JsonSerializer.SerializeToUtf8Bytes(message, options);

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        public ReadOnlyMemory<byte> ToBytes(object message, Type type)
            => JsonSerializer.SerializeToUtf8Bytes(message, type, options);
    }
}
