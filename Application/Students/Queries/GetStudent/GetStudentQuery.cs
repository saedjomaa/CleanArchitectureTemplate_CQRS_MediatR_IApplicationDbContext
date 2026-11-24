using Application.Common.Responses;
using Application.Common.Results;
using MediatR;

namespace Application.Students.Queries.GetStudent;

public sealed record GetStudentQuery(Guid Id) : IRequest<Result<StudentResponse>>;
