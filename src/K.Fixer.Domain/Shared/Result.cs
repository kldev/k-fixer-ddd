namespace K.Fixer.Domain.Shared;

public class Result
{
    public bool    IsSuccess { get; }
    public bool    IsFailure => !IsSuccess;
    public string? Error     { get; }

    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error     = error;
    }

    public static Result         Success()              => new(true, null);
    public static Result         Failure(string error)  => new(false, error);
    public static Result<T>      Success<T>(T value)    => new(value);
    public static Result<T>      Failure<T>(string error) => new(error);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }
    
    internal Result(T value) : base(true, null)
        => Value = value;
    
    internal Result(string error) : base(false, error)
        => Value = default;
}