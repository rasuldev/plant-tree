using AuthTokenServer.ExternalLogin;

namespace PlantTree.Models.Api
{
    public class UserInfo
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Birthday { get; set; }
    }
}