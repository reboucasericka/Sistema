namespace Sistema.Helpers
{
    public interface IImageUrlHelper
    {
        string GetImageUrl(Guid imageId, string folder);
        string GetNoImageUrl();
    }
}
