using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using User.API.Exceptions;

namespace User.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly IHostingEnvironment _env;
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(IHostingEnvironment env, ILogger<GlobalExceptionFilter> logger)
        {
            _env = env;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var result = new JsonErrorResponse();
            if (context.Exception.GetType() == typeof(UserOperationException))
            {
                result.Message = context.Exception.Message;
                context.Result = new BadRequestObjectResult(result);
            }
            else
            {
                result.Message = "发生了未知的内部错误";
                if (_env.IsDevelopment())
                {
                    result.DevelopMessage = context.Exception.ToString();
                }
                context.Result = new InternalServerErrorObject(result);
            }
            _logger.LogError(context.Exception,context.Exception.Message);
            context.ExceptionHandled = true;
        }
    }

    public class InternalServerErrorObject : ObjectResult
    {
        public InternalServerErrorObject(JsonErrorResponse error) : base(error)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}