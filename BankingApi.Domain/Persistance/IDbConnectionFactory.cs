using System.Data;

namespace BankingApiCore.Persistance;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
