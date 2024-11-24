using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Moq
{
    [ExcludeFromCodeCoverage]
    internal static class MoqVerifyExtensions
    {
        public static void VerifyOnce<T>(this Mock<T> mock, Expression<Action<T>> expression)
            where T : class
        {
            mock.Verify(expression, Times.Once());
        }

        public static void VerifyOnceNoOthersCall<T>(this Mock<T> mock, Expression<Action<T>> expression)
            where T : class
        {
            VerifyOnce(mock, expression);
            mock.VerifyNoOtherCalls();
        }
    }
}
