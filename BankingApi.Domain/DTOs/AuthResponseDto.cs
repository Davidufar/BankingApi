using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApiCore.DTOs
{
    public record AuthResponseDto(
    int ClientId,
    string Email,
    string AccessToken,
    string RefreshToken
    );
}
