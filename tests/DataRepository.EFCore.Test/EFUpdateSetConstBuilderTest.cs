using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace DataRepository.EFCore.Test
{
    public class EFUpdateSetConstBuilderTest
    {
        [Fact]
        public void BuildWithSet_MustEqualsExpression()
        {
            Expression<Func<SetPropertyCalls<Student>, SetPropertyCalls<Student>>> exp = w => w.SetProperty(y => y.Hit, 1);

            var builder = new EFUpdateSetBuilder<Student>()
                .SetProperty(y => y.Hit, 1);
            var actual = builder.Build();

            builder.SetCount.Should().Be(1);
            actual.Should().BeEquivalentTo(exp);
        }

        [Fact]
        public void BuildWithSetAndAdd_MustEqualsExpression()
        {
            Expression<Func<SetPropertyCalls<Student>, SetPropertyCalls<Student>>> exp = w => w.SetProperty(y => y.Hit, 1).SetProperty(y => y.Hit, y => y.Hit + 1);

            var builder = new EFUpdateSetBuilder<Student>()
                .SetProperty(x => x.Hit, 1)
                .SetProperty(x => x.Hit, x => x.Hit + 1);
            var actual = builder.Build();

            builder.SetCount.Should().Be(2);
            actual.Should().BeEquivalentTo(exp);
        }

        [Fact]
        public void BuildWithSetAndAdd_ThrowWhenNoSet()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new EFUpdateSetBuilder<Student>().Build());
            ex.Message.Should().BeEquivalentTo("Must at less one set");
        }
    }
}
