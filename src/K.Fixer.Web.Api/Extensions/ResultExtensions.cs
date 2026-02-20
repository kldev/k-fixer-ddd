using Common.Toolkit.ResultPattern;

namespace K.Fixer.Web.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToApiResult<T>(this Result<T> result) => result switch
    {
        Success<T> s => TypedResults.Ok(s.Value),
        ValidationFailure<T> f => TypedResults.BadRequest(f.Error),
        NotFoundFailure<T> f => TypedResults.NotFound(f.Error),
        ForbiddenFailure<T> => TypedResults.Forbid(),
        BusinessLogicFailure<T> f => TypedResults.UnprocessableEntity(f.Error),
        GenericFailure<T> f => TypedResults.Problem(detail: f.Error.Message, statusCode: 500),
        _ => TypedResults.Problem()
    };
}