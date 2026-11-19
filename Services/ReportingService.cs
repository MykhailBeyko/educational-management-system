using systemforstudent.Persistence;

namespace systemforstudent.Services;

public class ReportingService
{
    private readonly IDataStorage _storage;
    private readonly TextWriter _output;

    public ReportingService(IDataStorage storage, TextWriter? output = null)
    {
        _storage = storage;
        _output = output ?? Console.Out;
    }

    // 1. Посеместровий табель успішності студента
    public void PrintStudentSemesterReport(int studentId, int semester)
    {
        var student = _storage.Students.FirstOrDefault(s => s.Id == studentId);
        if (student == null)
        {
            _output.WriteLine("Студента не знайдено.");
            return;
        }

        var semesterSubjects = student.Subjects.Where(s => s.Semester == semester).ToList();
        var semesterGrades = _storage.Grades
            .Where(g => g.Student.Id == studentId && g.Subject.Semester == semester)
            .ToList();

        _output.WriteLine($"\n=======================================================");
        _output.WriteLine($"ТАБЕЛЬ УСПІШНОСТІ: {student.FullName} (Група {student.Group})");
        _output.WriteLine($"Семестр: {semester}");
        _output.WriteLine($"-------------------------------------------------------");

        if (semesterSubjects.Count == 0 && semesterGrades.Count == 0)
        {
            _output.WriteLine("Немає даних за цей семестр.");
            _output.WriteLine("=======================================================");
            return;
        }

        foreach (var sub in semesterSubjects)
        {
            var grades = semesterGrades.Where(g => g.Subject.Id == sub.Id).ToList();
            string gradesStr = grades.Count > 0
                ? string.Join(", ", grades.Select(g => $"{g.NumericValue}({g.LetterGrade})"))
                : "немає оцінок";

            double? avg = grades.Count > 0 ? Math.Round(grades.Average(g => g.NumericValue), 2) : null;
            _output.WriteLine($"• {sub.Name} [{sub.Credits} кр.]: {gradesStr} | Сер. бал: {(avg.HasValue ? avg.Value.ToString() : "-")}");
        }

        if (semesterGrades.Count > 0)
        {
            double totalAvg = Math.Round(semesterGrades.Average(g => g.NumericValue), 2);
            _output.WriteLine($"-------------------------------------------------------");
            _output.WriteLine($"СЕРЕДНІЙ БАЛ ЗА СЕМЕСТР: {totalAvg}");
        }
        _output.WriteLine($"=======================================================\n");
    }

    // 2. Підсумкова таблиця успішності за всі семестри (згрупована за роком і семестром навчання)
    public void PrintStudentFullAcademicReport(int studentId)
    {
        var student = _storage.Students.FirstOrDefault(s => s.Id == studentId);
        if (student == null)
        {
            _output.WriteLine("Студента не знайдено.");
            return;
        }

        var allGrades = _storage.Grades.Where(g => g.Student.Id == studentId).ToList();

        _output.WriteLine($"\n=======================================================");
        _output.WriteLine($"ПІДСУМКОВА ТАБЛИЦЯ УСПІШНОСТІ: {student.FullName}");
        _output.WriteLine($"Група: {student.Group} | Рік вступу: {student.AdmissionYear}");
        _output.WriteLine($"-------------------------------------------------------");

        if (allGrades.Count == 0)
        {
            _output.WriteLine("У студента ще немає жодної оцінки.");
            _output.WriteLine("=======================================================");
            return;
        }

        // Групуємо за роком навчання (вираховуємо від року вступу) та семестром
        var groupedByYearAndSem = allGrades
            .GroupBy(g => new
            {
                StudyYear = student.AdmissionYear + (g.Subject.Semester - 1) / 2,
                Semester = g.Subject.Semester
            })
            .OrderBy(x => x.Key.StudyYear)
            .ThenBy(x => x.Key.Semester);

        foreach (var group in groupedByYearAndSem)
        {
            int course = group.Key.StudyYear - student.AdmissionYear + 1;
            _output.WriteLine($"\n--- {group.Key.StudyYear} н.р. ({course} курс), Семестр {group.Key.Semester} ---");
            foreach (var grade in group)
            {
                _output.WriteLine($"  {grade.Subject.Name,-35} | {grade.NumericValue,3} ({grade.LetterGrade}) | Викл: {grade.Teacher.FullName}");
            }
            _output.WriteLine($"  -> Середній бал семестру: {Math.Round(group.Average(g => g.NumericValue), 2)}");
        }

        _output.WriteLine($"-------------------------------------------------------");
        _output.WriteLine($"ЗАГАЛЬНИЙ РЕЙТИНГОВИЙ БАЛ ЗА ВСІ СЕМЕСТРИ: {Math.Round(allGrades.Average(g => g.NumericValue), 2)}");
        _output.WriteLine($"=======================================================\n");
    }

    // 3. Статистика по групі за семестр
    public void PrintGroupStatistics(string group, int semester)
    {
        var groupStudents = _storage.Students
            .Where(s => s.Group.Equals(group, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (groupStudents.Count == 0)
        {
            _output.WriteLine($"Групу \"{group}\" не знайдено.");
            return;
        }

        var studentIds = groupStudents.Select(s => s.Id).ToHashSet();
        var semesterGrades = _storage.Grades
            .Where(g => studentIds.Contains(g.Student.Id) && g.Subject.Semester == semester)
            .ToList();

        _output.WriteLine($"\n=== СТАТИСТИКА ГРУПИ {group.ToUpper()} (Семестр {semester}) ===");
        _output.WriteLine($"Кількість студентів у групі: {groupStudents.Count}");

        if (semesterGrades.Count == 0)
        {
            _output.WriteLine("Немає виставлених оцінок за цей семестр.");
            return;
        }

        double groupAvg = Math.Round(semesterGrades.Average(g => g.NumericValue), 2);
        _output.WriteLine($"Середній бал групи за семестр: {groupAvg}");

        var failedGrades = semesterGrades.Where(g => g.NumericValue < 60).ToList();
        int failedStudentsCount = failedGrades.Select(g => g.Student.Id).Distinct().Count();

        _output.WriteLine($"Кількість студентів, які не склали предмет (<60 балів): {failedStudentsCount}");
        if (failedGrades.Count > 0)
        {
            _output.WriteLine("Деталі по заборгованостях:");
            foreach (var fg in failedGrades)
            {
                _output.WriteLine($" - {fg.Student.FullName} з предмету \"{fg.Subject.Name}\" ({fg.NumericValue} б. - {fg.LetterGrade})");
            }
        }
        else
        {
            _output.WriteLine("Усі студенти успішно склали предмети!");
        }
    }

    // 4. Звіти для викладачів
    public void PrintTeacherReport(int teacherId, int semester)
    {
        var teacher = _storage.Teachers.FirstOrDefault(t => t.Id == teacherId);
        if (teacher == null)
        {
            _output.WriteLine("Викладача не знайдено.");
            return;
        }

        var teacherGrades = _storage.Grades
            .Where(g => g.Teacher.Id == teacherId && g.Subject.Semester == semester)
            .ToList();

        _output.WriteLine($"\n=== ЗВІТ ВИКЛАДАЧА: {teacher.FullName} ===");
        _output.WriteLine($"Кафедра: {teacher.Department} | Семестр: {semester}");

        if (teacherGrades.Count == 0)
        {
            _output.WriteLine("У цьому семестрі викладач ще не виставляв оцінок.");
            return;
        }

        _output.WriteLine($"Кількість оцінок за семестр: {teacherGrades.Count}");
        _output.WriteLine($"Загальний середній бал: {Math.Round(teacherGrades.Average(g => g.NumericValue), 2)}");

        _output.WriteLine("\nСередній бал студентів по предметах:");
        var bySubject = teacherGrades.GroupBy(g => g.Subject.Name);
        foreach (var subGroup in bySubject)
        {
            _output.WriteLine($" - {subGroup.Key}: оцінок - {subGroup.Count()}, середній бал - {Math.Round(subGroup.Average(g => g.NumericValue), 2)}");
        }
    }

    // 5. Звіти для завідувача кафедри
    public void PrintDepartmentReport(string department)
    {
        var teachers = _storage.Teachers
            .Where(t => t.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (teachers.Count == 0)
        {
            _output.WriteLine($"Кафедру \"{department}\" не знайдено або на ній немає викладачів.");
            return;
        }

        var teacherIds = teachers.Select(t => t.Id).ToHashSet();
        var departmentGrades = _storage.Grades.Where(g => teacherIds.Contains(g.Teacher.Id)).ToList();

        var departmentSubjects = teachers.SelectMany(t => t.Subjects).DistinctBy(s => s.Id).ToList();
        var subjectIds = departmentSubjects.Select(s => s.Id).ToHashSet();

        // Студенти, що вивчають предмети кафедри або мають оцінки від її викладачів
        var studentIdsFromSubjects = _storage.Students
            .Where(s => s.Subjects.Any(sub => subjectIds.Contains(sub.Id)))
            .Select(s => s.Id);
        var studentIdsFromGrades = departmentGrades.Select(g => g.Student.Id);
        int studentsCount = studentIdsFromSubjects.Union(studentIdsFromGrades).Distinct().Count();

        _output.WriteLine($"\n=== ЗВІТ ДЛЯ ЗАВІДУВАЧА КАФЕДРИ: {department.ToUpper()} ===");
        _output.WriteLine($"Кількість викладачів: {teachers.Count}");
        _output.WriteLine($"Кількість предметів: {departmentSubjects.Count}");
        _output.WriteLine($"Кількість студентів: {studentsCount}");

        if (departmentGrades.Count > 0)
        {
            double avgScore = Math.Round(departmentGrades.Average(g => g.NumericValue), 2);
            _output.WriteLine($"Середній бал кафедри: {avgScore}");
        }
        else
        {
            _output.WriteLine("Оцінок по дисциплінах кафедри ще немає.");
        }
    }
}
