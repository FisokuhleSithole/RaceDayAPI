using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RaceDay.Api.Services
{
    public class BlobStorageService
    {
        private readonly IWebHostEnvironment _env;

        public BlobStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Simulates uploading to Azure Blob Storage by saving locally to wwwroot
        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("No file supplied.");

            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the local URL just like Azure would
            return $"/uploads/{uniqueFileName}";
        }
    }
}