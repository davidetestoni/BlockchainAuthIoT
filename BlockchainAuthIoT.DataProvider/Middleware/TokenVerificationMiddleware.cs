using BlockchainAuthIoT.DataProvider.Exceptions;
using BlockchainAuthIoT.DataProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Middleware
{
    public class TokenVerificationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenVerificationMiddleware> _logger;
        private readonly IHostEnvironment _env;
        private readonly ITokenVerificationService _tokenVerification;

        public TokenVerificationMiddleware(RequestDelegate next, ILogger<TokenVerificationMiddleware> logger,
            IHostEnvironment env, ITokenVerificationService tokenVerification)
        {
            _next = next;
            _logger = logger;
            _env = env;
            _tokenVerification = tokenVerification;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress;

            // Check if the request has the required header
            if (!context.Request.Headers.ContainsKey("Token"))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Missing Token header");
                _logger.LogError($"[{ip}] Missing Token header");
                return;
            }

            // Verify the token and store it for the next middleware
            try
            {
                var contractAddress = await _tokenVerification.VerifyToken(context.Request.Headers["Token"]);
                context.Items.Add("contractAddress", contractAddress);
                await _next(context);
            }
            catch (ContractNotFoundException ex)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync($"Contract not found at {ex.Address}");
                _logger.LogError($"[{ip}] Sent an invalid contract address ({ex.Address})");            
            }
            catch (TokenVerificationException ex)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync($"Token verification failed: {ex.Message}");
                _logger.LogError($"[{ip}] Token verification failed");
            }
            catch (InvalidContractException ex)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync($"Invalid contract: {ex.Message}");
                _logger.LogError($"[{ip}] Provided an invalid contract: {ex.Message}");
            }
        }
    }
}
