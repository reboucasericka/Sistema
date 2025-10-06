using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Sistema.Helpers
{
    public class ImageHelper : IImageHelper
    {
        private readonly IWebHostEnvironment _environment;

        public ImageHelper(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

    public async Task<string> UploadImageAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            return string.Empty;

        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", folder);
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Retorna apenas o nome do arquivo
        return uniqueFileName;
    }

    public void DeleteImage(string imagePath, string folder)
    {
        if (string.IsNullOrEmpty(imagePath)) return;

        // Constrói o caminho completo usando o nome do arquivo
        var fullPath = Path.Combine(_environment.WebRootPath, "uploads", folder, imagePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
    }
}
