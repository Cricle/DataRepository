using System.Diagnostics.CodeAnalysis;

namespace DataRepository.Casing.Redis.Test
{
    [ExcludeFromCodeCoverage]
    public class Student
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}
