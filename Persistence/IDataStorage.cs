using systemforstudent.Models;

namespace systemforstudent.Persistence;

public interface IDataStorage
{
    List<Student> Students { get; }
    List<Teacher> Teachers { get; }
    List<Subject> Subjects { get; }
    List<Grade> Grades { get; }

    void Save();
}
