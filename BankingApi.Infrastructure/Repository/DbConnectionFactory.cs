using BankingApiCore.Persistance;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using System.Data;

namespace BankingApiInfrastructure.Repository;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public DbConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_options.MyLocalDataBase);
    }
}

