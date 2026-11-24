using Application.Common.Abstractions;
using Application.Common.Responses;
using Application.Common.Results;
using Application.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace Application.Enrollments.Queries.GetEnrollment;

internal sealed class GetEnrollmentQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetEnrollmentQuery, Result<EnrollmentResponse>>
{
    public async Task<Result<EnrollmentResponse>> HandleAsync(
        GetEnrollmentQuery query, CancellationToken cancellationToken)
    {
        var enrollment = await db.Enrollments.AsNoTracking()
            .Where(enrollment => enrollment.StudentId == query.StudentId && enrollment.CourseId == query.CourseId)
            .Select(enrollment => new EnrollmentResponse(enrollment.StudentId, enrollment.CourseId, enrollment.EnrolledAt))
            .SingleOrDefaultAsync(cancellationToken);

        return enrollment is null
            ? Result<EnrollmentResponse>.Failure(EnrollmentErrors.NotFound)
            : Result<EnrollmentResponse>.Success(enrollment);
    }
}
