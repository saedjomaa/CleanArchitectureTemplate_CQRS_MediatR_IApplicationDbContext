using Application.Common.Abstractions;
using Application.Common.Results;
using Domain.Entities;

namespace Application.Courses.Commands.CreateCourse;

internal sealed class CreateCourseCommandHandler(IApplicationDbContext db)
    : ICommandHandler<CreateCourseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        var course = Course.Create(command.Title, command.Capacity);
        db.AddCourse(course);
        await db.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(course.Id);
    }
}
