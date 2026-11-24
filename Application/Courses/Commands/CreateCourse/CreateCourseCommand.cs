using Application.Common.Results;
using Domain.Entities;
using MediatR;

namespace Application.Courses.Commands.CreateCourse;

public sealed record CreateCourseCommand(string Title, int Capacity = Course.DefaultCapacity) : IRequest<Result<Guid>>;
