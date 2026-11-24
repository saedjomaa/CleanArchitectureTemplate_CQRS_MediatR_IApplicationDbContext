using Application.Common.Pagination;
using Application.Common.Responses;
using Application.Common.Results;
using Application.JsonPlaceholder.Commands.CreatePost;
using Application.JsonPlaceholder.Queries.GetUser;
using Application.JsonPlaceholder.Queries.ListUsers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Diagnostics;

namespace WebAPI.Controllers;

/// <summary>
/// Endpoints backed by the external JSONPlaceholder API (https://jsonplaceholder.typicode.com).
/// One controller per external service; the database is not used here.
/// </summary>
[ApiController]
[Route("api/jsonplaceholder")]
public sealed class JsonPlaceholderController(ISender sender) : ControllerBase
{
    [HttpGet("users")]
    [EndpointSummary("List users from JSONPlaceholder")]
    [ProducesResponseType<PagedResult<JsonPlaceholderUserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<PagedResult<JsonPlaceholderUserResponse>>> ListUsers(
        CancellationToken cancellationToken,
        [FromQuery] int page = Pagination.DefaultPage,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        var result = await sender.Send(new ListUsersQuery(page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }

    [HttpGet("users/{id:int}")]
    [EndpointSummary("Get a user from JSONPlaceholder")]
    [ProducesResponseType<JsonPlaceholderUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<JsonPlaceholderUserResponse>> GetUser(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }

    [HttpPost("posts")]
    [EndpointSummary("Create a post in JSONPlaceholder (fake: JSONPlaceholder does not persist it)")]
    [ProducesResponseType<JsonPlaceholderPostResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<JsonPlaceholderPostResponse>> CreatePost(
        CreatePostCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        // There is no endpoint to read a single post, so the 201 has no Location header.
        return result.IsSuccess
            ? Created((string?)null, result.Value)
            : result.ToProblemDetails(HttpContext);
    }
}
