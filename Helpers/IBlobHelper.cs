using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Sistema.Helpers
{
    public interface IBlobHelper
    {
        Task<Guid> UploadBlobAsync(IFormFile file, string containerName);//metodo de criar um metodo para fazer o upload do blob

        Task<Guid> UploadBlobAsync(byte[] file, string containerName); //metodo de criar um 3 maneiras diferentes  bytes metodo para fazer o upload do blob

        Task<Guid> UploadBlobAsync(string image, string containerName);
    }
}
