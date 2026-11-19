using systemforstudent.Models;
using systemforstudent.Persistence;
using systemforstudent.Security;

namespace systemforstudent.Services;

public sealed class RoleStudyService
{
    private readonly IDataStorage _storage;
    private readonly UserSession _session;
    private readonly StudyService _study;

    public RoleStudyService(IDataStorage storage, UserSession session)
    {
        _storage = storage;
        _session = session;
        _study = new StudyService(storage);
    }

    public Student CurrentStudent
    {
        get
        {
            _session.RequireRole(AppRoles.Student);
            return _storage.Students.Single(s => s.Id == _session.EntityId);
        }
    }

    public Teacher CurrentTeacher
    {
        get
        {
            _session.RequireRole(AppRoles.Teacher);
            return _storage.Teachers.Single(t => t.Id == _session.EntityId);
        }
    }

    public Department CurrentDepartment
    {
        get
        {
            _session.RequireRole(AppRoles.DepartmentHead);
            return _storage.Departments.Single(d => d.Id == _session.EntityId);
        }
    }

    public List<Grade> GetMyGrades()
    {
        var student = CurrentStudent;
        return _storage.Grades.Where(g => g.Student.Id == student.Id).ToList();
    }

    public List<(Student Student, double AverageScore)> GetMyGroupRanking()
    {
        return _study.GetGroupRanking(CurrentStudent.Group);
    }

    public List<Student> GetMyStudents()
    {
        var subjectIds = CurrentTeacher.Subjects.Select(s => s.Id).ToHashSet();
        return _storage.Students
            .Where(s => s.Subjects.Any(sub => subjectIds.Contains(sub.Id)))
            .ToList();
    }

    public List<Grade> GetTeacherGrades()
    {
        var teacher = CurrentTeacher;
        return _storage.Grades.Where(g => g.Teacher.Id == teacher.Id).ToList();
    }

    public (bool Success, string Message, Grade? Grade) AddGrade(int studentId, int subjectId, int value)
    {
        var teacher = CurrentTeacher;
        if (!teacher.Subjects.Any(s => s.Id == subjectId))
        {
            throw new UnauthorizedAccessException("Можна виставляти оцінки лише зі своїх предметів.");
        }

        return _study.AddGrade(studentId, subjectId, teacher.Id, value);
    }

    private void RequireOwnGrade(int gradeId)
    {
        var teacher = CurrentTeacher;
        var grade = _storage.Grades.SingleOrDefault(g => g.Id == gradeId);
        if (grade == null
            || grade.Teacher.Id != teacher.Id
            || !teacher.Subjects.Any(s => s.Id == grade.Subject.Id))
        {
            throw new UnauthorizedAccessException("Можна змінювати лише власні оцінки зі своїх предметів.");
        }
    }

    public (bool Success, string Message) UpdateGrade(int gradeId, int value)
    {
        RequireOwnGrade(gradeId);
        return _study.UpdateGrade(gradeId, value);
    }

    public bool DeleteGrade(int gradeId)
    {
        RequireOwnGrade(gradeId);
        return _study.DeleteGrade(gradeId);
    }

    public string StudentReport(int? semester = null)
    {
        var student = CurrentStudent;
        using var output = new StringWriter();
        var reporting = new ReportingService(_storage, output);
        if (semester.HasValue)
        {
            reporting.PrintStudentSemesterReport(student.Id, semester.Value);
        }
        else
        {
            reporting.PrintStudentFullAcademicReport(student.Id);
        }

        return output.ToString();
    }

    public string TeacherReport(int semester)
    {
        var teacher = CurrentTeacher;
        using var output = new StringWriter();
        new ReportingService(_storage, output).PrintTeacherReport(teacher.Id, semester);
        return output.ToString();
    }

    public string DepartmentReport()
    {
        var department = CurrentDepartment;
        using var output = new StringWriter();
        new ReportingService(_storage, output).PrintDepartmentReport(department.Name);
        _study.PrintDepartmentAdvancedStats(department.Id, output);
        return output.ToString();
    }
}
