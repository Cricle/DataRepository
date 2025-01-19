using System.Data;

namespace DataRepository
{
    public interface IDbConnectionProvider
    {
        bool SupportDbConnection { get; }

        IDbConnection? GetConnection();
    }
}
