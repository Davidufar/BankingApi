using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApiCore.Models
{
    public record ClientAuthRecord(int Id, string Email, string Password);

    public record ClientSessionRecord(
        Guid SessionId,
        int ClientId,
        DateTime LastActivityAt,
        DateTime ExpiresAt,
        bool IsRevoked,
        string Email
    )
    {
    // Parameterless constructor required for Dapper/ORM materialization
    public ClientSessionRecord() : this(
        Guid.Empty,
        0,
        DateTime.MinValue,
        DateTime.MinValue,
        false,
        string.Empty)
    {
    }
}
}
