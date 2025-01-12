namespace DataRepository.Casing.Models
{
    public readonly record struct NewestResult<T>(DateTime Time, T Value)
    {
        public long UnixTimeMilliseconds => new DateTimeOffset(Time.ToUniversalTime()).ToUnixTimeMilliseconds();
    }
}
