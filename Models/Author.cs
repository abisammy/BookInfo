using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookInfo.Models;

public class Author
{
    // Primary Key
    [Key]
    public int AuthorId { get; set; }

    // Name
    [Required]
    [MaxLength(64)]
    [DisplayName("Author")]
    public string Name { get; set; }

    // Updated at
    [Required]
    [DisplayName("Updated at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}