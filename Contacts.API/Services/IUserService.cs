using System.Threading.Tasks;
using Contacts.API.Dtos;

namespace Contacts.API.Services
{
    public interface IUserService
    {
        Task<BaseUserInfo> GetBaseUserInfoAsync(int userId);
    }
}