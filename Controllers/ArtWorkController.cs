using ArtGallery.API.Data;
using ArtGallery.API.DTOs;
using ArtGallery.API.Models;
using ArtGallery.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace ArtGallery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtWorkController : ControllerBase
    {
        private readonly ArtGalleryContext _context;
        private readonly IWebHostEnvironment _environment;

        public ArtWorkController(ArtGalleryContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/ArtWork
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArtWorkDto>>> GetArtWorks()
        {
            var artworks = await _context.ArtWorks
                .Include(a => a.Category)
                .AsNoTracking()
                .ToListAsync();

            var artworkDtos = artworks.Select(a => new ArtWorkDto
            {
                Id = a.Id,
                Title = a.Title,
                Artist = a.Artist,
                Description = a.Description,
                Price = a.Price,
                Year = a.Year,
                Medium = a.Medium,
                Dimensions = a.Dimensions,
                CategoryId = a.CategoryId,
                CategoryName = a.Category?.Name,
                IsAvailable = a.IsAvailable,
                CreatedAt = a.CreatedAt,
                ImageBytes = a.ImageBytes != null && a.ImageBytes.Length > 0 ? a.ImageBytes : null,
                ImageFileName = a.ImageFileName
            }).ToList();

            return Ok(artworkDtos);
        }

        // GET: api/ArtWork/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ArtWorkDto>> GetArtWork(int id)
        {
            var artWork = await _context.ArtWorks
                .Include(a => a.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (artWork == null)
            {
                return NotFound();
            }

            var artWorkDto = new ArtWorkDto
            {
                Id = artWork.Id,
                Title = artWork.Title,
                Artist = artWork.Artist,
                Description = artWork.Description,
                Price = artWork.Price,
                ImageBytes = artWork.ImageBytes != null && artWork.ImageBytes.Length > 0 ? artWork.ImageBytes : null,
                ImageFileName = artWork.ImageFileName,
                Year = artWork.Year,
                Medium = artWork.Medium,
                Dimensions = artWork.Dimensions,
                CategoryId = artWork.CategoryId,
                CategoryName = artWork.Category?.Name,
                IsAvailable = artWork.IsAvailable,
                CreatedAt = artWork.CreatedAt
            };

            return Ok(artWorkDto);
        }

        // GET: api/ArtWork/image/{id}
        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetImage(int id)
        {
            try
            {
                var artWork = await _context.ArtWorks.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

                if (artWork == null || artWork.ImageBytes == null || artWork.ImageBytes.Length == 0)
                {
                    return NotFound("Image not found for this artwork.");
                }

                // Determine content type based on file extension from ImageFileName
                var contentType = "application/octet-stream"; // Default
                if (!string.IsNullOrEmpty(artWork.ImageFileName))
                {
                    var fileExtension = Path.GetExtension(artWork.ImageFileName)?.ToLowerInvariant();
                    switch (fileExtension)
                    {
                        case ".jpg":
                        case ".jpeg":
                            contentType = "image/jpeg";
                            break;
                        case ".png":
                            contentType = "image/png";
                            break;
                        case ".gif":
                            contentType = "image/gif";
                            break;
                        // Add more cases for other image types if needed
                    }
                }

                return File(artWork.ImageBytes, contentType);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error retrieving image for artwork ID {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/ArtWork/category/1
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ArtWorkDto>>> GetArtWorksByCategory(int categoryId)
        {
            var artWorks = await _context.ArtWorks
                .Include(a => a.Category)
                .Where(a => a.CategoryId == categoryId)
                .Select(a => new ArtWorkDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Artist = a.Artist,
                    Description = a.Description,
                    Price = a.Price,
                    ImageBytes = a.ImageBytes,
                    ImageFileName = a.ImageFileName,
                    Year = a.Year,
                    Medium = a.Medium,
                    Dimensions = a.Dimensions,
                    CategoryId = a.CategoryId,
                    CategoryName = a.Category.Name,
                    IsAvailable = a.IsAvailable,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(artWorks);
        }

        // POST: api/ArtWork/batch
        [HttpPost("batch")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<ActionResult<IEnumerable<ArtWorkDto>>> GetArtWorksBatch([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("No ids provided");

            var artWorks = await _context.ArtWorks
                .Include(a => a.Category)
                .Where(a => ids.Contains(a.Id))
                .Select(a => new ArtWorkDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Artist = a.Artist,
                    Description = a.Description,
                    Price = a.Price,
                    ImageBytes = a.ImageBytes,
                    ImageFileName = a.ImageFileName,
                    Year = a.Year,
                    Medium = a.Medium,
                    Dimensions = a.Dimensions,
                    CategoryId = a.CategoryId,
                    CategoryName = a.Category.Name,
                    IsAvailable = a.IsAvailable,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            if (!artWorks.Any())
                return NotFound("No art works found with the provided ids");

            return Ok(artWorks);
        }

        // POST: api/ArtWork
        [HttpPost]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<ActionResult<ArtWorkDto>> CreateArtWork([FromForm] CreateArtWorkDto createArtWorkDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (createArtWorkDto.ImageFile == null || createArtWorkDto.ImageFile.Length == 0)
                return BadRequest("Image file is required");

            var category = await _context.Categories.FindAsync(createArtWorkDto.CategoryId);
            if (category == null)
                return BadRequest("Invalid category ID");

            byte[]? imageBytes = null;
            string? imageFileName = null;

            try
            {
                using (var ms = new MemoryStream())
                {
                    await createArtWorkDto.ImageFile.CopyToAsync(ms);
                    imageBytes = ms.ToArray();
                    
                    if (imageBytes.Length == 0)
                        return BadRequest("Invalid image file");

                    imageFileName = createArtWorkDto.ImageFile.FileName;
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing image: {ex.Message}");
            }

            var artwork = new ArtWork
            {
                Title = createArtWorkDto.Title,
                Artist = createArtWorkDto.Artist,
                Description = createArtWorkDto.Description,
                Price = createArtWorkDto.Price,
                Year = createArtWorkDto.Year,
                Medium = createArtWorkDto.Medium,
                Dimensions = createArtWorkDto.Dimensions,
                CategoryId = createArtWorkDto.CategoryId,
                IsAvailable = true,
                CreatedAt = DateTime.Now,
                ImageBytes = imageBytes,
                ImageFileName = imageFileName
            };

            _context.ArtWorks.Add(artwork);
            await _context.SaveChangesAsync();

            // Verify the image was saved
            var savedArtwork = await _context.ArtWorks
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == artwork.Id);

            if (savedArtwork?.ImageBytes == null || savedArtwork.ImageBytes.Length == 0)
            {
                // If image wasn't saved properly, delete the artwork
                _context.ArtWorks.Remove(artwork);
                await _context.SaveChangesAsync();
                return BadRequest("Failed to save image");
            }

            var artworkDto = new ArtWorkDto
            {
                Id = artwork.Id,
                Title = artwork.Title,
                Artist = artwork.Artist,
                Description = artwork.Description,
                Price = artwork.Price,
                Year = artwork.Year,
                Medium = artwork.Medium,
                Dimensions = artwork.Dimensions,
                CategoryId = artwork.CategoryId,
                CategoryName = category.Name,
                IsAvailable = artwork.IsAvailable,
                CreatedAt = artwork.CreatedAt,
                ImageBytes = savedArtwork.ImageBytes,
                ImageFileName = savedArtwork.ImageFileName
            };

            return CreatedAtAction(nameof(GetArtWork), new { id = artwork.Id }, artworkDto);
        }

        // PUT: api/ArtWork/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateArtWork(int id, [FromForm] UpdateArtWorkDto updateArtWorkDto)
        {
            if (id != updateArtWorkDto.Id)
                return BadRequest();

            var existingArtwork = await _context.ArtWorks.FindAsync(id);
            if (existingArtwork == null)
                return NotFound();

            var category = await _context.Categories.FindAsync(updateArtWorkDto.CategoryId);
            if (category == null)
                return BadRequest("Invalid category ID");

            byte[]? imageBytes = existingArtwork.ImageBytes;
            string? imageFileName = existingArtwork.ImageFileName;

            if (updateArtWorkDto.ImageFile != null)
            {
                using (var ms = new MemoryStream())
                {
                    await updateArtWorkDto.ImageFile.CopyToAsync(ms);
                    imageBytes = ms.ToArray();
                    imageFileName = updateArtWorkDto.ImageFile.FileName;
                }
            }

            existingArtwork.Title = updateArtWorkDto.Title;
            existingArtwork.Artist = updateArtWorkDto.Artist;
            existingArtwork.Description = updateArtWorkDto.Description;
            existingArtwork.Price = updateArtWorkDto.Price;
            existingArtwork.Year = updateArtWorkDto.Year;
            existingArtwork.Medium = updateArtWorkDto.Medium;
            existingArtwork.Dimensions = updateArtWorkDto.Dimensions;
            existingArtwork.CategoryId = updateArtWorkDto.CategoryId;
            existingArtwork.IsAvailable = updateArtWorkDto.IsAvailable;
            existingArtwork.ImageBytes = imageBytes;
            existingArtwork.ImageFileName = imageFileName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArtWorkExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/ArtWork/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteArtWork(int id)
        {
            var artWork = await _context.ArtWorks.FindAsync(id);
            if (artWork == null)
            {
                return NotFound();
            }

            // Check if the artwork is part of any order
            var isInOrder = await _context.OrderItems.AnyAsync(oi => oi.ArtWorkId == id);
            if (isInOrder)
            {
                // Instead of deleting, mark as unavailable
                artWork.IsAvailable = false;
            }
            else
            {
                // Delete the image file if it exists
                if (!string.IsNullOrEmpty(artWork.ImageFileName))
                {
                    ArtImageBytesHelper.DeleteImageFromFileSystem(artWork.ImageFileName);
                }

                _context.ArtWorks.Remove(artWork);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/ArtWork/process-database-images
        [HttpPost("process-database-images")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> ProcessDatabaseImages()
        {
            try
            {
                // Retrieve all artworks that have image bytes stored in the database
                var artworksWithImages = await _context.ArtWorks
                    .Where(a => a.ImageBytes != null && a.ImageBytes.Length > 0)
                    .ToListAsync();

                if (!artworksWithImages.Any())
                {
                    return Ok(new { Message = "No artworks with image bytes found in the database." });
                }

                int processedCount = 0;
                var processedArtworkInfo = new List<object>();

                // Iterate through artworks and perform basic processing (e.g., verification, logging)
                foreach (var artwork in artworksWithImages)
                {
                    // *** Add your specific image processing logic here ***
                    // For now, we'll just report on the image.
                    Console.WriteLine($"Processing image for Artwork ID {artwork.Id}, Title: {artwork.Title}");
                    // You could add image validation, resizing, metadata extraction, etc.
                    // Example: Validate image format
                    // var contentType = ArtImageBytesHelper.GetImageFormat(artwork.ImageBytes);
                    // Console.WriteLine($"  Detected Content Type: {contentType}");
                    // ******************************************************

                    processedCount++;
                    processedArtworkInfo.Add(new { ArtworkId = artwork.Id, Title = artwork.Title, ImageFileName = artwork.ImageFileName, ImageSize = artwork.ImageBytes.Length });
                }

                // You might save changes here if your processing modifies the artwork
                // await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = $"Successfully processed {processedCount} artworks with images from the database.",
                    ProcessedArtworks = processedArtworkInfo
                });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error processing database images: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private bool ArtWorkExists(int id)
        {
            return _context.ArtWorks.Any(e => e.Id == id);
        }
    }
}
