namespace Common.Images
{
    public interface IImageResizer
    {
        void Resize(string imgPath, string resizedImgPath, int width, int height);
    }
}