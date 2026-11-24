using Application.Common.Responses;
using Application.Common.Results;
using MediatR;

namespace Application.JsonPlaceholder.Queries.GetUser;

public sealed record GetUserQuery(int Id) : IRequest<Result<JsonPlaceholderUserResponse>>;
