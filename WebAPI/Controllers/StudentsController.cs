using Application.Common.Pagination;
using Application.Common.Responses;
using Application.Common.Results;
using Application.Students.Commands.CreateStudent;
using Application.Students.Queries.GetStudent;
using Application.Students.Queries.ListStudents;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Diagnostics;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Create a student")]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create(
        CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, result.Value)
            : result.ToProblemDetails(HttpContext);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a student by ID")]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentResponse>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStudentQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }

    [HttpGet]
    [EndpointSummary("List students")]
    [ProducesResponseType<PagedResult<StudentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<StudentResponse>>> List(
        CancellationToken cancellationToken,
        [FromQuery] int page = Pagination.DefaultPage,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        var result = await sender.Send(new ListStudentsQuery(page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }
}
