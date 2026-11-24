using Domain.Entities;

namespace Application.Common.Abstractions;

public interface IApplicationDbContext
{
    IQueryable<Student> Students { get; }
    IQueryable<Course> Courses { get; }
    IQueryable<Enrollment> Enrollments { get; }

    void AddStudent(Student student);
    void AddCourse(Course course);
    void RemoveEnrollment(Enrollment enrollment);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<bool> TrySaveEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken);
}
