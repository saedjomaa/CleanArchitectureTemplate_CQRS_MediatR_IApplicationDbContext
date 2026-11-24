using Application.Common.Responses;
using Application.Common.Results;
using MediatR;

namespace Application.Enrollments.Queries.GetEnrollment;

public sealed record GetEnrollmentQuery(Guid StudentId, Guid CourseId) : IRequest<Result<EnrollmentResponse>>;
