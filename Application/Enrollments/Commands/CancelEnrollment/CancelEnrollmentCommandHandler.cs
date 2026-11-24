using Application.Common.Abstractions;
using Application.Common.Results;
using Application.Shared.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Enrollments.Commands.CancelEnrollment;

internal sealed class CancelEnrollmentCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CancelEnrollmentCommand, Result>
{
    public async Task<Result> Handle(CancelEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var registration = await (
            from enrollment in db.Enrollments
            join course in db.Courses on enrollment.CourseId equals course.Id
            where enrollment.StudentId == command.StudentId && enrollment.CourseId == command.CourseId
            select new { Enrollment = enrollment, Course = course }
        ).SingleOrDefaultAsync(cancellationToken);
        if (registration is null)
        {
            return Result.Failure(EnrollmentErrors.NotFound);
        }

        registration.Course.CancelEnrollment(registration.Enrollment);
        db.RemoveEnrollment(registration.Enrollment);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(EnrollmentErrors.ConcurrencyConflict);
        }
    }
}
