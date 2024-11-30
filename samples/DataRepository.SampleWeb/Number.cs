using System.ComponentModel.DataAnnotations;

namespace DataRepository.SampleWeb
{
    public class Number
    {
        [Key]
        public int Id { get; set; }

        public int Value { get; set; }
    }
}
