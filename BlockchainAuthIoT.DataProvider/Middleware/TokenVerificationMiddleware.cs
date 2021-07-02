using BlockchainAuthIoT.DataProvider.Exceptions;
using BlockchainAuthIoT.DataProvider.Exceptions.Api;
using BlockchainAuthIoT.DataProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Middleware
{
    public class TokenVerificationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenVerificationMiddleware> _logger;
        private readonly ITokenVerificationService _tokenVerification;

        public TokenVerificationMiddleware(RequestDelegate next, ILogger<TokenVerificationMiddleware> logger,
            ITokenVerificationService tokenVerification)
        {
            _next = next;
            _logger = logger;
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
                throw new NotFoundException($"Contract not found at {ex.Address}", ex);
            }
            catch (TokenVerificationException ex)
            {
                throw new UnauthorizedException($"Token verification failed: {ex.Message}", ex);
            }
            catch (InvalidContractException ex)
            {
                throw new ForbiddenException($"Invalid contract: {ex.Message}", ex);
            }
        }
    }
}
