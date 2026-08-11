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

        var guid = Guid.NewGuid();
        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = $"{guid}{fileExtension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Retorna o GUID como string
        return guid.ToString();
    }

    public void DeleteImage(string imageId, string folder)
    {
        if (string.IsNullOrEmpty(imageId)) return;

        // Busca por arquivos que começam com o GUID
        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", folder);
        if (!Directory.Exists(uploadsPath)) return;

        var files = Directory.GetFiles(uploadsPath, $"{imageId}.*");
        foreach (var file in files)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }
    }
}
