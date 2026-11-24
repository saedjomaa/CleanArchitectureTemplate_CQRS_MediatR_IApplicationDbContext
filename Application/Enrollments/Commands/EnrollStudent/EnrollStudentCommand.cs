using Application.Common.Results;
using MediatR;

namespace Application.Enrollments.Commands.EnrollStudent;

public sealed record EnrollStudentCommand(Guid StudentId, Guid CourseId) : IRequest<Result>;
