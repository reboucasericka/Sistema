namespace Sistema.Helpers
{
    public interface IStorageHelper
    {
        Task<string> UploadAsync(IFormFile file, string folder);
        Task DeleteAsync(string fileName, string folder);
    }
}
