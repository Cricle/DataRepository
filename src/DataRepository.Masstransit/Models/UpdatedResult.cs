namespace DataRepository.Masstransit.Models
{
    public class UpdatedResult<TData> : OperatorResult<TData>
    {
        public int Count { get; set; }

        public List<string>? Conditions { get; set; }
    }
}
