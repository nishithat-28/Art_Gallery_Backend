using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ArtGallery.API.DTOs
{
    public class ArtWorkDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Year { get; set; }
        public string Medium { get; set; } = string.Empty;
        public string Dimensions { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte[]? ImageBytes { get; set; }
        public string? ImageFileName { get; set; }
    }

    public class ArtWorkCreateDto
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public IFormFile ImageFile { get; set; }
        public int Year { get; set; }
        public string Medium { get; set; }
        public string Dimensions { get; set; }
        public int CategoryId { get; set; }
    }

    public class CreateArtWorkDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Artist { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public string Medium { get; set; } = string.Empty;

        [Required]
        public string Dimensions { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public IFormFile ImageFile { get; set; } = null!;
    }

    public class UpdateArtWorkDto
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Artist { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public string Medium { get; set; } = string.Empty;

        [Required]
        public string Dimensions { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public bool IsAvailable { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}