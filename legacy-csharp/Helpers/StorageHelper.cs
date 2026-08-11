using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sistema.Helpers
{
    public class StorageHelper  : IStorageHelper    
    {
        private readonly IBlobHelper _blobHelper;
        private readonly IImageHelper _imageHelper;
        private readonly string _mode;

        public StorageHelper(IBlobHelper blobHelper, IImageHelper imageHelper, IConfiguration configuration)
        {
            _blobHelper = blobHelper;
            _imageHelper = imageHelper;
            _mode = configuration["Storage:Mode"] ?? "Local";
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            try
            {
                if (_mode.Equals("Blob", StringComparison.OrdinalIgnoreCase))
                {
                    // Usa o Azure Blob
                    var guid = await _blobHelper.UploadBlobAsync(file, folder);
                    return guid.ToString();
                }
                else
                {
                    // Usa o armazenamento local
                    return await _imageHelper.UploadImageAsync(file, folder);
                }
            }
            catch (Exception ex) when (ex.Message.Contains("The specified resource does not exist"))
            {
                // Se o container não existe, tenta criar e fazer upload novamente
                if (_mode.Equals("Blob", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        await _blobHelper.CreateContainerIfNotExistsAsync(folder);
                        var guid = await _blobHelper.UploadBlobAsync(file, folder);
                        return guid.ToString();
                    }
                    catch
                    {
                        // Se ainda falhar, retorna string vazia
                        return string.Empty;
                    }
                }
                return string.Empty;
            }
            catch
            {
                // Em caso de qualquer outro erro, retorna string vazia
                return string.Empty;
            }
        }

        public async Task DeleteAsync(string fileName, string folder)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            if (_mode.Equals("Blob", StringComparison.OrdinalIgnoreCase))
            {
                // Blob ainda não tem delete implementado, podes adicionar depois
                await Task.CompletedTask;
            }
            else
            {
                _imageHelper.DeleteImage(fileName, folder);
            }
        }
    }
}
