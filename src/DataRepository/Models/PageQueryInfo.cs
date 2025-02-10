namespace DataRepository.Models
{
    public class PageQueryInfo : IPageQueryInfo
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int SkipCount => Math.Max(0, (PageIndex - 1) * PageSize);
    }
}
