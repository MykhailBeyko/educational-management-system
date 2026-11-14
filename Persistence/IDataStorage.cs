using systemforstudent.Models;

namespace systemforstudent.Persistence;

public interface IDataStorage
{
    List<Student> Students { get; }
    List<Teacher> Teachers { get; }
    List<Subject> Subjects { get; }
    List<Grade> Grades { get; }
    List<Department> Departments { get; }
    List<string> Groups { get; }

    void Save();
}
