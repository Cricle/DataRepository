using DataRepository.Bus.Serialization;
using System.Text;
using System.Text.Json;

namespace DataRepository.Bus.Test.Serialization
{
    public class JsonMessageSerializationTest
    {
        [Fact]
        public void FromBytes_MustDeserialize()
        {
            var ser = new JsonMessageSerialization();

            var stu = ser.FromBytes<Student>("{\"Id\":123}"u8.ToArray());
            stu.Should().BeEquivalentTo(new Student { Id = 123 });
            stu = (Student)ser.FromBytes("{\"Id\":123}"u8.ToArray(), typeof(Student));
            stu.Should().BeEquivalentTo(new Student { Id = 123 });
        }

        [Fact]
        public void ToBytes_MustSerialize()
        {
            var ser = new JsonMessageSerialization();

            var stu = ser.ToBytes(new Student { Id = 123 });
            Encoding.UTF8.GetString(stu.Span).Should().Be("{\"Id\":123}");
            stu = ser.ToBytes(new Student { Id = 123 }, typeof(Student));
            Encoding.UTF8.GetString(stu.Span).Should().Be("{\"Id\":123}");
        }

        [Fact]
        public void JsonSerializerOptionsInput()
        {
            var opt = new JsonSerializerOptions();

            var ser = new JsonMessageSerialization(opt);
            ser.options.Should().BeEquivalentTo(opt);
        }

        class Student
        {
            public int Id { get; set; }
        }
    }
}
