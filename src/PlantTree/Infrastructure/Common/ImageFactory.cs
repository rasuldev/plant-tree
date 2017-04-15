using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.Images;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using PlantTree.Data.Entities;

namespace PlantTree.Infrastructure.Common
{
    public class ImageFactory : CommonImageFactory
    {
        protected Dictionary<ImageKind, ImageSize> SmallImageSizes { get; set; }

        /// <summary>
        /// Creates <see cref="ImageProcessorCore.Image"/> entity.
        /// </summary>
        public ImageFactory(IHostingEnvironment env, Dictionary<ImageKind, ImageSize> smallImageSizes) 
            : base(env)
        {
            SmallImageSizes = smallImageSizes;
        }

        private async Task<Image> CreateImage(IFormFile imgFile, ImageKind kind)
        {
            var size = SmallImageSizes[kind];
            var image = new Image(imgFile.FileName, kind);
            await SetImageFile(image, imgFile, size);
            return image;
        }

        public async Task<Image> CreateProjectImage(IFormFile imgFile)
        {
            return await CreateImage(imgFile, ImageKind.Project);
        }

        public async Task<Image> CreateUserImage(IFormFile imgFile)
        {
            return await CreateImage(imgFile, ImageKind.User);
        }

        public async Task<Image> CreateNewsImage(IFormFile imgFile)
        {
            return await CreateImage(imgFile, ImageKind.News);
        }
    }
}