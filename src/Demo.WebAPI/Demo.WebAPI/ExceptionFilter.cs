using Demo.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Demo.WebAPI {
    public class ExceptionFilter : ExceptionFilterAttribute {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger) {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context) {
            context.Result = new InternalServerErrorObjectResult(new Response<object>(null) {
                Error = {
                    ErrorMessage = context.Exception.Message,
                    StackTrace = context.Exception.StackTrace
                }
            });

            _logger.LogError($"{context.Exception.Message} \n {context.Exception.StackTrace}");

            base.OnException(context);
        }

        private class InternalServerErrorObjectResult : ObjectResult {
            public InternalServerErrorObjectResult(object value) : base(value) {
                StatusCode = StatusCodes.Status500InternalServerError;
            }

            public InternalServerErrorObjectResult() : this(null) {
                StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}