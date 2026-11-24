using System.Diagnostics;
using Application.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

/// <summary>
/// Logs every request with its elapsed time and outcome. Registered first, so it wraps validation too.
/// </summary>
internal sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var result = await next();

        logger.LogInformation(
            "Handled {RequestType} in {ElapsedMs} ms with success {IsSuccess} and error {ErrorCode}",
            typeof(TRequest).Name, Stopwatch.GetElapsedTime(started).TotalMilliseconds,
            result.IsSuccess, result.Error?.Code);

        return result;
    }
}
