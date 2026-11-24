using Application.Common.Abstractions;
using Application.Common.Responses;
using Application.Common.Results;
using Application.Shared.Errors;
using MediatR;

namespace Application.JsonPlaceholder.Queries.GetUser;

internal sealed class GetUserQueryHandler(IJsonPlaceholderClient jsonPlaceholder)
    : IRequestHandler<GetUserQuery, Result<JsonPlaceholderUserResponse>>
{
    public async Task<Result<JsonPlaceholderUserResponse>> Handle(
        GetUserQuery query, CancellationToken cancellationToken)
    {
        var result = await jsonPlaceholder.GetUserAsync(query.Id, cancellationToken);

        return result.Error == ExternalServiceErrors.NotFound
            ? Result<JsonPlaceholderUserResponse>.Failure(JsonPlaceholderErrors.UserNotFound)
            : result;
    }
}
