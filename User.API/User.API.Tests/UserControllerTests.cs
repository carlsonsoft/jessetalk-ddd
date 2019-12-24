using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using User.Api.Controllers;
using User.Api.Data;
using User.Api.Models;
using Xunit;

namespace User.API.Tests
{
    public class UserControllerTests
    {
        private (UserController controller, UserContext userContext) CreateUserController()
        {
            var context = GetContext();
            var logger = new Mock<ILogger<UserController>>().Object;
            var controller = new UserController(context, logger);
            return (controller, context);
        }

        private UserContext GetContext()
        {
            var options = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var userContext = new UserContext(options);
            userContext.Users.Add(new AppUser
            {
                Id=1,
                Name = "carlson"
            });
            userContext.SaveChanges();
            return userContext;
        }

        [Fact]
        public async Task Get_ReturnTrueUser_WithRightParam()
        {
            (UserController controller, UserContext userContext) = CreateUserController();
            var response = await controller.Get();
            var result = response.Should().BeOfType<JsonResult>().Subject;
            var appUser = result.Value.Should().BeAssignableTo<AppUser>().Subject;
            appUser.Id.Should().Be(1);
            appUser.Name.Should().Be("carlson");
        }

        [Fact]
        public async Task Patch_ReturnNewName_WithUpdateUserName()
        {
            (UserController controller, UserContext userContext) = CreateUserController();
            var pathDocument = new JsonPatchDocument<AppUser>();
            pathDocument.Replace(u => u.Name, "lei");
            var response = await controller.Patch(pathDocument);
            var result = response.Should().BeOfType<JsonResult>().Subject;
            var appUser = result.Value.Should().BeAssignableTo<AppUser>().Subject;
            appUser.Name.Should().Be("lei");
            var contextResult = await userContext.Users.SingleOrDefaultAsync(u => u.Id == 1);
            contextResult.Id.Should().Be(1);
            contextResult.Name.Should().Be("lei");
        }
    }
}