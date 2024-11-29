using DataRepository.Models;

namespace DataRepository.Test.Models
{
    public class WorkDataResultTest
    {
        [Theory, AutoData]
        public void CaseWithFail_ReturnFail(Fail fail)
        {
            WorkDataResult<object> result = fail;

            result.Should().BeEquivalentTo(new WorkDataResult<object>(fail));
        }

        [Theory, AutoData]
        public void CaseWithData_ReturnSucceed(object obj)
        {
            WorkDataResult<object> result = obj;

            result.Should().BeEquivalentTo(new WorkDataResult<object>(obj));
        }
    }
}
