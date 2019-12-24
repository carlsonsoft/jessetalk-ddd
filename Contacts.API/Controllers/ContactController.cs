using System;
using System.Threading;
using System.Threading.Tasks;
using Contacts.API.Data;
using Contacts.API.Models;
using Contacts.API.Services;
using Contacts.API.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Contact.API.Controllers
{
    [Route("api/[controller]")]
    public class ContactController : BaseController
    {
        private readonly IContactApplyRequestRepository _contactApplyRequestRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IUserService _userService;

        public ContactController(IContactApplyRequestRepository contactApplyRequestRepository, IUserService userService, IContactRepository contactRepository)
        {
            _contactApplyRequestRepository = contactApplyRequestRepository;
            _userService = userService;
            _contactRepository = contactRepository;
        }

        /// <summary>
        /// 获取好友申请列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("apply-requests")]
        public async Task<IActionResult> GetApplyRequests()
        {
            var result =
                await _contactApplyRequestRepository.GetRequestListAsync(UserIdentity.UserId, CancellationToken.None);
            return Ok(result);
        }

        /// <summary>
        /// 添加好友请求
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        [Route("apply-requests")]
        public async Task<IActionResult> AddApplyRequest(int userId, CancellationToken cancellationToken)
        {
            var baseUserInfo = await _userService.GetBaseUserInfoAsync(userId);
            if (baseUserInfo == null)
            {
                throw new Exception("用户参数有误");
            }

            var result = await _contactApplyRequestRepository.AddRequestAsync(new ContactApplyRequest()
            {
                ApplierId = UserIdentity.UserId,
                UserId = userId,
                Name = baseUserInfo.Name,
                Company = baseUserInfo.Company,
                Title = baseUserInfo.Title,
                ApplyTime = DateTime.Now,
                Avatar = baseUserInfo.Avatar
            }, cancellationToken);
            if (!result)
            {
                return BadRequest();
            }

            return Ok();
        }

        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <param name="applierId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("apply-requests")]
        public async Task<IActionResult> ApprovalApplyRequest(int applierId,CancellationToken cancellationToken)
        {
            var result = await _contactApplyRequestRepository.ApprovalAsync(UserIdentity.UserId, applierId, cancellationToken);
            if (!result)
            {
                return BadRequest();
            }
            var applierInfo = await _userService.GetBaseUserInfoAsync(applierId);
            var userInfo = await _userService.GetBaseUserInfoAsync(UserIdentity.UserId);
            await _contactRepository.AddContactAsync(UserIdentity.UserId, applierInfo, cancellationToken);           
            await _contactRepository.AddContactAsync(applierId, userInfo, cancellationToken);           
            return Ok();
        }
    }
}