namespace Common.Images
{
    public struct ImageSize
    {
        public int Width, Height;

        public ImageSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public ImageSize(string width, string height) : this(int.Parse(width ?? "0"), int.Parse(height ?? "0"))
        {
        }
    }
}