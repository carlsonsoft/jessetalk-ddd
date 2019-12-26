using System.Collections.Generic;
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
        /// <summary>
        /// 添加联系人信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="contact">联系人信息</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> AddContactAsync(int userId, BaseUserInfo contact, CancellationToken cancellationToken);

        Task<List<Models.Contact>> GetContactsAsync(int userId,CancellationToken cancellationToken);
        Task<bool> TagContactAsync(int userId, int contactId, List<string> tags, CancellationToken cancellationToken);
    }
}