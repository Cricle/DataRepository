using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Casing.Redis
{
    public interface INewestValueConverter<
#if !NETSTANDARD
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif

    T>
    {
        T? ConvertBack(RedisValue value);

        RedisValue Convert(T? value);
    }
}
