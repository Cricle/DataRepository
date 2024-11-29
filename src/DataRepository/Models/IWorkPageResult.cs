namespace DataRepository.Models
{
    public interface IWorkPageResult<T>:IWorkDataResult<IEnumerable<T>>
    {
        int TotalCount { get; }

        int PageIndex { get; }

        int PageSize { get; }
    }
}
