using BankingApiCore.Enums;
using System.ComponentModel.DataAnnotations;

namespace BankingApiCore.Models;

    public class Client
    {

    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters.")]
    public required string Name { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Surname must be between 2 and 50 characters.")]
    public required string Surname { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(100)]
    public required string Email { get; set; }

    [Required]
    [Phone(ErrorMessage = "Invalid phone number.")]
    [StringLength(20)]
    public required string PhoneNumber { get; set; }

    [Required]
    [StringLength(255)] 
    public required string Password { get; set; }

    [Required]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "SSN must be exactly 9 digits.")]
    [StringLength(9, MinimumLength = 9)]
    public required string SSN { get; set; }

    public ClientStatus Status { get; set; } = ClientStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Account> Accounts { get; set; } = new();
}

