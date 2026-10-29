

namespace BankingApiCore.DTOs;
    public class ClientDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public List<AccountDTO> Accounts { get; set; } = new();
    }

