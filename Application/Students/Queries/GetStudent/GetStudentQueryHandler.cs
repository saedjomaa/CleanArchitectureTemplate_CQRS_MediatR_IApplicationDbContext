using Application.Common.Abstractions;
using Application.Common.Responses;
using Application.Common.Results;
using Application.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace Application.Students.Queries.GetStudent;

internal sealed class GetStudentQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetStudentQuery, Result<StudentResponse>>
{
    public async Task<Result<StudentResponse>> HandleAsync(GetStudentQuery query, CancellationToken cancellationToken)
    {
        var student = await db.Students.AsNoTracking()
            .Where(student => student.Id == query.Id)
            .Select(StudentMappings.ToResponse)
            .SingleOrDefaultAsync(cancellationToken);

        return student is null
            ? Result<StudentResponse>.Failure(StudentErrors.NotFound)
            : Result<StudentResponse>.Success(student);
    }
}
