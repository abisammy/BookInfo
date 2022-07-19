using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookInfo.Models;

public class Publisher
{
    // Primary Key
    [Key]
    public int Id { get; set; }

    // Name
    [Required]
    [MaxLength(64)]
    [DisplayName("Publisher")]
    public string Name { get; set; }
}