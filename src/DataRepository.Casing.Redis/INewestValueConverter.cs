using StackExchange.Redis;

namespace DataRepository.Casing.Redis
{
    public interface INewestValueConverter<T>
    {
        T? ConvertBack(RedisValue value);

        RedisValue Convert(T? value);
    }
}
