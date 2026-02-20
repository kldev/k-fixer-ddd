namespace Common.Toolkit.ResultPattern;

// --- Success ---

public class Success<T> : Result<T>
{
    public T Value { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public Success(T value) => Value = value;
}

// --- Failure subtypes ---

public class Failure<T> : Result<T>
{
    public Error Error { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public Failure(Error error) => Error = error;
}

public class ValidationFailure<T> : Result<T>
{
    public ValidationError Error { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public ValidationFailure(ValidationError error) => Error = error;
}

public class GenericFailure<T> : Result<T>
{
    public GenericError Error { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public GenericFailure(GenericError error) => Error = error;
}

public class NotFoundFailure<T> : Result<T>
{
    public NotFoundError Error { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public NotFoundFailure(NotFoundError error) => Error = error;
}

public class BusinessLogicFailure<T> : Result<T>
{
    public BusinessLogicError Error { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public BusinessLogicFailure(BusinessLogicError error) => Error = error;
}

public class ForbiddenFailure<T> : Result<T>
{
    public ForbiddenError Error { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public ForbiddenFailure(ForbiddenError error) => Error = error;
}