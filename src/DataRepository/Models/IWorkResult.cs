namespace DataRepository.Models
{
    public interface IWorkResult
    {
        Fail? Fail { get; }

        bool Succeed { get; }
    }
}
