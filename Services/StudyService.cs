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

    public List<Department> GetAllDepartments() => _storage.Departments;

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

    public Department? GetDepartmentById(int depId)
    {
        return _storage.Departments.FirstOrDefault(d => d.Id == depId);
    }

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
            return false;
        }

        student.Subjects.Add(subject);
        _storage.Save();
        return true;
    }

    public List<Subject> GetStudentSubjects(int studentId)
    {
        var student = GetStudentById(studentId);
        return student?.Subjects ?? new List<Subject>();
    }

    // Додавання оцінки з валідацією та автоматичним визначенням літерної оцінки
    public (bool Success, string Message, Grade? Grade) AddGrade(int studentId, int subjectId, int teacherId, int numericValue)
    {
        if (numericValue < 0 || numericValue > 100)
        {
            return (false, "Оцінка має бути в межах від 0 до 100 балів.", null);
        }

        var student = GetStudentById(studentId);
        if (student == null)
        {
            return (false, "Студента не знайдено.", null);
        }

        var subject = GetSubjectById(subjectId);
        if (subject == null)
        {
            return (false, "Предмет не знайдено.", null);
        }

        var teacher = GetTeacherById(teacherId);
        if (teacher == null)
        {
            return (false, "Викладача не знайдено.", null);
        }

        if (!teacher.Subjects.Any(s => s.Id == subjectId))
        {
            return (false, "Викладач не викладає цей предмет.", null);
        }
        // Валідація: студент має вивчати цей предмет
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
        return _storage.Grades.Where(g => g.Student.Id == studentId && g.Subject.Id == subjectId)
            .ToList();
    }

    public (bool Success, string Message) UpdateGrade(int gradeId, int newNumericValue)
    {
        if (newNumericValue < 0 || newNumericValue > 100)
        {
            return (false, "Оцінка має бути від 0 до 100.");
        }

        var grade = _storage.Grades.FirstOrDefault(g => g.Id == gradeId);
        if (grade == null)
        {
            return (false, "Оцінку не знайдено.");
        }

        grade.NumericValue = newNumericValue;
        grade.LetterGrade = ActiveScale.ConvertToLetter(newNumericValue);
        grade.Date = DateTime.Now;
        _storage.Save();
        return (true, "Оцінку оновлено.");
    }

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

    public double? GetStudentSubjectAverage(int studentId, int subjectId)
    {
        var grades = _storage.Grades.Where(g => g.Student.Id == studentId && g.Subject.Id == subjectId)
            .ToList();
        if (grades.Count == 0)
        {
            return null;
        }

        return Math.Round(grades.Average(g => g.NumericValue), 2);
    }

    public double? GetStudentSemesterAverage(int studentId, int semester)
    {
        var grades = _storage.Grades.Where(g => g.Student.Id == studentId && g.Subject.Semester == semester)
            .ToList();
        if (grades.Count == 0)
        {
            return null;
        }

        return Math.Round(grades.Average(g => g.NumericValue), 2);
    }

    public double? GetStudentOverallAverage(int studentId)
    {
        var grades = _storage.Grades.Where(g => g.Student.Id == studentId).ToList();
        if (grades.Count == 0)
        {
            return null;
        }

        return Math.Round(grades.Average(g => g.NumericValue), 2);
    }

    // ================= ЕТАП 4: НОВІ ФУНКЦІЇ =================
    // Додавання нової кафедри
    public Department AddDepartment(string name, string headOfDepartment)
    {
        int nextId = _storage.Departments.Count > 0 ? _storage.Departments.Max(d => d.Id) + 1 : 1;
        var dep = new Department(nextId, name, headOfDepartment);
        _storage.Departments.Add(dep);
        _storage.Save();
        return dep;
    }

    // Зміна посади викладача
    public bool UpdateTeacherPosition(int teacherId, string newPosition)
    {
        var teacher = GetTeacherById(teacherId);
        if (teacher == null)
        {
            return false;
        }

        teacher.Position = newPosition;
        _storage.Save();
        return true;
    }

    // Призначення викладача до кафедри
    public bool AssignTeacherToDepartment(int teacherId, int departmentId)
    {
        var teacher = GetTeacherById(teacherId);
        var department = GetDepartmentById(departmentId);
        if (teacher == null || department == null)
        {
            return false;
        }

        foreach (var previous in _storage.Departments.Where(d => d.Id != departmentId))
        {
            previous.Teachers.RemoveAll(t => t.Id == teacherId);
        }

        teacher.Department = department.Name;
        if (!department.Teachers.Any(t => t.Id == teacherId))
        {
            department.Teachers.Add(teacher);
        }

        _storage.Save();
        return true;
    }

    // Рейтинг студентів академічної групи за середнім балом (спадаючий)
    public List<(Student Student, double AverageScore)> GetGroupRanking(string group)
    {
        var students = _storage.Students.Where(s => s.Group.Equals(group, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var ranking = new List<(Student Student, double AverageScore)>();
        foreach (var student in students)
        {
            var avg = GetStudentOverallAverage(student.Id) ?? 0.0;
            ranking.Add((student, avg));
        }

        return ranking.OrderByDescending(r => r.AverageScore).ToList();
    }

    // Фільтрація студентів за групою, семестром (наявність предметів цього семестру) та мін. середнім балом
    public List<Student> FilterStudents(string? group, int? semester, double? minGpa)
    {
        var query = _storage.Students.AsQueryable();
        if (!string.IsNullOrWhiteSpace(group))
        {
            query = query.Where(s => s.Group.Equals(group, StringComparison.OrdinalIgnoreCase));
        }

        if (semester.HasValue)
        {
            query = query.Where(s => s.Subjects.Any(sub => sub.Semester == semester.Value));
        }

        var result = query.ToList();
        if (minGpa.HasValue)
        {
            result = result.Where(s => (GetStudentOverallAverage(s.Id) ?? 0.0) >= minGpa.Value).ToList();
        }

        return result;
    }

    // Розширена статистика кафедри: кількість предметів, середній бал кафедри, ТОП-5 студентів
    public void PrintDepartmentAdvancedStats(int departmentId, TextWriter? output = null)
    {
        output ??= Console.Out;
        var department = GetDepartmentById(departmentId);
        if (department == null)
        {
            output.WriteLine("Кафедру не знайдено.");
            return;
        }

        var teachers = _storage.Teachers
            .Where(t => t.Department.Equals(department.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var subjects = teachers.SelectMany(t => t.Subjects).DistinctBy(s => s.Id).ToList();
        var teacherIds = teachers.Select(t => t.Id).ToHashSet();
        var departmentGrades = _storage.Grades.Where(g => teacherIds.Contains(g.Teacher.Id)).ToList();
        output.WriteLine($"\n=======================================================");
        output.WriteLine($"СТАТИСТИКА КАФЕДРИ: \"{department.Name}\"");
        output.WriteLine($"Завідувач кафедри: {department.HeadOfDepartment}");
        output.WriteLine($"Кількість викладачів: {teachers.Count}");
        output.WriteLine($"Кількість дисциплін кафедри: {subjects.Count}");
        if (subjects.Count > 0)
        {
            output.WriteLine("Дисципліни кафедри:");
            foreach (var sub in subjects)
            {
                output.WriteLine($" - {sub.Name} ({sub.Credits} кр., {sub.Semester} семестр)");
            }
        }

        if (departmentGrades.Count > 0)
        {
            double depAvg = Math.Round(departmentGrades.Average(g => g.NumericValue), 2);
            output.WriteLine($"\nСередній бал студентів кафедри: {depAvg}");
        }
        else
        {
            output.WriteLine("\nСередній бал студентів кафедри: оцінок ще немає.");
        }
        // ТОП-5 студентів за середнім балом з предметів кафедри
        var top5 = departmentGrades
            .GroupBy(g => g.Student)
            .Select(group => new
            {
                Student = group.Key,
                Avg = Math.Round(group.Average(g => g.NumericValue), 2)
            })
            .OrderByDescending(x => x.Avg)
            .Take(5)
            .ToList();
        output.WriteLine($"\nТОП-5 СТУДЕНТІВ ЗА УСПІШНІСТЮ НА КАФЕДРІ:");
        if (top5.Count == 0)
        {
            output.WriteLine("Ще немає оцінених студентів на дисциплінах цієї кафедри.");
        }
        else
        {
            int place = 1;
            foreach (var item in top5)
            {
                output.WriteLine($" {place++}. {item.Student.FullName} (Група {item.Student.Group}) — Сер. бал: {item.Avg}");
            }
        }

        output.WriteLine($"=======================================================\n");
    }

    // ================= КЕРУВАННЯ ГРУПАМИ ТА СТУДЕНТАМИ =================
    // Список усіх груп
    public List<string> GetAllGroups()
    {
        return _storage.Groups.Union(_storage.Students.Select(s => s.Group))
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g)
            .ToList();
    }

    // Створення нової академічної групи
    public bool AddGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return false;
        }

        string normalized = groupName.Trim();
        if (_storage.Groups.Any(g => g.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return false; // вже є така група
        }

        _storage.Groups.Add(normalized);
        _storage.Save();
        return true;
    }

    // Додавання нового студента та запис його в групу
    public Student AddStudent(string fullName, string group, int admissionYear)
    {
        string normGroup = group.Trim();
        if (!_storage.Groups.Any(g => g.Equals(normGroup, StringComparison.OrdinalIgnoreCase)))
        {
            _storage.Groups.Add(normGroup);
        }

        int nextId = _storage.Students.Count > 0 ? _storage.Students.Max(s => s.Id) + 1 : 1;
        var student = new Student(nextId, fullName.Trim(), normGroup, admissionYear);
        _storage.Students.Add(student);
        _storage.Save();
        return student;
    }

    // Переведення студента в іншу групу
    public bool ChangeStudentGroup(int studentId, string newGroup)
    {
        var student = GetStudentById(studentId);
        if (student == null || string.IsNullOrWhiteSpace(newGroup))
        {
            return false;
        }

        string normGroup = newGroup.Trim();
        student.Group = normGroup;
        if (!_storage.Groups.Any(g => g.Equals(normGroup, StringComparison.OrdinalIgnoreCase)))
        {
            _storage.Groups.Add(normGroup);
        }

        _storage.Save();
        return true;
    }

    // Отримати студентів певної групи
    public List<Student> GetStudentsByGroup(string group)
    {
        return _storage.Students.Where(s => s.Group.Equals(group, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
