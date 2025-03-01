using System.Diagnostics.CodeAnalysis;

namespace DataRepository.EFCore.Test
{
    [ExcludeFromCodeCoverage]
    public class Student
    {
        public string? Name { get; set; }

        public int Hit { get; set; }

        public DateTime? Date { get; set; }
    }
}
