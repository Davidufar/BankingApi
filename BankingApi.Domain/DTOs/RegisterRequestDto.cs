using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApiCore.DTOs
{
    public record RegisterRequestDto(
    string Name,
    string Surname,
    string Email,
    string PhoneNumber,
    string Password,
    string SSN
    );
}
