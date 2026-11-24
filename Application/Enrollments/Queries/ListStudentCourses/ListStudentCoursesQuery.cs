using Application.Common.Pagination;
using Application.Common.Responses;
using Application.Common.Results;
using MediatR;

namespace Application.Enrollments.Queries.ListStudentCourses;

public sealed record ListStudentCoursesQuery(
    Guid StudentId, int Page = Pagination.DefaultPage, int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PagedResult<StudentCourseResponse>>>;
