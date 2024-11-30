namespace DataRepository.Models
{
    public class WorkDataResult<T> : WorkResult, IWorkDataResult<T>
    {
        public WorkDataResult(Fail fail) : base(fail)
        {

        }

        public WorkDataResult(T? data) : base(null) => Data = data;

        public T? Data { get; }

        public static implicit operator WorkDataResult<T>(Fail fail)
        {
            return new WorkDataResult<T>(fail);
        }

        public static implicit operator WorkDataResult<T>(T? data)
        {
            return new WorkDataResult<T>(data);
        }
    }
}
