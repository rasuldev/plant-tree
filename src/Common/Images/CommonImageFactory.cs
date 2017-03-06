using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ImageProcessorCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Common.Images
{
    public class CommonImageFactory
    {
        protected string HostingRoot { get; set; }
        protected IImageResizer Resizer { get; set; } = new ImageResizer();

        /// <summary>
        /// Creates <see cref="Image"/> entity.
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        /// <param name="resizer"></param>
        public CommonImageFactory(IHostingEnvironment hostingEnvironment)
        {
            HostingRoot = hostingEnvironment.WebRootPath;
            //Resizer = resizer;
        }

        protected string ServerPathFor(string url)
        {
            return Path.Combine(HostingRoot, url.Replace('/', '\\').Substring(1));
        }

        public async Task SetImageFile(CommonImage image, IFormFile imageFile, ImageSize sizeOfSmall)
        {
            // FileExt and FileId are used to construct Url and UrlSmall
            image.ImageExt = Path.GetExtension(imageFile.FileName).Substring(1);
            image.ImageId = GenerateUniqueId();
            var imgPath = ServerPathFor(image.Url);
            using (var file = new FileStream(imgPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(file);
            }

            Resizer.Resize(imgPath, ServerPathFor(image.UrlSmall), sizeOfSmall.Width, sizeOfSmall.Height);
        }

        public static string GenerateUniqueId()
        {
            //var name = Path.GetFileNameWithoutExtension(filename);
            return $"image_{DateTime.Now:yyyy-MM-ddTHH-mm-ss-ff}_{Path.GetRandomFileName().Replace(".", "")}";
        }
    }
}