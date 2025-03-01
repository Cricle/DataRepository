using DataRepository.Casing.Models;

namespace DataRepository.SampleWeb.Models
{
    public class GpsPosition : ITimedValue
    {
        public string DeviceId { get; set; } = null!;

        public DateTime Time { get; set; }

        public double Lat { get; set; }

        public double Lon { get; set; }

        public DateTime GetTime() => Time;
    }
}
