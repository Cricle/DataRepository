namespace DataRepository.Models
{
    public interface IWorkDataResult<T> : IWorkResult
    {
        T? Data { get; }
    }
}
