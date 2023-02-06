using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookInfo.Models;

public enum AccountType
{
    User,
    Administrator
}

public class User
{
    // Primary Key
    [Key]
    public int UserId { get; set; }

    // Name
    [Required]
    [MaxLength(128)]
    public string Username
    {
        get; set;
    }

    [Required]
    public string Password { get; set; }

    [Required]
    [DisplayName("Account Type")]
    public AccountType AccountType { get; set; } = AccountType.User;
}