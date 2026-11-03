using systemforstudent.Models;
using systemforstudent.Persistence;

namespace systemforstudent.Services;

public class StudyService
{
    private readonly IDataStorage _storage;

    public StudyService(IDataStorage storage)
    {
        _storage = storage;
    }

    public List<Student> GetAllStudents() => _storage.Students;
    public List<Teacher> GetAllTeachers() => _storage.Teachers;
    public List<Subject> GetAllSubjects() => _storage.Subjects;
    public List<Grade> GetAllGrades() => _storage.Grades;

    public Student? GetStudentById(int studentId)
    {
        return _storage.Students.FirstOrDefault(s => s.Id == studentId);
    }

    public Subject? GetSubjectById(int subjectId)
    {
        return _storage.Subjects.FirstOrDefault(s => s.Id == subjectId);
    }

    public Teacher? GetTeacherById(int teacherId)
    {
        return _storage.Teachers.FirstOrDefault(t => t.Id == teacherId);
    }

    // Призначити студенту предмет
    public bool AssignSubjectToStudent(int studentId, int subjectId)
    {
        var student = GetStudentById(studentId);
        var subject = GetSubjectById(subjectId);

        if (student == null || subject == null)
        {
            return false;
        }

        if (student.Subjects.Any(s => s.Id == subjectId))
        {
            return false; // предмет уже призначено
        }

        student.Subjects.Add(subject);
        _storage.Save();
        return true;
    }

    // Отримати список предметів студента
    public List<Subject> GetStudentSubjects(int studentId)
    {
        var student = GetStudentById(studentId);
        return student?.Subjects ?? new List<Subject>();
    }

    // Додати оцінку студенту з певного предмету
    public Grade? AddGrade(int studentId, int subjectId, int teacherId, int numericValue, string letterGrade)
    {
        var student = GetStudentById(studentId);
        var subject = GetSubjectById(subjectId);
        var teacher = GetTeacherById(teacherId);

        if (student == null || subject == null || teacher == null)
        {
            return null;
        }

        int nextId = _storage.Grades.Count > 0 ? _storage.Grades.Max(g => g.Id) + 1 : 1;
        var grade = new Grade(nextId, student, subject, teacher, numericValue, letterGrade, DateTime.Now);

        _storage.Grades.Add(grade);
        _storage.Save();

        return grade;
    }

    // Отримати список оцінок студента за певний предмет
    public List<Grade> GetStudentGradesBySubject(int studentId, int subjectId)
    {
        return _storage.Grades
            .Where(g => g.Student.Id == studentId && g.Subject.Id == subjectId)
            .ToList();
    }

    // Відредагувати оцінку
    public bool UpdateGrade(int gradeId, int newNumericValue, string newLetterGrade)
    {
        var grade = _storage.Grades.FirstOrDefault(g => g.Id == gradeId);
        if (grade == null)
        {
            return false;
        }

        grade.NumericValue = newNumericValue;
        grade.LetterGrade = newLetterGrade;
        grade.Date = DateTime.Now;

        _storage.Save();
        return true;
    }

    // Видалити оцінку
    public bool DeleteGrade(int gradeId)
    {
        var grade = _storage.Grades.FirstOrDefault(g => g.Id == gradeId);
        if (grade == null)
        {
            return false;
        }

        _storage.Grades.Remove(grade);
        _storage.Save();
        return true;
    }
}
