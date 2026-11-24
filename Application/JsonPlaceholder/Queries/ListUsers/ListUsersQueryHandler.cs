using Application.Common.Abstractions;
using Application.Common.Responses;
using Application.Common.Results;
using MediatR;

namespace Application.JsonPlaceholder.Queries.ListUsers;

internal sealed class ListUsersQueryHandler(IJsonPlaceholderClient jsonPlaceholder)
    : IRequestHandler<ListUsersQuery, Result<PagedResult<JsonPlaceholderUserResponse>>>
{
    public Task<Result<PagedResult<JsonPlaceholderUserResponse>>> Handle(
        ListUsersQuery query, CancellationToken cancellationToken) =>
        jsonPlaceholder.ListUsersAsync(query.Page, query.PageSize, cancellationToken);
}
