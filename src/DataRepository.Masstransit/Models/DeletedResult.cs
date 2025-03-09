namespace DataRepository.Masstransit.Models
{
    public class DeletedResult<TData> : OperatorResult<TData>
    {
        public int Count { get; set; }

        public List<string>? Conditions { get; set; }
    }
}
