using BlockchainAuthIoT.DataProvider.Exceptions.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                (int errorCode, string errorType) = ex switch
                {
                    BadRequestException => (400, "Bad Request"),
                    UnauthorizedException => (401, "Unauthorized"),
                    ForbiddenException => (403, "Forbidden"),
                    NotFoundException => (404, "Not Found"),
                    _ => (500, "Internal Server Errror")
                };

                // Log the exception as a warning
                var logMessage = _env.IsDevelopment() && ex.InnerException is not null 
                    ? ex.Message + Environment.NewLine + ex.InnerException.ToString() 
                    : ex.Message;

                _logger.LogWarning($"[{errorType}][{ip}] {logMessage}");

                // Send the bad response
                context.Response.StatusCode = errorCode;
                await context.Response.WriteAsync(ex.Message);
            }
        }
    }
}
