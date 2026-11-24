using Application.Common.Responses;
using Application.Common.Results;
using MediatR;

namespace Application.JsonPlaceholder.Commands.CreatePost;

public sealed record CreatePostCommand(int UserId, string Title, string Body) : IRequest<Result<JsonPlaceholderPostResponse>>
{
    public const int MaximumTitleLength = 200;
    public const int MaximumBodyLength = 2000;
}
