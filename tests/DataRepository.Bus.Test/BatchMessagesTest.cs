namespace DataRepository.Bus.Test
{
    public class BatchMessagesTest
    {
        [Fact]
        public void Create_ThrowArgumentNullException_WhenInputNullBuffer()
        {
            Assert.Throws<ArgumentNullException>(() => BatchMessages<int>.Create(null!, 1));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Create_ThrowIndexOutOfRangeException_WhenSizeMinThanZero(int size)
        {
            Assert.Throws<ArgumentNullException>(() => BatchMessages<int>.Create(new int[2], size));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public void Create_ThrowIndexOutOfRangeException_WhenSizeOutOfBuffer(int size)
        {
            Assert.Throws<ArgumentNullException>(() => BatchMessages<int>.Create(new int[2], size));
        }

        [Fact]
        public void Create_FullSize_Succeed()
        {
            var buffer = new int[2] { 1, 2 };
            var msg = BatchMessages<int>.Create(buffer, buffer.Length);

            msg.shouldReturn.Should().BeFalse();
            msg.Size.Should().Be(2);
            msg.Memory.ToArray().Should().BeEquivalentTo([1, 2]);
            msg.Span.ToArray().Should().BeEquivalentTo([1, 2]);
            msg.UnsafeGetBuffer().Should().BeEquivalentTo(buffer);

            msg.Dispose();
        }

        [Fact]
        public void Create_PartSize_Succeed()
        {
            var buffer = new int[2] { 1, 2 };
            var msg = BatchMessages<int>.Create(buffer, 1);

            msg.shouldReturn.Should().BeFalse();
            msg.Size.Should().Be(1);
            msg.Memory.ToArray().Should().BeEquivalentTo([1]);
            msg.Span.ToArray().Should().BeEquivalentTo([1]);
            msg.UnsafeGetBuffer().Should().BeEquivalentTo(buffer);

            msg.Dispose();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Rent_ThrowIndexOutOfRangeException_WhenSizeMinThenZero(int size)
        {
            Assert.Throws<ArgumentNullException>(() => BatchMessages<int>.Rent(size));
        }

        [Fact]
        public void Rent_Succeed()
        {
            var msg = BatchMessages<int>.Rent(100);

            msg.buffer.Length.Should().BeGreaterThanOrEqualTo(100);
            msg.Size.Should().Be(100);
            msg.Memory.Length.Should().Be(100);
            msg.Span.Length.Should().Be(100);

            msg.Dispose();
        }
    }
}
