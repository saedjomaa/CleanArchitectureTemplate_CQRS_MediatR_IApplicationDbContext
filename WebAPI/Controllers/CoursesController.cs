using Application.Common.Pagination;
using Application.Common.Responses;
using Application.Common.Results;
using Application.Courses.Commands.CreateCourse;
using Application.Courses.Queries.GetCourse;
using Application.Courses.Queries.ListCourses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Diagnostics;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController(ISender sender) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Create a course")]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create(
        CreateCourseCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, result.Value)
            : result.ToProblemDetails(HttpContext);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a course by ID")]
    [ProducesResponseType<CourseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseResponse>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCourseQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }

    [HttpGet]
    [EndpointSummary("List courses")]
    [ProducesResponseType<PagedResult<CourseResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CourseResponse>>> List(
        CancellationToken cancellationToken,
        [FromQuery] int page = Pagination.DefaultPage,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        var result = await sender.Send(new ListCoursesQuery(page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }
}
