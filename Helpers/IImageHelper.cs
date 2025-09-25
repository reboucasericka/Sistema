namespace Sistema.Helpers
{
    public interface IImageHelper
    {
        Task<string> UploadImageAsync(IFormFile ImageProductFile, string folder);
        
    }
}
