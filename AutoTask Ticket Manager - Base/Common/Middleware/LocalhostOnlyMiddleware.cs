using Microsoft.AspNetCore.Http;
using System.Net;

namespace AutoTaskTicketManager_Base.Common.Middleware
{
    public class LocalhostOnlyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LocalhostOnlyMiddleware> _logger;

        public LocalhostOnlyMiddleware(RequestDelegate next, ILogger<LocalhostOnlyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            if (remoteIp == null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Unable to determine remote IP.");
                return;
            }

            if (!IsLocalhost(remoteIp))
            {
                _logger.LogWarning("Forbidden request from non-local address {RemoteIp}", remoteIp);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden: Local requests only.");
                return;
            }

            await _next(context);
        }

        private bool IsLocalhost(IPAddress remoteIp)
        {
            return IPAddress.IsLoopback(remoteIp) ||
                   remoteIp.Equals(IPAddress.Parse("::1"));
        }
    }
}
