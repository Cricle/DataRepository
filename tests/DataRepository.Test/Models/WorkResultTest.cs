using DataRepository.Models;

namespace DataRepository.Test.Models
{
    public class WorkResultTest
    {
        [Fact]
        public void SucceedResult_MustSucceed()
        {
            WorkResult.SucceedResult.Succeed.Should().BeTrue();
            WorkResult.SucceedResult.Fail.Should().BeNull();
        }

        [Theory, AutoData]
        public void CaseWithFail_MustFail(Fail fail)
        {
            WorkResult result = fail;
            result.Fail.Should().BeEquivalentTo(fail);
            result.Succeed.Should().BeFalse();
        }

        [Fact]
        public void CaseWithFail_MustSucceed_WhenNullFail()
        {
            WorkResult result = (Fail?)null;
            result.Should().BeEquivalentTo(WorkResult.SucceedResult);
        }
    }
}
