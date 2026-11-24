using System.Linq.Expressions;
using Application.Common.Responses;
using Domain.Entities;

namespace Application.Students;

internal static class StudentMappings
{
    public static readonly Expression<Func<Student, StudentResponse>> ToResponse = student =>
        new StudentResponse(student.Id, student.FullName);
}
