namespace TaskManagement.Api.Common;

public class ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse Ok(string? message = null) => new() { Success = true, Message = message };
}

public sealed class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };
}
