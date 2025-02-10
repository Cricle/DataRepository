namespace DataRepository.Models
{
    public interface IPageQueryInfo
    {
        int PageIndex { get; }

        int PageSize { get; }
    }
}
