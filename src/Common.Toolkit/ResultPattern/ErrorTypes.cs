namespace Common.Toolkit.ResultPattern;

public abstract record Error(string Code, string Message, string? Details = null, Exception? Exception = null);

public record ValidationError(string Code, string Message, string? Details = null, Exception? Exception = null)
  : Error(Code, Message, Details, Exception);

public record GenericError(string Code, string Message, string? Details = null, Exception? Exception = null)
  : Error(Code, Message, Details, Exception);

public record NotFoundError(string Code, string Message, string? Details = null, Exception? Exception = null)
  : Error(Code, Message, Details, Exception);

public record BusinessLogicError(string Code, string Message, string? Details = null, Exception? Exception = null)
  : Error(Code, Message, Details, Exception);

public record ForbiddenError(string Code, string Message, string? Details = null, Exception? Exception = null)
  : Error(Code, Message, Details, Exception);