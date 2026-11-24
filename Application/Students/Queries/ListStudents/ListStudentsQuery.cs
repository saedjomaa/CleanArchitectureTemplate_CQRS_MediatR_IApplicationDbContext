using Application.Common.Pagination;
using Application.Common.Responses;
using Application.Common.Results;
using MediatR;

namespace Application.Students.Queries.ListStudents;

public sealed record ListStudentsQuery(int Page = Pagination.DefaultPage, int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PagedResult<StudentResponse>>>;
