namespace DataRepository.Casing
{
    public interface IInitable
    {
        bool IsInited { get; }

        Task InitAsync();

        Task DeInitAsync();
    }
}
