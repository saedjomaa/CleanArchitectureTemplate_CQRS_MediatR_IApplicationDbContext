using Application.Common.Pagination;
using Application.Common.Responses;
using Application.Common.Results;
using MediatR;

namespace Application.JsonPlaceholder.Queries.ListUsers;

public sealed record ListUsersQuery(int Page = Pagination.DefaultPage, int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PagedResult<JsonPlaceholderUserResponse>>>;
