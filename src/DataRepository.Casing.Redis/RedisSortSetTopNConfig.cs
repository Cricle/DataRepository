namespace DataRepository.Casing.Redis
{
    public class RedisHashCasingNewestConfig
    {
        public RedisHashCasingNewestConfig(string prefx)
        {
            Prefx = prefx;
        }

        public string Prefx { get; set; }

    }
    public class RedisSortSetTopNConfig
    {
        public RedisSortSetTopNConfig(string prefx)
        {
            Prefx = prefx;
        }

        public string Prefx { get; set; }

        public int StoreSize { get; set; } = 100;
    }
}
