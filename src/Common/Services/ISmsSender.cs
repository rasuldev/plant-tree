using System.Threading.Tasks;

namespace Common.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
