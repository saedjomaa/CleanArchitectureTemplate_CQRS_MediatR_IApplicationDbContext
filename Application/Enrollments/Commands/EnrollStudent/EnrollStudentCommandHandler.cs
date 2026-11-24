using Application.Common.Abstractions;
using Application.Common.Results;
using Application.Shared.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Enrollments.Commands.EnrollStudent;

internal sealed class EnrollStudentCommandHandler(IApplicationDbContext db, TimeProvider clock)
    : IRequestHandler<EnrollStudentCommand, Result>
{
    public async Task<Result> Handle(EnrollStudentCommand command, CancellationToken cancellationToken)
    {
        if (!await db.Students.AnyAsync(student => student.Id == command.StudentId, cancellationToken))
        {
            return Result.Failure(StudentErrors.NotFound);
        }

        var course = await db.Courses
            .SingleOrDefaultAsync(course => course.Id == command.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        if (await db.Enrollments.AnyAsync(enrollment =>
                enrollment.StudentId == command.StudentId && enrollment.CourseId == command.CourseId, cancellationToken))
        {
            return Result.Failure(EnrollmentErrors.AlreadyEnrolled);
        }

        var enrollment = course.TryEnroll(command.StudentId, clock.GetUtcNow());
        if (enrollment is null)
        {
            return Result.Failure(EnrollmentErrors.CourseFull);
        }

        try
        {
            return await db.TrySaveEnrollmentAsync(enrollment, cancellationToken)
                ? Result.Success()
                : Result.Failure(EnrollmentErrors.AlreadyEnrolled);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(EnrollmentErrors.ConcurrencyConflict);
        }
    }
}
