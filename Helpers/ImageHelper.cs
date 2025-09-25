﻿using System.IO;

namespace Sistema.Helpers
{
    public class ImageHelper : IImageHelper
    {
        public async Task<string> UploadImageAsync(IFormFile ImageProductFile, string folder)
        {
            string guid = Guid.NewGuid().ToString(); // 
            string file = $"{guid}.jpg"; // 

            string path = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\images\\{folder}", file); 


            using (FileStream stream = new FileStream(path, FileMode.Create)) // 
            {
                await ImageProductFile.CopyToAsync(stream); //
            }
            return $"~/images/{folder}/{file}"; // 
        }
    }
}
