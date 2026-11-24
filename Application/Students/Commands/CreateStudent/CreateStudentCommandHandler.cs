using Application.Common.Abstractions;
using Application.Common.Results;
using Domain.Entities;

namespace Application.Students.Commands.CreateStudent;

internal sealed class CreateStudentCommandHandler(IApplicationDbContext db)
    : ICommandHandler<CreateStudentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateStudentCommand command, CancellationToken cancellationToken)
    {
        var student = Student.Create(command.FullName);
        db.AddStudent(student);
        await db.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(student.Id);
    }
}
