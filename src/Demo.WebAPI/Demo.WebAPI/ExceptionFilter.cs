using System;
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
            context.Result = new InternalServerErrorObjectResult(new {
                errorMessage = GetAllInnerExceptions(context.Exception),
                stackTrace = context.Exception.StackTrace
            });

            _logger.LogError($"{context.Exception.Message} \n {context.Exception.StackTrace}");
            base.OnException(context);
        }

        private static string GetAllInnerExceptions(Exception exception, string exceptionMessages = "") {
            while (true) {
                var result = exceptionMessages == string.Empty
                    ? exception.Message + Environment.NewLine
                    : exceptionMessages + exception.Message + Environment.NewLine;

                if (exception.InnerException == null) return result;
                exception = exception.InnerException;
                exceptionMessages = result;
            }
        }

        private class InternalServerErrorObjectResult : ObjectResult
        {
            public InternalServerErrorObjectResult(object value) : base(value)
            {
                StatusCode = StatusCodes.Status500InternalServerError;
            }

            public InternalServerErrorObjectResult() : this(null)
            {
                StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}