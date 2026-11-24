using Application.Common.Pagination;
using Application.Common.Responses;
using Application.Common.Results;
using MediatR;

namespace Application.Courses.Queries.ListCourses;

public sealed record ListCoursesQuery(int Page = Pagination.DefaultPage, int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PagedResult<CourseResponse>>>;
