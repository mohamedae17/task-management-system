namespace TaskManagement.Application.Common.Models;

public class Result
{
    public bool Succeeded { get; }
    public IReadOnlyList<string> Errors { get; }
    public string? ErrorCode { get; }

    protected Result(bool succeeded, IEnumerable<string>? errors, string? errorCode)
    {
        Succeeded = succeeded;
        Errors = (errors ?? Array.Empty<string>()).ToList().AsReadOnly();
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);

    public static Result Failure(params string[] errors) => new(false, errors, null);

    public static Result Failure(string errorCode, params string[] errors) =>
        new(false, errors, errorCode);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(T? value, bool succeeded, IEnumerable<string>? errors, string? errorCode)
        : base(succeeded, errors, errorCode)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, null, null);

    public new static Result<T> Failure(params string[] errors) =>
        new(default, false, errors, null);

    public new static Result<T> Failure(string errorCode, params string[] errors) =>
        new(default, false, errors, errorCode);
}
