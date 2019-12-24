using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using User.Identity.Services;

namespace User.Identity.Authentication
{
    public class SmsAuthCodeValidator:IExtensionGrantValidator
    {
        private readonly IUserService _userService;
        private readonly IAuthCodeService _codeService;
        public string GrantType => "sms_auth_code";

        public SmsAuthCodeValidator(IUserService userService,IAuthCodeService codeService)
        {
            _userService = userService;
            _codeService = codeService;
        }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var phone = context.Request.Raw["phone"];
            var authCode = context.Request.Raw["auth_code"];
            var errorGrantValidationResult = new GrantValidationResult(TokenRequestErrors.InvalidGrant);

            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(authCode))
            {
                context.Result = errorGrantValidationResult;
                return;
            }

            if (!_codeService.Validator(phone,authCode))
            {
                context.Result = errorGrantValidationResult;
                return;
            }

            var userId = await _userService.CheckOrCreate(phone);
            if (userId <= 0)
            {
                context.Result = errorGrantValidationResult;
                return;
            }
            context.Result = new GrantValidationResult(userId.ToString(),GrantType);
        }
    }
}
