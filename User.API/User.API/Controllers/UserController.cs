using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using User.Api.Data;
using User.API.Exceptions;
using User.Api.Models;

namespace User.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/users")]
    public class UserController : BaseController
    {
        private readonly UserContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(UserContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _context.Users.AsNoTracking().Include(u => u.Properties)
                .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);
            if (user == null)
            {
                throw new UserOperationException($"错误的用户上下文Id{UserIdentity.UserId}");
            }

            return Json(user);
        }

        [Route("")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody] JsonPatchDocument<AppUser> patch)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);
            patch.ApplyTo(user);
            foreach (var property in user.Properties)
            {
                _context.Entry(property).State = EntityState.Detached;
            }

            var originProperties = await
                _context.UserProperties.AsNoTracking().Where(u => u.AppUserId == user.Id).ToListAsync();
            var allProperties = originProperties.Union(user.Properties).Distinct();

            var newProperties = allProperties.Except(originProperties);
            var deleteProperties = originProperties.Except(user.Properties);
            foreach (var deleteProperty in deleteProperties)
            {
                _context.Remove(deleteProperty);
            }

            foreach (var newProperty in newProperties)
            {
                _context.Add(newProperty);
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Json(user);
        }
        [Route("check-or-create")]
        [HttpPost]
        public async Task<IActionResult> CheckOrCreate(string phone)
        {
            //TODO:check phone format
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Phone == phone);
            if (user == null)
            {
                user = new AppUser()
                {
                    Phone = phone
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return Ok(user.Id);
        }
        /// <summary>
        /// 获取用户标签
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("tags")]
        public async Task<IActionResult> GetUserTags()
        {
            return Ok(await _context.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync());
        }
        /// <summary>
        /// 根据手机号查找
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <returns></returns>
        [HttpGet]
        [Route("search/{phone}")]
        public async Task<IActionResult> Search(string phone)
        {
            return Ok(await _context.Users.Include(u=>u.Properties).SingleOrDefaultAsync(u => u.Phone == phone));
        }
        /// <summary>
        /// 更新用户标签数据 
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("tags")]
        public async Task<IActionResult> UpdateUserTags([FromBody] List<string> tags)
        {
            var originTags = await _context.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync();
            var newTags = tags.Except(originTags.Select(t => t.Tag));
            await _context.UserTags.AddRangeAsync(newTags.Select(t =>
                {
                    return new UserTag()
                    {
                        CreateDate = DateTime.Now,
                        UserId = UserIdentity.UserId,
                        Tag = t
                    };
                })
            );
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}