using Application.Common.Pagination;
using Application.Common.Responses;
using Application.Common.Results;
using Application.Enrollments.Commands.CancelEnrollment;
using Application.Enrollments.Commands.EnrollStudent;
using Application.Enrollments.Queries.GetEnrollment;
using Application.Enrollments.Queries.ListStudentCourses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Diagnostics;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/students/{studentId:guid}")]
public sealed class EnrollmentsController(ISender sender) : ControllerBase
{
    private const string EnrollmentRoute = "enrollments/{courseId:guid}";

    [HttpPost("enrollments")]
    [EndpointSummary("Create a course enrollment for a student")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid studentId,
        EnrollStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new EnrollStudentCommand(studentId, request.CourseId), cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblemDetails(HttpContext);
    }

    [HttpGet(EnrollmentRoute)]
    [EndpointSummary("Get a student's enrollment in a course")]
    [ProducesResponseType<EnrollmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnrollmentResponse>> Get(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEnrollmentQuery(studentId, courseId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }

    [HttpDelete(EnrollmentRoute)]
    [EndpointSummary("Cancel a student's enrollment and release the seat")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CancelEnrollmentCommand(studentId, courseId), cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblemDetails(HttpContext);
    }

    [HttpGet("courses")]
    [EndpointSummary("List courses a student is enrolled in")]
    [ProducesResponseType<PagedResult<StudentCourseResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<StudentCourseResponse>>> ListEnrolledCourses(
        Guid studentId,
        CancellationToken cancellationToken,
        [FromQuery] int page = Pagination.DefaultPage,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        var result = await sender.Send(new ListStudentCoursesQuery(studentId, page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }
}

public sealed record EnrollStudentRequest(Guid CourseId);
