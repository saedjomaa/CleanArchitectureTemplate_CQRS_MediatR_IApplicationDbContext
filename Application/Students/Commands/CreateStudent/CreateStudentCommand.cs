using Application.Common.Results;
using MediatR;

namespace Application.Students.Commands.CreateStudent;

public sealed record CreateStudentCommand(string FullName) : IRequest<Result<Guid>>;
