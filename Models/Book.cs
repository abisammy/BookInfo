using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookInfo.Models;


public class Book
{
    // Primary Key
    [Key]
    public int Id { get; set; }

    [Required]
    [MinLength(13)]
    [MaxLength(13)]
    public string ISBN
    { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; }

    // Category
    [ForeignKey("Category")]
    [DisplayName("Category")]
    public int CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    // Description
    [MaxLength(1024)]
    public string Description { get; set; } = "No description";

    // Updated at
    [DisplayName("Updated at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Author
    [ForeignKey("Author")]
    [DisplayName("Author")]
    public int AuthorId { get; set; }

    public virtual Author? Author { get; set; }

    // Publisher
    [ForeignKey("Publisher")]
    [DisplayName("Publisher")]
    public int PublisherId { get; set; }

    public virtual Publisher? Publisher { get; set; }
}