namespace BankingApi.RequestDTOs
{
    public class TransferRequest
    {
        public int fromAccountId { get; set; }
        public int toAccountId { get; set; }
        public decimal amount { get; set; }
    }
}
