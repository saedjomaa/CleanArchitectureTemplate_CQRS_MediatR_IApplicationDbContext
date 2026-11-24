using Application.Common.Abstractions;
using Application.Common.Pagination;
using Application.Common.Responses;
using Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Application.Courses.Queries.ListCourses;

internal sealed class ListCoursesQueryHandler(IApplicationDbContext db)
    : IQueryHandler<ListCoursesQuery, Result<PagedResult<CourseResponse>>>
{
    public async Task<Result<PagedResult<CourseResponse>>> HandleAsync(
        ListCoursesQuery query, CancellationToken cancellationToken)
    {
        var page = await db.Courses.AsNoTracking()
            .OrderBy(course => course.Title)
            .ThenBy(course => course.Id)
            .Select(CourseMappings.ToResponse)
            .ToPageAsync(query.Page, query.PageSize, cancellationToken);

        return Result<PagedResult<CourseResponse>>.Success(page);
    }
}
