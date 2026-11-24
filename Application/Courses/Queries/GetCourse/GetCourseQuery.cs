using Application.Common.Responses;
using Application.Common.Results;
using MediatR;

namespace Application.Courses.Queries.GetCourse;

public sealed record GetCourseQuery(Guid Id) : IRequest<Result<CourseResponse>>;
