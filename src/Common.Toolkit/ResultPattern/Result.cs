namespace Common.Toolkit.ResultPattern;


public abstract class Result<T>
{
    public bool IsSuccess => this is Success<T>;
    public bool IsFailure => !IsSuccess;

    // --- Factory methods ---

    public static Success<T> Success(T value) => new(value);

    public static ValidationFailure<T> Failure(ValidationError error) => new(error);
    public static GenericFailure<T> Failure(GenericError error) => new(error);
    public static BusinessLogicFailure<T> Failure(BusinessLogicError error) => new(error);
    public static NotFoundFailure<T> Failure(NotFoundError error) => new(error);
    public static ForbiddenFailure<T> Failure(ForbiddenError error) => new(error);

    // --- Implicit operators ---

    public static implicit operator Result<T>(T value)
        => value is null
            ? throw new ArgumentNullException(nameof(value), "Use explicit error instead of returning null.")
            : new Success<T>(value);

    public static implicit operator Result<T>(ValidationError error) => new ValidationFailure<T>(error);
    public static implicit operator Result<T>(GenericError error) => new GenericFailure<T>(error);
    public static implicit operator Result<T>(BusinessLogicError error) => new BusinessLogicFailure<T>(error);
    public static implicit operator Result<T>(NotFoundError error) => new NotFoundFailure<T>(error);
    public static implicit operator Result<T>(ForbiddenError error) => new ForbiddenFailure<T>(error);

    // --- Pattern matching ---

    /// <summary>
    /// Pattern match on Result — executes the appropriate function based on success/failure.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        return this switch
        {
            Success<T> s => onSuccess(s.Value),
            ValidationFailure<T> f => onFailure(f.Error),
            NotFoundFailure<T> f => onFailure(f.Error),
            ForbiddenFailure<T> f => onFailure(f.Error),
            BusinessLogicFailure<T> f => onFailure(f.Error),
            GenericFailure<T> f => onFailure(f.Error),
            Failure<T> f => onFailure(f.Error),
            _ => throw new InvalidOperationException("Unknown Result type")
        };
    }

    /// <summary>
    /// Maps the success value to a new type, preserving errors.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return this switch
        {
            Success<T> s => Result<TNew>.Success(mapper(s.Value)),
            ValidationFailure<T> f => Result<TNew>.Failure(f.Error),
            NotFoundFailure<T> f => Result<TNew>.Failure(f.Error),
            ForbiddenFailure<T> f => Result<TNew>.Failure(f.Error),
            BusinessLogicFailure<T> f => Result<TNew>.Failure(f.Error),
            GenericFailure<T> f => Result<TNew>.Failure(f.Error),
            Failure<T> f => Result<TNew>.Failure(new GenericError(f.Error.Code, f.Error.Message)),
            _ => throw new InvalidOperationException("Unknown Result type")
        };
    }

    /// <summary>
    /// Executes an action on the success value (side effect), returns original Result.
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (this is Success<T> s)
            action(s.Value);
        return this;
    }

    /// <summary>
    /// Executes an action on the error (side effect), returns original Result.
    /// </summary>
    public Result<T> OnFailure(Action<Error> action)
    {
        var error = this switch
        {
            ValidationFailure<T> f => f.Error,
            NotFoundFailure<T> f => f.Error,
            ForbiddenFailure<T> f => f.Error,
            BusinessLogicFailure<T> f => f.Error,
            GenericFailure<T> f => f.Error,
            Failure<T> f => f.Error,
            _ => null
        };
        if (error != null)
            action(error);
        return this;
    }
}