namespace DataRepository.SampleWeb.Models
{
    public class GpsPosition
    {
        public string DeviceId { get; set; } = null!;

        public DateTime Time { get; set; }

        public double Lat { get; set; }

        public double Lon { get; set; }
    }
}
