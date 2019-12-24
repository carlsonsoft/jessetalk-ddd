using System.Threading.Tasks;

namespace User.Identity.Services
{
    public interface IUserService
    {
        /// <summary>
        /// 验证手机号是否存在，如不存在则创建新的用户
        /// </summary>
        /// <param name="phone">手机号</param>
       Task<int> CheckOrCreate(string phone);
    }
}