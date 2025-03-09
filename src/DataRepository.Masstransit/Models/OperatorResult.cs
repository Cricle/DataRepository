namespace DataRepository.Masstransit.Models
{
    public abstract class OperatorResult
    {
        public abstract string? DataType { get; }
    }

    public class OperatorResult<TData> : OperatorResult
    {
        protected internal static readonly string DataFullName = typeof(TData).FullName ?? string.Empty;

        public override string? DataType => DataFullName;
    }
}
