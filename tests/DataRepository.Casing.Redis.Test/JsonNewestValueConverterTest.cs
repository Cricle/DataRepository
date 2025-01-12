using StackExchange.Redis;
using System.Text.Json;

namespace DataRepository.Casing.Redis.Test
{
    public class JsonNewestValueConverterTest
    {
        [Fact]
        public void GivenNull_MustThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonNewestValueConverter<object>(null!));
        }

        [Theory, AutoData]
        public void Convert_MustConvertToRedisValue(Student student)
        {
            var convert = new JsonNewestValueConverter<Student>();

            var res = convert.Convert(student);

            res.Should().BeEquivalentTo(new RedisValue(JsonSerializer.Serialize(student)));
        }

        [Fact]
        public void ConvertBack_MustConvertBackToValue()
        {
            var convert = new JsonNewestValueConverter<Student>();

            var res = convert.ConvertBack("{\"Id\":123, \"Name\":\"name\"}");

            res.Should().BeEquivalentTo(new Student { Id = 123, Name = "name" });
        }
    }
}
