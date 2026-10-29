using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankingApiCore.Models;

public class Account
{
    public int Id { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "A valid ClientId is required.")]
    public int ClientId { get; set; }

    [Required]
    [StringLength(15, MinimumLength = 15, ErrorMessage = "Account number must be exactly 15 digits.")]
    [RegularExpression(@"^\d{15}$", ErrorMessage = "Account number must consist of 15 numeric digits.")]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.00, double.MaxValue, ErrorMessage = "Initial balance cannot be negative.")]
    public decimal Balance { get; set; } = 0.00m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Client? Client { get; set; }
}
