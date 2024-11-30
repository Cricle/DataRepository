using DataRepository.Models;

namespace DataRepository.Test.Models
{
    public class WorkPageResultTest
    {
        [Theory, AutoData]
        public void InitWithFail_MustFail(Fail fail)
        {
            var result = new WorkPageResult<object>(fail);

            result.Fail.Should().BeEquivalentTo(fail);
            result.Succeed.Should().BeFalse();
            result.PageCount.Should().Be(0);
            result.TotalCount.Should().Be(0);
            result.PageIndex.Should().Be(0);
            result.PageSize.Should().Be(0);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        public void TotalCountOrPageSizeError_TotalPageZero(int totalCount, int pageSize)
        {
            var result = new WorkPageResult<object>([], totalCount, 1, pageSize);

            result.Fail.Should().BeNull();
            result.Succeed.Should().BeTrue();
            result.PageCount.Should().Be(0);
            result.TotalCount.Should().Be(totalCount);
            result.PageIndex.Should().Be(1);
            result.PageSize.Should().Be(pageSize);
        }

        [Theory, AutoData]
        public void InitWithData_MustSucceed(List<object> list, int totalCount, int pageIndex, int pageSize)
        {
            var result = new WorkPageResult<object>(list, totalCount, pageIndex, pageSize);

            result.Fail.Should().BeNull();
            result.Succeed.Should().BeTrue();
            result.PageCount.Should().Be((int)Math.Ceiling((double)totalCount / pageSize));
            result.TotalCount.Should().Be(totalCount);
            result.PageIndex.Should().Be(pageIndex);
            result.PageSize.Should().Be(pageSize);
        }
    }
}
