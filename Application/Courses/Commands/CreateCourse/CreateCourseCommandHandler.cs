using Application.Common.Abstractions;
using Application.Common.Results;
using Domain.Entities;
using MediatR;

namespace Application.Courses.Commands.CreateCourse;

internal sealed class CreateCourseCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateCourseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        var course = Course.Create(command.Title, command.Capacity);
        db.AddCourse(course);
        await db.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(course.Id);
    }
}
