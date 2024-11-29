namespace DataRepository.Models
{
    public class WorkResult : IWorkResult
    {
        public static readonly WorkResult SucceedResult = new WorkResult(null);

        public WorkResult(Fail? fail) => Fail = fail;

        public Fail? Fail { get; }

        public bool Succeed => Fail == null;

        public static implicit operator WorkResult(Fail? fail)
        {
            return fail == null ? SucceedResult : new WorkResult(fail);
        }
    }
}
