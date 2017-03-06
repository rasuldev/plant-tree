using AuthTokenServer.ExternalLogin;

namespace PlantTree.Models.Api
{
    public class UserInfoModel
    {
        public string Name { get; set; }
        public Gender? Gender { get; set; }
        public string Birthday { get; set; }
    }
}