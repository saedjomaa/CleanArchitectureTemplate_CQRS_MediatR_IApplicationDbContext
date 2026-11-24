using Application.Common.Abstractions;
using Application.Common.Responses;
using Application.Common.Results;
using MediatR;

namespace Application.JsonPlaceholder.Commands.CreatePost;

internal sealed class CreatePostCommandHandler(IJsonPlaceholderClient jsonPlaceholder)
    : IRequestHandler<CreatePostCommand, Result<JsonPlaceholderPostResponse>>
{
    public Task<Result<JsonPlaceholderPostResponse>> Handle(
        CreatePostCommand command, CancellationToken cancellationToken) =>
        jsonPlaceholder.CreatePostAsync(command.UserId, command.Title, command.Body, cancellationToken);
}
