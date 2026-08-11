using Microsoft.AspNetCore.Http;

namespace Sistema.Helpers
{
    public interface IImageHelper
    {
        Task<string> UploadImageAsync(IFormFile file, string folder);
        void DeleteImage(string imageName, string folder);
    }
}
