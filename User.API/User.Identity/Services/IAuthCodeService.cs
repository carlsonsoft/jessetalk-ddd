namespace User.Identity.Services
{
    public interface IAuthCodeService
    {
        /// <summary>
        /// 验证手机号和验证码是否正确
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="authCode">验证码</param>
        /// <returns></returns>
        bool Validator(string phone, string authCode);
    }
}