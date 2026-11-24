using Application.Common.Abstractions;
using Application.Common.Pagination;
using Application.Common.Responses;
using Application.Common.Results;
using Application.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace Application.Enrollments.Queries.ListStudentCourses;

internal sealed class ListStudentCoursesQueryHandler(IApplicationDbContext db)
    : IQueryHandler<ListStudentCoursesQuery, Result<PagedResult<StudentCourseResponse>>>
{
    public async Task<Result<PagedResult<StudentCourseResponse>>> HandleAsync(
        ListStudentCoursesQuery query, CancellationToken cancellationToken)
    {
        if (!await db.Students.AsNoTracking().AnyAsync(student => student.Id == query.StudentId, cancellationToken))
        {
            return Result<PagedResult<StudentCourseResponse>>.Failure(StudentErrors.NotFound);
        }

        var courses =
            from enrollment in db.Enrollments.AsNoTracking()
            join course in db.Courses.AsNoTracking() on enrollment.CourseId equals course.Id
            where enrollment.StudentId == query.StudentId
            orderby course.Title, course.Id
            select new StudentCourseResponse(course.Id, course.Title, enrollment.EnrolledAt);

        var page = await courses.ToPageAsync(query.Page, query.PageSize, cancellationToken);
        return Result<PagedResult<StudentCourseResponse>>.Success(page);
    }
}
