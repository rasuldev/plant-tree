using System.IO;
using ImageProcessorCore;

namespace Common.Images
{
    public class ImageResizer : IImageResizer
    {
        /// <summary>
        /// Resizes image. If you set only one dimension the other will be taken proportionally.
        /// </summary>
        /// <param name="imgPath"></param>
        /// <param name="resizedImgPath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Resize(string imgPath, string resizedImgPath, int width = 0, int height = 0)
        {
            if (width == 0 && height == 0)
                return;
            using (FileStream stream = File.OpenRead(imgPath))
            using (FileStream output = File.OpenWrite(resizedImgPath))
            {
                ImageProcessorCore.Image image = new ImageProcessorCore.Image(stream);
                if (width == 0)
                {
                    width = image.Width * height / image.Height;
                }
                else if (height == 0)
                {
                    height = image.Height * width / image.Width;
                }

                image.Resize(width, height)
                     .Save(output);
            }
        }
    }
}