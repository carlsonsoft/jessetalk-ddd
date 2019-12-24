using System.Threading;
using System.Threading.Tasks;
using Contacts.API.Dtos;

namespace Contacts.API.Data
{
    public interface IContactRepository
    {
        /// <summary>
        /// 更新联系人信息
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateContactInfoAsync(BaseUserInfo userInfo, CancellationToken cancellationToken);

        Task<bool> AddContactAsync(int userId, BaseUserInfo contact, CancellationToken cancellationToken);
    }
}