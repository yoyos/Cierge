using System.Threading.Tasks;

namespace CiergeLib.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
