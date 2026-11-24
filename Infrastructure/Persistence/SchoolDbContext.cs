using Application.Common.Abstractions;
using Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

internal sealed class SchoolDbContext(DbContextOptions<SchoolDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    private const int SqliteConstraintPrimaryKey = 1555;

    public IQueryable<Student> Students => Set<Student>();
    public IQueryable<Course> Courses => Set<Course>();
    public IQueryable<Enrollment> Enrollments => Set<Enrollment>();

    public void AddStudent(Student student) => Set<Student>().Add(student);

    public void AddCourse(Course course) => Set<Course>().Add(course);

    public void RemoveEnrollment(Enrollment enrollment) => Set<Enrollment>().Remove(enrollment);

    public async Task<bool> TrySaveEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken)
    {
        var entry = Set<Enrollment>().Add(enrollment);
        try
        {
            await SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqliteException { SqliteExtendedErrorCode: SqliteConstraintPrimaryKey } &&
            exception.Entries.Count == 1 && ReferenceEquals(exception.Entries[0].Entity, enrollment))
        {
            entry.State = EntityState.Detached;
            return false;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchoolDbContext).Assembly);
        SchoolSeed.Configure(modelBuilder);
    }
}
