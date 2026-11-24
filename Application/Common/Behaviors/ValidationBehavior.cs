using Application.Common.Results;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Common.Behaviors;

/// <summary>
/// Runs every registered FluentValidation validator for the request before its handler.
/// On failure the handler is not called and a <c>Validation.Failed</c> error is returned as a failed result.
/// </summary>
internal sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
        {
            return await next();
        }

        var errors = failures.GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

        return TResponse.Failure(new Error(
            "Validation.Failed", "One or more fields are invalid.", ErrorType.Validation, errors));
    }
}
