using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace ArtGallery.API.Helpers
{
    public static class ArtImageBytesHelper
    {
        private const string ImageStoragePath = "wwwroot/images";
        private const int MaxImageSizeInBytes = 5 * 1024 * 1024; // 5MB

        /// <summary>
        /// Converts an IFormFile image to a byte array
        /// </summary>
        /// <param name="imageFile">The image file to convert</param>
        /// <returns>Byte array containing the image data</returns>
        public static async Task<byte[]> ConvertImageToByteArrayAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("Invalid image file");

            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Converts a file from the file system to a byte array
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Byte array containing the file data</returns>
        public static async Task<byte[]> ConvertFileToByteArrayAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            return await File.ReadAllBytesAsync(filePath);
        }

        /// <summary>
        /// Converts all images from a directory to byte arrays
        /// </summary>
        /// <param name="directoryPath">Path to the directory containing images</param>
        /// <returns>Dictionary of filename and corresponding byte array</returns>
        public static async Task<Dictionary<string, byte[]>> ConvertDirectoryImagesToByteArraysAsync(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            var result = new Dictionary<string, byte[]>();
            var files = Directory.GetFiles(directoryPath, "*.*");

            foreach (var file in files)
            {
                try
                {
                    var imageBytes = await File.ReadAllBytesAsync(file);
                    result.Add(Path.GetFileName(file), imageBytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {file}: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// Gets an image from the file system
        /// </summary>
        public static async Task<byte[]> GetImageFromFileSystemAsync(string fileName)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), ImageStoragePath, fileName);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Image file not found: {fileName}");

            return await File.ReadAllBytesAsync(filePath);
        }

        /// <summary>
        /// Gets the content type (MIME type) for an image based on its bytes
        /// </summary>
        [SupportedOSPlatform("windows")]
        public static string GetImageFormat(byte[] imageBytes)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("Image format detection is only supported on Windows.");
            }

            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("Invalid image data");

            using (var ms = new MemoryStream(imageBytes))
            {
                using (var img = Image.FromStream(ms))
                {
                    if (img.RawFormat.Equals(ImageFormat.Jpeg))
                        return "image/jpeg";
                    if (img.RawFormat.Equals(ImageFormat.Png))
                        return "image/png";
                    if (img.RawFormat.Equals(ImageFormat.Gif))
                        return "image/gif";
                    return "application/octet-stream";
                }
            }
        }

        /// <summary>
        /// Processes and saves an uploaded image
        /// </summary>
        [SupportedOSPlatform("windows")]
        public static async Task<(byte[] imageBytes, string fileName)> ProcessAndSaveImageAsync(IFormFile imageFile)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("Image processing is only supported on Windows.");
            }

            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("Invalid image file");

            if (imageFile.Length > MaxImageSizeInBytes)
                throw new ArgumentException($"Image size exceeds the maximum limit of {MaxImageSizeInBytes / (1024 * 1024)}MB");

            // Generate a unique filename
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), ImageStoragePath, fileName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), ImageStoragePath));

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Read the file back as bytes
            byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

            return (imageBytes, fileName);
        }

        /// <summary>
        /// Deletes an image from the file system
        /// </summary>
        public static void DeleteImageFromFileSystem(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), ImageStoragePath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [SupportedOSPlatform("windows")]
        public static byte[]? GetImageBytesFromStream(Stream stream)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("Image operations are only supported on Windows.");
            }

            try
            {
                using var image = Image.FromStream(stream);
                using var ms = new MemoryStream();

                if (image.RawFormat.Equals(ImageFormat.Jpeg))
                {
                    image.Save(ms, ImageFormat.Jpeg);
                }
                else if (image.RawFormat.Equals(ImageFormat.Png))
                {
                    image.Save(ms, ImageFormat.Png);
                }
                else if (image.RawFormat.Equals(ImageFormat.Gif))
                {
                    image.Save(ms, ImageFormat.Gif);
                }
                else
                {
                    // Default to JPEG if format is not recognized
                    image.Save(ms, ImageFormat.Jpeg);
                }

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
                return null;
            }
        }
    }
}
