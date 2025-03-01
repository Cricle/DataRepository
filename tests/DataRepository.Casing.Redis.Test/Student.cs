using DataRepository.Casing.Models;
using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Casing.Redis.Test
{
    [ExcludeFromCodeCoverage]
    public class Student : ITimedValue
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public DateTime Time { get; set; }

        public DateTime GetTime() => Time;
    }
}
