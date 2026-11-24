using Application.Common.Abstractions;
using Application.Common.Pagination;
using Application.Common.Responses;
using Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Application.Students.Queries.ListStudents;

internal sealed class ListStudentsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<ListStudentsQuery, Result<PagedResult<StudentResponse>>>
{
    public async Task<Result<PagedResult<StudentResponse>>> HandleAsync(
        ListStudentsQuery query, CancellationToken cancellationToken)
    {
        var page = await db.Students.AsNoTracking()
            .OrderBy(student => student.FullName)
            .ThenBy(student => student.Id)
            .Select(StudentMappings.ToResponse)
            .ToPageAsync(query.Page, query.PageSize, cancellationToken);

        return Result<PagedResult<StudentResponse>>.Success(page);
    }
}
