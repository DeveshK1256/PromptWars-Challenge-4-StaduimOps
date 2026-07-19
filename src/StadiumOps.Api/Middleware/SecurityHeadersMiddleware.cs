using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace StadiumOps.Api.Middleware;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Enforce secure transport and framing protections
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), camera=(), microphone=()");
        var isSwagger = context.Request.Path.StartsWithSegments("/swagger") 
            || context.Request.Path.StartsWithSegments("/swagger-ui")
            || context.Request.Path.Value?.Contains("/swagger/") == true;

        if (isSwagger)
        {
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline'; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "frame-ancestors 'none'; " +
                "object-src 'none';");
        }
        else
        {
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self' https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "frame-ancestors 'none'; " +
                "object-src 'none';");
        }

        await next(context);
    }
}
