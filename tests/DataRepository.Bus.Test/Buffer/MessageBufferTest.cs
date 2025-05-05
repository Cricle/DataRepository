using DataRepository.Bus.Buffer;

namespace DataRepository.Bus.Test.Buffer
{
    public class MessageBufferTest
    {
        [Fact]
        public void Add_Clear()
        {
            var buffer = new MessageBuffer<int>(2);

            buffer.MaxCount.Should().Be(2);
            buffer.Add(1).Should().BeFalse();
            buffer.Index.Should().Be(1);
            buffer.Add(1).Should().BeTrue();
            buffer.Index.Should().Be(2);
            buffer.UnsafeGetMessages().Should().BeEquivalentTo([1,1]);

            buffer.Clear();
            buffer.Index.Should().Be(0);
        }

        [Fact]
        public void AddAndToBatchMessage()
        {
            var buffer = new MessageBuffer<int>(2);

            buffer.Add(1).Should().BeFalse();
            buffer.Index.Should().Be(1);

            var msg = buffer.ToBathMessages();
            msg.Size.Should().Be(1);
            msg.Memory.Length.Should().Be(1);
            msg.Memory.Span[0].Should().Be(1);
            msg.Span.Length.Should().Be(1);
            msg.Span[0].Should().Be(1);
        }
    }
}
