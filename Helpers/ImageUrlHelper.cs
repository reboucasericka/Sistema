using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Sistema.Helpers
{
    public class ImageUrlHelper : IImageUrlHelper
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly string _mode;

        public ImageUrlHelper(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
            _mode = configuration["Storage:Mode"] ?? "Local";
        }

        public string GetImageUrl(Guid imageId, string folder)
        {
            if (imageId == Guid.Empty)
                return GetNoImageUrl();

            if (_mode.Equals("Blob", StringComparison.OrdinalIgnoreCase))
            {
                // URL do Azure Blob Storage
                var blobAccount = _configuration["Storage:Blob:AccountName"] ?? "supershopcontaarmazename";
                return $"https://{blobAccount}.blob.core.windows.net/{folder}/{imageId}.png";
            }
            else
            {
                // URL local
                return $"/uploads/{folder}/{imageId}.png";
            }
        }

        public string GetNoImageUrl()
        {
            if (_mode.Equals("Blob", StringComparison.OrdinalIgnoreCase))
            {
                return "https://supershoptpsi-ftc4dnb4bcbkgmhw.westeurope-01.azurewebsites.net/images/noimage.png";
            }
            else
            {
                return "/images/noimage.png";
            }
        }
    }
}
