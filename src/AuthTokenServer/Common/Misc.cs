using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace AuthTokenServer.Common
{
    static class Misc
    {
        public static X509Certificate2 GetX509Certificate2(string path, string password)
        {
            X509Certificate2 certificate2 = null;
            using (var memoryStream = new MemoryStream())
            {
                using (
                    var fileStream =
                        new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(memoryStream);
                }
                certificate2 = new X509Certificate2(memoryStream.ToArray(), password);
            }
            return certificate2;
        }
    }
}