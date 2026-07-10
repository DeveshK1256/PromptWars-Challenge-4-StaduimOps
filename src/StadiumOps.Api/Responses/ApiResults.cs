using StadiumOps.Application.Abstractions;

namespace StadiumOps.Api.Responses;

public static class ApiResults
{
    public static IResult Ok<T>(HttpContext context, T data) =>
        Results.Ok(ApiEnvelope<T>.Ok(data, context.GetCorrelationId()));

    public static IResult Created<T>(HttpContext context, string uri, T data) =>
        Results.Created(uri, ApiEnvelope<T>.Ok(data, context.GetCorrelationId()));

    public static IResult ValidationProblem(HttpContext context, string message) =>
        Results.Problem(
            title: "Validation failed",
            detail: message,
            statusCode: StatusCodes.Status400BadRequest,
            extensions: Extensions(context));

    public static IResult Unauthorized(HttpContext context, string message = "Authentication is required.") =>
        Results.Problem(
            title: "Unauthorized",
            detail: message,
            statusCode: StatusCodes.Status401Unauthorized,
            extensions: Extensions(context));

    public static IResult NotFound(HttpContext context, string message) =>
        Results.Problem(
            title: "Not found",
            detail: message,
            statusCode: StatusCodes.Status404NotFound,
            extensions: Extensions(context));

    public static IResult Conflict(HttpContext context, string message) =>
        Results.Problem(
            title: "Conflict",
            detail: message,
            statusCode: StatusCodes.Status409Conflict,
            extensions: Extensions(context));

    public static IResult ServiceUnavailable(HttpContext context, string message) =>
        Results.Problem(
            title: "Integration not configured",
            detail: message,
            statusCode: StatusCodes.Status503ServiceUnavailable,
            extensions: Extensions(context));

    private static Dictionary<string, object?> Extensions(HttpContext context) =>
        new() { ["correlationId"] = context.GetCorrelationId() };
}

public static class HttpContextCorrelationExtensions
{
    public const string CorrelationItemKey = "CorrelationId";

    public static string GetCorrelationId(this HttpContext context) =>
        context.Items.TryGetValue(CorrelationItemKey, out var value) && value is string correlationId
            ? correlationId
            : context.TraceIdentifier;
}
