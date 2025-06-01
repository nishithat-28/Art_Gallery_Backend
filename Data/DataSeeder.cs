using ArtGallery.API.Models;
using ArtGallery.API.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArtGallery.API.Data
{
    public class DataSeeder
    {
        private readonly IWebHostEnvironment _environment;

        public DataSeeder(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task SeedDataAsync(ArtGalleryContext context)
        {
            try
            {
                // First seed categories
                if (!context.Categories.Any())
                {
                    Console.WriteLine("Seeding categories...");
                    await SeedCategoriesAsync(context);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Categories seeded successfully.");
                }

                // Then seed users
                if (!context.Users.Any())
                {
                    Console.WriteLine("Seeding users...");
                    await SeedUsersAsync(context);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Users seeded successfully.");
                }

                // Finally seed artworks
                if (!context.ArtWorks.Any())
                {
                    Console.WriteLine("Seeding artworks...");
                    await SeedArtWorksAsync(context);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Artworks seeded successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during seeding: {ex.Message}");
                throw;
            }
        }

        private async Task SeedUsersAsync(ArtGalleryContext context)
        {
            AuthHelper.CreatePasswordHash("Admin123!", out byte[] adminPasswordHash, out byte[] adminPasswordSalt);
            AuthHelper.CreatePasswordHash("User123!", out byte[] userPasswordHash, out byte[] userPasswordSalt);

            var users = new List<User>
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@artgallery.com",
                    FirstName = "Admin",
                    LastName = "User",
                    PasswordHash = adminPasswordHash,
                    PasswordSalt = adminPasswordSalt,
                    Role = "Admin",
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    Username = "john",
                    Email = "john@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    PasswordHash = userPasswordHash,
                    PasswordSalt = userPasswordSalt,
                    Role = "Customer",
                    CreatedAt = DateTime.Now
                }
            };

            await context.Users.AddRangeAsync(users);
        }

        private async Task SeedCategoriesAsync(ArtGalleryContext context)
        {
            var categories = new List<Category>
            {
                new Category { Name = "Paintings", Description = "Original paintings in various media" },
                new Category { Name = "Sculptures", Description = "Three-dimensional art pieces" },
                new Category { Name = "Photography", Description = "Fine art photography prints" },
                new Category { Name = "Digital Art", Description = "Art created or presented using digital technology" },
                new Category { Name = "Drawings", Description = "Sketches, illustrations and drawing artworks" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync(); // Save categories first to get their IDs
        }

        public Dictionary<string, byte[]> ConvertAllImagesToByteArrays(string directoryPath)
        {
            Dictionary<string, byte[]> imagesData = new Dictionary<string, byte[]>();

            if (Directory.Exists(directoryPath))
            {
                foreach (string filePath in Directory.GetFiles(directoryPath))
                {
                    byte[] imageBytes = File.ReadAllBytes(filePath);
                    string fileName = Path.GetFileName(filePath);
                    imagesData[fileName] = imageBytes; // Store image bytes with file name as key
                }
            }

            return imagesData; // Returns a dictionary of image file names and their byte arrays
        }

        private async Task SeedArtWorksAsync(ArtGalleryContext context)
        {
            // Get category IDs after they've been saved
            var categories = await context.Categories.ToListAsync();
            var paintingsCategory = categories.First(c => c.Name == "Paintings");
            var sculpturesCategory = categories.First(c => c.Name == "Sculptures");
            var photographyCategory = categories.First(c => c.Name == "Photography");
            var digitalArtCategory = categories.First(c => c.Name == "Digital Art");
            var drawingsCategory = categories.First(c => c.Name == "Drawings");

            string imagesPath = Path.Combine(_environment.WebRootPath, "images");
            Console.WriteLine($"Looking for images in: {imagesPath}");

            if (!Directory.Exists(imagesPath))
            {
                Console.WriteLine($"Images directory not found at {imagesPath}. Creating directory...");
                Directory.CreateDirectory(imagesPath);
            }

            // First, try to load all available images
            var imageFiles = Directory.GetFiles(imagesPath, "*.*")
                .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                              file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                              file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Console.WriteLine($"Found {imageFiles.Count} image files in directory");

            // Create a dictionary of image bytes for quick lookup
            var imageBytesDict = new Dictionary<string, byte[]>();
            foreach (var file in imageFiles)
            {
                try
                {
                    var fileName = Path.GetFileName(file);
                    var imageBytes = await File.ReadAllBytesAsync(file);
                    imageBytesDict[fileName] = imageBytes;
                    Console.WriteLine($"Loaded image {fileName} ({imageBytes.Length} bytes)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image {file}: {ex.Message}");
                }
            }

            // Create artworks with images if available
            var artworks = new List<ArtWork>
            {
                new ArtWork
                {
                    Title = "Sunset Horizon",
                    Artist = "Maria Johnson",
                    Description = "A vibrant oil painting depicting a sunset over the ocean",
                    Price = 1200.00m,
                    Year = 2023,
                    Medium = "Oil on Canvas",
                    Dimensions = "24\" x 36\"",
                    CategoryId = paintingsCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.Now,
                    ImageFileName = "sunset_horizon.jpg",
                    ImageBytes = imageBytesDict.GetValueOrDefault("sunset_horizon.jpg")
                },
                new ArtWork
                {
                    Title = "Abstract Emotions",
                    Artist = "David Chen",
                    Description = "An abstract expression of human emotions using bold colors and shapes",
                    Price = 950.00M,
                    Year = 2022,
                    Medium = "Acrylic on Canvas",
                    Dimensions = "30\" x 30\"",
                    CategoryId = paintingsCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.Now,
                    ImageFileName = "abstract_emotions.jpg",
                    ImageBytes = imageBytesDict.GetValueOrDefault("abstract_emotions.jpg")
                },
                new ArtWork
                {
                    Title = "Bronze Guardian",
                    Artist = "Sophie Miller",
                    Description = "A bronze sculpture of a mythical guardian figure",
                    Price = 2500.00M,
                    Year = 2021,
                    Medium = "Bronze",
                    Dimensions = "18\" x 8\" x 6\"",
                    CategoryId = sculpturesCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.Now,
                    ImageFileName = "bronze_guardian.jpg",
                    ImageBytes = imageBytesDict.GetValueOrDefault("bronze_guardian.jpg")
                },
                new ArtWork
                {
                    Title = "Urban Reflections",
                    Artist = "James Wilson",
                    Description = "Black and white photography capturing city reflections in rain",
                    Price = 450.00M,
                    Year = 2023,
                    Medium = "Photography, Archival Print",
                    Dimensions = "16\" x 20\"",
                    CategoryId = photographyCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.Now,
                    ImageFileName = "urban_reflections.jpg",
                    ImageBytes = imageBytesDict.GetValueOrDefault("urban_reflections.jpg")
                },
                new ArtWork
                {
                    Title = "Digital Dreamscape",
                    Artist = "Alex Rivera",
                    Description = "A surreal digital landscape with floating islands and strange creatures",
                    Price = 350.00M,
                    Year = 2024,
                    Medium = "Digital Art, Limited Print",
                    Dimensions = "24\" x 36\"",
                    CategoryId = digitalArtCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.Now,
                    ImageFileName = "digital_dreamscape.jpg",
                    ImageBytes = imageBytesDict.GetValueOrDefault("digital_dreamscape.jpg")
                },
                new ArtWork
                {
                    Title = "Charcoal Study",
                    Artist = "Emma Thompson",
                    Description = "A detailed charcoal drawing of a human figure",
                    Price = 600.00M,
                    Year = 2022,
                    Medium = "Charcoal on Paper",
                    Dimensions = "18\" x 24\"",
                    CategoryId = drawingsCategory.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.Now,
                    ImageFileName = "charcoal_study.jpg",
                    ImageBytes = imageBytesDict.GetValueOrDefault("charcoal_study.jpg")
                }
            };

            // Log the status of each artwork's image
            foreach (var artwork in artworks)
            {
                Console.WriteLine($"Artwork '{artwork.Title}':");
                Console.WriteLine($"- ImageFileName: {artwork.ImageFileName}");
                Console.WriteLine($"- ImageBytes: {(artwork.ImageBytes != null ? $"{artwork.ImageBytes.Length} bytes" : "null")}");
                Console.WriteLine($"- CategoryId: {artwork.CategoryId}");
            }

            await context.ArtWorks.AddRangeAsync(artworks);
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {artworks.Count} artworks with images.");
        }
    }
}
