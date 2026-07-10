using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StadiumOps.Api.Responses;
using StadiumOps.Application.Abstractions;
using StadiumOps.Application.Features;
using StadiumOps.Application.Security;
using StadiumOps.Infrastructure.Identity;
using StadiumOps.Infrastructure.Persistence;
using StadiumOps.Infrastructure.Security;

namespace StadiumOps.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/auth").WithTags("Authentication");

        group.MapPost("/register", RegisterAsync).AllowAnonymous();
        group.MapPost("/login", LoginAsync).AllowAnonymous();
        group.MapPost("/refresh", RefreshAsync).AllowAnonymous();
        group.MapPost("/logout", LogoutAsync).RequireAuthorization();
        group.MapGet("/me", MeAsync).RequireAuthorization();

        return group;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        TokenService tokenService,
        IAuditWriter auditWriter,
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var validation = ValidateRegistration(request);
        if (validation is not null)
        {
            return ApiResults.ValidationProblem(context, validation);
        }

        var requestedRole = string.IsNullOrWhiteSpace(request.RequestedRole)
            ? StadiumRoles.RegisteredFan
            : request.RequestedRole.Trim();

        if (!StadiumRoles.SelfRegistrable.Contains(requestedRole))
        {
            return ApiResults.ValidationProblem(context, "The requested role is not available for self-registration.");
        }

        if (!await roleManager.RoleExistsAsync(requestedRole))
        {
            return ApiResults.ValidationProblem(context, $"Role '{requestedRole}' is not configured.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            Name = request.Name.Trim(),
            PreferredLanguage = string.IsNullOrWhiteSpace(request.PreferredLanguage) ? "en" : request.PreferredLanguage.Trim(),
            AccessibilityPreference = request.AccessibilityPreference?.Trim(),
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ApiResults.ValidationProblem(context, string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await userManager.AddToRoleAsync(user, requestedRole);
        auditWriter.Add(
            user.Id,
            "UserRegistered",
            "Auth",
            requestedRole,
            context.Connection.RemoteIpAddress?.ToString(),
            context.GetCorrelationId());
        await dbContext.SaveChangesAsync(cancellationToken);

        var auth = await tokenService.CreateTokenPairAsync(
            user,
            "registration",
            context.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return ApiResults.Created(context, $"/api/v1/users/{user.Id}", auth);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        TokenService tokenService,
        IAuditWriter auditWriter,
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiResults.ValidationProblem(context, "Email and password are required.");
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || user.IsDisabled)
        {
            return ApiResults.Unauthorized(context, "Invalid email or password.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return ApiResults.Unauthorized(context, "Invalid email or password.");
        }

        auditWriter.Add(
            user.Id,
            "UserLoggedIn",
            "Auth",
            request.DeviceName,
            context.Connection.RemoteIpAddress?.ToString(),
            context.GetCorrelationId());
        await dbContext.SaveChangesAsync(cancellationToken);

        var auth = await tokenService.CreateTokenPairAsync(
            user,
            request.DeviceName,
            context.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        return ApiResults.Ok(context, auth);
    }

    private static async Task<IResult> RefreshAsync(
        RefreshTokenRequest request,
        TokenService tokenService,
        IAuditWriter auditWriter,
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return ApiResults.ValidationProblem(context, "Refresh token is required.");
        }

        var auth = await tokenService.RefreshAsync(
            request.RefreshToken,
            context.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        if (auth is null)
        {
            return ApiResults.Unauthorized(context, "Refresh token is invalid or expired.");
        }

        auditWriter.Add(
            auth.User.Id,
            "RefreshTokenRotated",
            "Auth",
            null,
            context.Connection.RemoteIpAddress?.ToString(),
            context.GetCorrelationId());
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResults.Ok(context, auth);
    }

    private static async Task<IResult> LogoutAsync(
        LogoutRequest request,
        ClaimsPrincipal principal,
        TokenService tokenService,
        IAuditWriter auditWriter,
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return ApiResults.ValidationProblem(context, "Refresh token is required.");
        }

        var revoked = await tokenService.RevokeAsync(request.RefreshToken, cancellationToken);
        auditWriter.Add(
            principal.GetUserId(),
            "UserLoggedOut",
            "Auth",
            revoked ? "Refresh token revoked." : "Refresh token was already inactive or not found.",
            context.Connection.RemoteIpAddress?.ToString(),
            context.GetCorrelationId());
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResults.Ok(context, new { message = "Session revoked." });
    }

    private static async Task<IResult> MeAsync(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        HttpContext context)
    {
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(id, out var userId))
        {
            return ApiResults.Unauthorized(context);
        }

        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return ApiResults.NotFound(context, "User was not found.");
        }

        var roles = await userManager.GetRolesAsync(user);
        return ApiResults.Ok(
            context,
            new UserProfileResponse(
                user.Id,
                user.Name,
                user.Email ?? "",
                user.PreferredLanguage,
                user.AccessibilityPreference,
                roles.ToArray()));
    }

    private static string? ValidateRegistration(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return "Name is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return "A valid email address is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 10)
        {
            return "Password must be at least 10 characters long.";
        }

        return null;
    }
}
