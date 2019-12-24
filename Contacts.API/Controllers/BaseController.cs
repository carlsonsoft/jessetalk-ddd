using Contacts.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Contacts.API.Controllers
{
    public class BaseController : Controller
    {
        protected  UserIdentity UserIdentity => new UserIdentity() { UserId = 1 };
    }
}