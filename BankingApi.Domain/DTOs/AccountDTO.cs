

namespace BankingApiCore.DTOs;

public class AccountDTO
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0.00m;
    public DateTime CreatedAt { get; set; }
}
