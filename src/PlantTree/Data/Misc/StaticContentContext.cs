using System.IO;

namespace PlantTree.Data.Misc
{
    public class StaticContentContext : IStaticContentContext
    {
        private readonly string _baseFolder;
        public StaticContentContext(string baseFolder)
        {
            _baseFolder = baseFolder;
        }

        public string Load(string key)
        {
            var targetFile = Path.Combine(_baseFolder, key + ".html");
            return File.Exists(targetFile) ? File.ReadAllText(targetFile) : null;
        }

        public void Save(string key, string content)
        {
            var targetFile = Path.Combine(_baseFolder, key + ".html");
            File.WriteAllText(targetFile, content);
        }

        public string LoadAbout()
        {
            return Load("about");
        }

        public void SaveAbout(string content)
        {
            Save("about", content);
        }

    }
}