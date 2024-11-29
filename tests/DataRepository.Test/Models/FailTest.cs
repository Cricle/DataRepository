using DataRepository.Models;

namespace DataRepository.Test.Models
{
    public class FailTest
    {
        [Theory, AutoData]
        public void CaseWithWithMsg(string? msg, int? code)
        {
            var fail = Fail.WithMsg(msg, code);
            fail.Msg.Should().Be(msg);
            fail.Code.Should().Be(code);
        }

        [Theory, AutoData]
        public void CaseWithWithMsgEnum(string? msg, ConsoleKey key)
        {
            var fail = Fail.WithMsg(msg, key);
            fail.Msg.Should().Be(msg);
            fail.Code.Should().Be((int)key);
        }
    }
}
