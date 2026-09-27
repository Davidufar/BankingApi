namespace BankingApi.RequestDTOs
{
    public class AccountRequest
    {
        public int ClientId { get; set; }
        
        public decimal InitialBalance { get; set; } = 0.00m;
    }
}
