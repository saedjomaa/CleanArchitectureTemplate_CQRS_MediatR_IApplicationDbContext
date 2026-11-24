using Application.Common.Abstractions;
using Application.Common.Responses;
using Application.Common.Results;
using Application.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace Application.Courses.Queries.GetCourse;

internal sealed class GetCourseQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetCourseQuery, Result<CourseResponse>>
{
    public async Task<Result<CourseResponse>> HandleAsync(GetCourseQuery query, CancellationToken cancellationToken)
    {
        var course = await db.Courses.AsNoTracking()
            .Where(course => course.Id == query.Id)
            .Select(CourseMappings.ToResponse)
            .SingleOrDefaultAsync(cancellationToken);

        return course is null
            ? Result<CourseResponse>.Failure(CourseErrors.NotFound)
            : Result<CourseResponse>.Success(course);
    }
}
