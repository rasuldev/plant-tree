namespace PlantTree.Data
{
    public interface IStaticContentContext
    {
        string Load(string key);
        void Save(string key, string content);
    }
}