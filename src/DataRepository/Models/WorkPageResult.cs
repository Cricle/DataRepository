namespace DataRepository.Models
{
    public class WorkPageResult<T> : WorkDataResult<IEnumerable<T>>, IWorkPageResult<T>
    {
        public WorkPageResult(Fail fail) : base(fail)
        {
        }

        public WorkPageResult(IEnumerable<T>? data, int totalCount, int pageIndex, int pageSize)
            : base(data)
        {
            TotalCount = totalCount;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        public int TotalCount { get; }

        public int PageIndex { get; }

        public long PageCount
        {
            get
            {
                if (TotalCount <= 0 || PageSize <= 0) return 0;

                return (long)Math.Ceiling((double)TotalCount / PageSize);
            }
        }

        public int PageSize { get; }
    }
}
