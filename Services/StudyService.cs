using systemforstudent.Models;
using systemforstudent.Persistence;
using systemforstudent.Services.Scales;

namespace systemforstudent.Services;

public class StudyService
{
    private readonly IDataStorage _storage;
    public IGradeScale ActiveScale { get; set; } = new EctsGradeScale();

    public StudyService(IDataStorage storage)
    {
        _storage = storage;
    }

    public List<Student> GetAllStudents() => _storage.Students;
    public List<Teacher> GetAllTeachers() => _storage.Teachers;
    public List<Subject> GetAllSubjects() => _storage.Subjects;
    public List<Grade> GetAllGrades() => _storage.Grades;

    public Student? GetStudentById(int studentId) => _storage.Students.FirstOrDefault(s => s.Id == studentId);
    public Subject? GetSubjectById(int subjectId) => _storage.Subjects.FirstOrDefault(s => s.Id == subjectId);
    public Teacher? GetTeacherById(int teacherId) => _storage.Teachers.FirstOrDefault(t => t.Id == teacherId);

    public bool AssignSubjectToStudent(int studentId, int subjectId)
    {
        var student = GetStudentById(studentId);
        var subject = GetSubjectById(subjectId);

        if (student == null || subject == null) return false;
        if (student.Subjects.Any(s => s.Id == subjectId)) return false;

        student.Subjects.Add(subject);
        _storage.Save();
        return true;
    }

    public List<Subject> GetStudentSubjects(int studentId)
    {
        var student = GetStudentById(studentId);
        return student?.Subjects ?? new List<Subject>();
    }

    // Додавання оцінки з валідацією та авто-визначенням літерної оцінки
    public (bool Success, string Message, Grade? Grade) AddGrade(int studentId, int subjectId, int teacherId, int numericValue)
    {
        if (numericValue < 0 || numericValue > 100)
        {
            return (false, "Оцінка має бути в межах від 0 до 100 балів.", null);
        }

        var student = GetStudentById(studentId);
        if (student == null) return (false, "Студента не знайдено.", null);

        var subject = GetSubjectById(subjectId);
        if (subject == null) return (false, "Предмет не знайдено.", null);

        var teacher = GetTeacherById(teacherId);
        if (teacher == null) return (false, "Викладача не знайдено.", null);

        // Перевірка наявності предмету у студента
        if (!student.Subjects.Any(s => s.Id == subjectId))
        {
            return (false, $"Студент {student.FullName} не вивчає дисципліну \"{subject.Name}\". Спочатку призначте предмет.", null);
        }

        string letterGrade = ActiveScale.ConvertToLetter(numericValue);
        int nextId = _storage.Grades.Count > 0 ? _storage.Grades.Max(g => g.Id) + 1 : 1;

        var grade = new Grade(nextId, student, subject, teacher, numericValue, letterGrade, DateTime.Now);
        _storage.Grades.Add(grade);
        _storage.Save();

        return (true, "Оцінку успішно виставлено.", grade);
    }

    public List<Grade> GetStudentGradesBySubject(int studentId, int subjectId)
    {
        return _storage.Grades
            .Where(g => g.Student.Id == studentId && g.Subject.Id == subjectId)
            .ToList();
    }

    public (bool Success, string Message) UpdateGrade(int gradeId, int newNumericValue)
    {
        if (newNumericValue < 0 || newNumericValue > 100)
        {
            return (false, "Оцінка має бути від 0 до 100.");
        }

        var grade = _storage.Grades.FirstOrDefault(g => g.Id == gradeId);
        if (grade == null) return (false, "Оцінку не знайдено.");

        grade.NumericValue = newNumericValue;
        grade.LetterGrade = ActiveScale.ConvertToLetter(newNumericValue);
        grade.Date = DateTime.Now;

        _storage.Save();
        return (true, "Оцінку оновлено.");
    }

    public bool DeleteGrade(int gradeId)
    {
        var grade = _storage.Grades.FirstOrDefault(g => g.Id == gradeId);
        if (grade == null) return false;

        _storage.Grades.Remove(grade);
        _storage.Save();
        return true;
    }

    // Підрахунок середнього балу по конкретному предмету
    public double? GetStudentSubjectAverage(int studentId, int subjectId)
    {
        var grades = _storage.Grades
            .Where(g => g.Student.Id == studentId && g.Subject.Id == subjectId)
            .ToList();

        if (grades.Count == 0) return null;
        return Math.Round(grades.Average(g => g.NumericValue), 2);
    }

    // Підрахунок середнього балу студента за семестр
    public double? GetStudentSemesterAverage(int studentId, int semester)
    {
        var grades = _storage.Grades
            .Where(g => g.Student.Id == studentId && g.Subject.Semester == semester)
            .ToList();

        if (grades.Count == 0) return null;
        return Math.Round(grades.Average(g => g.NumericValue), 2);
    }

    // Загальний середній бал студента
    public double? GetStudentOverallAverage(int studentId)
    {
        var grades = _storage.Grades
            .Where(g => g.Student.Id == studentId)
            .ToList();

        if (grades.Count == 0) return null;
        return Math.Round(grades.Average(g => g.NumericValue), 2);
    }
}
