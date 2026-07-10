namespace StadiumOps.Application.Abstractions;

public sealed record ApiEnvelope<T>(bool Success, T? Data, ApiError? Error, string CorrelationId)
{
    public static ApiEnvelope<T> Ok(T data, string correlationId) => new(true, data, null, correlationId);
    public static ApiEnvelope<T> Fail(string code, string message, string correlationId) =>
        new(false, default, new ApiError(code, message), correlationId);
}

public sealed record ApiError(string Code, string Message);

public sealed record PagedEnvelope<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
