using Contacts.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Contacts.API.Data
{
    public interface IContactApplyRequestRepository
    {
        /// <summary>
        /// 添加申请好友请求
        /// </summary>
        /// <param name="reqeust"></param>
        /// <returns></returns>
        Task<bool> AddRequestAsync(ContactApplyRequest reqeust,CancellationToken cancellationToken);

        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="applierId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ApprovalAsync(int userId, int applierId, CancellationToken cancellationToken);

        /// <summary>
        /// 获取好友申请列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<ContactApplyRequest>> GetRequestListAsync(int userId, CancellationToken cancellationToken);
    }
}
