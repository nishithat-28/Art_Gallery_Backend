namespace ArtGallery.API.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public ICollection<ArtWork> ArtWorks { get; set; } = new List<ArtWork>();
    }
}