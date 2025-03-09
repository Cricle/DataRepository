namespace DataRepository.Masstransit.Models
{
    public class AddedResult<TData> : OperatorResult<TData>
    {
        public int Count { get; set; }

        public List<TData>? Datas { get; set; }
    }
}
