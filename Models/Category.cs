using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookInfo.Models;

public class Category
{
    // Primary Key
    [Key]
    public int CategoryId { get; set; }

    // Name
    [Required]
    [MaxLength(32)]
    public string Name { get; set; }

    // Description
    [MaxLength(1024)]
    public string Description { get; set; } = "No description";

    // Updated at
    [DisplayName("Updated at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}