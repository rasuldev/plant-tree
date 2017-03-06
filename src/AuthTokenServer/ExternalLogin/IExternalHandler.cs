using System.Threading.Tasks;

namespace AuthTokenServer.ExternalLogin
{
    public interface IExternalHandler
    {
        Task<UserInfo> GetUserInfo(string token);
        //Task<string> GetUserInfoRaw(string idToken);
    }
}