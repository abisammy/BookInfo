using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookInfo.Models;

public class User
{
    // Primary Key
    [Key]
    public int Id { get; set; }

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
    public string AccountType { get; set; } = "USER";
}