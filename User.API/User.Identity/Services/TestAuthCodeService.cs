namespace User.Identity.Services
{
    public class TestAuthCodeService:IAuthCodeService
    {
        public bool Validator(string phone, string authCode)
        {
            return true;
        }
    }
}