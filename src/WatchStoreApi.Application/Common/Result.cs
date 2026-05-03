namespace WatchStoreApi.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public int StatusCode { get; }

    protected Result(bool isSuccess, string? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result Success() => new(true, null, 200);
    public static Result Failure(string error, int statusCode = 400) => new(false, error, statusCode);
    public static Result NotFound(string error) => new(false, error, 404);
    public static Result Unauthorized(string error) => new(false, error, 401);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(true, null, 200) => Value = value;
    private Result(T value, int statusCode) : base(true, null, statusCode) => Value = value;
    private Result(string error, int statusCode) : base(false, error, statusCode) { }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Created(T value) => new(value, 201);
    public new static Result<T> Failure(string error, int statusCode = 400) => new(error, statusCode);
    public new static Result<T> NotFound(string error) => new(error, 404);
    public new static Result<T> Unauthorized(string error) => new(error, 401);
}
