using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sistema.Helpers
{
    public class BlobHelper : IBlobHelper
    {
        private readonly CloudBlobClient _blobClient;//ligar ao contentor
       

        public BlobHelper(IConfiguration configuration)
        {
            try
            {
                string keys = configuration["Blob:ConnectionString"]; //pegar a chave de acesso ao blob
                if (string.IsNullOrEmpty(keys))
                {
                    throw new ArgumentException("Connection string do Blob Storage não configurada");
                }
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(keys); //criar a conta de armazenamento
                _blobClient = storageAccount.CreateCloudBlobClient(); //criar o cliente do blob
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao configurar Blob Storage: {ex.Message}", ex);
            }
        }
        public async Task<Guid> UploadBlobAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Arquivo não pode ser nulo ou vazio");
            }
            
            Stream stream = file.OpenReadStream();
            return await UploadStreamAsync(stream, containerName);
        }

        public async Task<Guid> UploadBlobAsync(byte[] file, string containerName)
        {
            MemoryStream stream = new MemoryStream(file);
            return await UploadStreamAsync(stream, containerName);
        }

        public async Task<Guid> UploadBlobAsync(string image, string containerName)
        {
            Stream stream = File.OpenRead(image);
            return await UploadStreamAsync(stream, containerName);
        }

        private async Task<Guid> UploadStreamAsync(Stream stream, string containerName)
        {
            try
            {
                Guid name = Guid.NewGuid();
                CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
                await container.CreateIfNotExistsAsync();
                CloudBlockBlob blockBlob = container.GetBlockBlobReference($"{name}.png");
                await blockBlob.UploadFromStreamAsync(stream);
                return name;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao fazer upload do arquivo: {ex.Message}", ex);
            }
        }

        public async Task CreateContainerIfNotExistsAsync(string containerName)
        {
            try
            {
                CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
                await container.CreateIfNotExistsAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar container '{containerName}': {ex.Message}", ex);
            }
        }
    }
}
