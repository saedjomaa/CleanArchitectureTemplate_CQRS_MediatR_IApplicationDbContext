using Application.Common.Results;
using MediatR;

namespace Application.Enrollments.Commands.CancelEnrollment;

public sealed record CancelEnrollmentCommand(Guid StudentId, Guid CourseId) : IRequest<Result>;
