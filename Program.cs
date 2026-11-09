using System.Text;
using systemforstudent.Persistence;
using systemforstudent.Services;
using systemforstudent.Services.Scales;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

IDataStorage storage = new JsonDataStorage();
StudyService service = new StudyService(storage);

while (true)
{
    Console.WriteLine($"\n=== СИСТЕМА УПРАВЛІННЯ НАВЧАЛЬНИМ ПРОЦЕСОМ (Етап 2) ===");
    Console.WriteLine($"Поточна шкала оцінювання: {service.ActiveScale.Name}");
    Console.WriteLine("-----------------------------------------------------");
    Console.WriteLine("1. Список студентів");
    Console.WriteLine("2. Список предметів");
    Console.WriteLine("3. Список викладачів");
    Console.WriteLine("4. Призначити предмет студенту");
    Console.WriteLine("5. Список предметів студента");
    Console.WriteLine("6. Додати оцінку студенту (автовизначення літери + валідація)");
    Console.WriteLine("7. Список оцінок студента з конкретного предмету");
    Console.WriteLine("8. Редагувати оцінку");
    Console.WriteLine("9. Видалити оцінку");
    Console.WriteLine("10. Всі поточні оцінки в системі");
    Console.WriteLine("11. Середній бал студента (по предмету / за семестр / загальний)");
    Console.WriteLine("12. Змінити шкалу оцінювання");
    Console.WriteLine("0. Вихід");
    Console.Write("Оберіть пункт меню: ");

    string? choice = Console.ReadLine();
    Console.WriteLine();

    switch (choice)
    {
        case "1":
            ShowStudents(service);
            break;
        case "2":
            ShowSubjects(service);
            break;
        case "3":
            ShowTeachers(service);
            break;
        case "4":
            AssignSubject(service);
            break;
        case "5":
            ShowStudentSubjects(service);
            break;
        case "6":
            AddGrade(service);
            break;
        case "7":
            ShowStudentGradesBySubject(service);
            break;
        case "8":
            EditGrade(service);
            break;
        case "9":
            DeleteGrade(service);
            break;
        case "10":
            ShowAllGrades(service);
            break;
        case "11":
            ShowAverageScores(service);
            break;
        case "12":
            ChangeScale(service);
            break;
        case "0":
            Console.WriteLine("Завершення роботи програми.");
            return;
        default:
            Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
            break;
    }
}

static void ShowStudents(StudyService service)
{
    Console.WriteLine("--- Список студентів ---");
    foreach (var s in service.GetAllStudents())
    {
        Console.WriteLine(s);
    }
}

static void ShowSubjects(StudyService service)
{
    Console.WriteLine("--- Список предметів ---");
    foreach (var sub in service.GetAllSubjects())
    {
        Console.WriteLine(sub);
    }
}

static void ShowTeachers(StudyService service)
{
    Console.WriteLine("--- Список викладачів ---");
    foreach (var t in service.GetAllTeachers())
    {
        Console.WriteLine(t);
    }
}

static void AssignSubject(StudyService service)
{
    Console.WriteLine("--- Призначення предмету студенту ---");
    ShowStudents(service);
    Console.Write("Введіть ID студента: ");
    if (!int.TryParse(Console.ReadLine(), out int studentId)) return;

    ShowSubjects(service);
    Console.Write("Введіть ID предмету: ");
    if (!int.TryParse(Console.ReadLine(), out int subjectId)) return;

    bool success = service.AssignSubjectToStudent(studentId, subjectId);
    if (success)
    {
        Console.WriteLine("Предмет успішно призначено студенту!");
    }
    else
    {
        Console.WriteLine("Помилка: студента чи предмет не знайдено, або предмет уже призначено.");
    }
}

static void ShowStudentSubjects(StudyService service)
{
    Console.WriteLine("--- Предмети студента ---");
    Console.Write("Введіть ID студента: ");
    if (!int.TryParse(Console.ReadLine(), out int studentId)) return;

    var student = service.GetStudentById(studentId);
    if (student == null)
    {
        Console.WriteLine("Студента не знайдено.");
        return;
    }

    var subjects = service.GetStudentSubjects(studentId);
    Console.WriteLine($"Предмети студента {student.FullName}:");
    if (subjects.Count == 0)
    {
        Console.WriteLine("У студента ще немає призначених предметів.");
    }
    else
    {
        foreach (var sub in subjects)
        {
            Console.WriteLine($" - {sub.Name} (Семестр: {sub.Semester}, Кредити: {sub.Credits})");
        }
    }
}

static void AddGrade(StudyService service)
{
    Console.WriteLine("--- Додавання оцінки ---");
    ShowStudents(service);
    Console.Write("Введіть ID студента: ");
    if (!int.TryParse(Console.ReadLine(), out int studentId)) return;

    ShowSubjects(service);
    Console.Write("Введіть ID предмету: ");
    if (!int.TryParse(Console.ReadLine(), out int subjectId)) return;

    ShowTeachers(service);
    Console.Write("Введіть ID викладача: ");
    if (!int.TryParse(Console.ReadLine(), out int teacherId)) return;

    Console.Write("Введіть числову оцінку (0-100): ");
    if (!int.TryParse(Console.ReadLine(), out int numericValue)) return;

    var result = service.AddGrade(studentId, subjectId, teacherId, numericValue);
    if (result.Success && result.Grade != null)
    {
        Console.WriteLine($"Успіх! {result.Message}");
        Console.WriteLine($"Зафіксовано: {result.Grade}");
    }
    else
    {
        Console.WriteLine($"Помилка валідації: {result.Message}");
    }
}

static void ShowStudentGradesBySubject(StudyService service)
{
    Console.WriteLine("--- Оцінки студента з предмету ---");
    Console.Write("Введіть ID студента: ");
    if (!int.TryParse(Console.ReadLine(), out int studentId)) return;

    Console.Write("Введіть ID предмету: ");
    if (!int.TryParse(Console.ReadLine(), out int subjectId)) return;

    var grades = service.GetStudentGradesBySubject(studentId, subjectId);
    if (grades.Count == 0)
    {
        Console.WriteLine("Оцінок з цього предмету не знайдено.");
    }
    else
    {
        foreach (var g in grades)
        {
            Console.WriteLine(g);
        }
    }
}

static void EditGrade(StudyService service)
{
    Console.WriteLine("--- Редагування оцінки ---");
    ShowAllGrades(service);
    Console.Write("Введіть ID оцінки для редагування: ");
    if (!int.TryParse(Console.ReadLine(), out int gradeId)) return;

    Console.Write("Нова числова оцінка (0-100): ");
    if (!int.TryParse(Console.ReadLine(), out int newNumeric)) return;

    var result = service.UpdateGrade(gradeId, newNumeric);
    Console.WriteLine(result.Message);
}

static void DeleteGrade(StudyService service)
{
    Console.WriteLine("--- Видалення оцінки ---");
    ShowAllGrades(service);
    Console.Write("Введіть ID оцінки для видалення: ");
    if (!int.TryParse(Console.ReadLine(), out int gradeId)) return;

    bool success = service.DeleteGrade(gradeId);
    if (success)
    {
        Console.WriteLine("Оцінку успішно видалено!");
    }
    else
    {
        Console.WriteLine("Оцінку з таким ID не знайдено.");
    }
}

static void ShowAllGrades(StudyService service)
{
    Console.WriteLine("--- Усі оцінки ---");
    var grades = service.GetAllGrades();
    if (grades.Count == 0)
    {
        Console.WriteLine("В системі ще немає оцінок.");
    }
    else
    {
        foreach (var g in grades)
        {
            Console.WriteLine(g);
        }
    }
}

static void ShowAverageScores(StudyService service)
{
    Console.WriteLine("--- Розрахунок середнього балу ---");
    ShowStudents(service);
    Console.Write("Введіть ID студента: ");
    if (!int.TryParse(Console.ReadLine(), out int studentId)) return;

    var student = service.GetStudentById(studentId);
    if (student == null)
    {
        Console.WriteLine("Студента не знайдено.");
        return;
    }

    Console.WriteLine("1. Середній бал по конкретному предмету");
    Console.WriteLine("2. Середній бал за семестр");
    Console.WriteLine("3. Загальний середній бал за весь час");
    Console.Write("Вибір: ");
    string? subChoice = Console.ReadLine();

    switch (subChoice)
    {
        case "1":
            ShowSubjects(service);
            Console.Write("Введіть ID предмету: ");
            if (int.TryParse(Console.ReadLine(), out int subjectId))
            {
                var avg = service.GetStudentSubjectAverage(studentId, subjectId);
                Console.WriteLine(avg.HasValue ? $"Середній бал: {avg.Value}" : "Оцінок з цього предмету немає.");
            }
            break;
        case "2":
            Console.Write("Введіть номер семестру (1, 2, ...): ");
            if (int.TryParse(Console.ReadLine(), out int semester))
            {
                var avg = service.GetStudentSemesterAverage(studentId, semester);
                Console.WriteLine(avg.HasValue ? $"Середній бал за {semester}-й семестр: {avg.Value}" : "Оцінок за цей семестр немає.");
            }
            break;
        case "3":
            var overall = service.GetStudentOverallAverage(studentId);
            Console.WriteLine(overall.HasValue ? $"Загальний середній бал: {overall.Value}" : "Оцінок немає.");
            break;
    }
}

static void ChangeScale(StudyService service)
{
    Console.WriteLine("--- Вибір шкали оцінювання ---");
    Console.WriteLine("1. ECTS (A, B, C, D, E, Fx, F)");
    Console.WriteLine("2. Національна шкала (Відмінно, Добре, Задовільно, Незадовільно)");
    Console.WriteLine("3. Ввести власну шкалу");
    Console.Write("Оберіть шкалу: ");
    string? ch = Console.ReadLine();

    if (ch == "1")
    {
        service.ActiveScale = new EctsGradeScale();
        Console.WriteLine("Встановлено шкалу ECTS.");
    }
    else if (ch == "2")
    {
        service.ActiveScale = new TraditionalGradeScale();
        Console.WriteLine("Встановлено Національну шкалу.");
    }
    else if (ch == "3")
    {
        Console.Write("Назва нової шкали (наприклад, Залік): ");
        string name = Console.ReadLine()?.Trim() ?? "Власна шкала";
        if (string.IsNullOrEmpty(name)) name = "Власна шкала";

        var rules = new List<(int MinScore, string Label)>();
        Console.WriteLine("Вводьте пороги (від мінімального балу до літери/оцінки). Натисніть Enter на порожній назві, щоб завершити.");

        while (true)
        {
            Console.Write("Позначка (наприклад A, 5, Зараховано): ");
            string? label = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(label)) break;

            Console.Write($"Мінімальний бал для \"{label}\": ");
            if (int.TryParse(Console.ReadLine(), out int minScore))
            {
                rules.Add((minScore, label));
            }
            else
            {
                Console.WriteLine("Некоректне число, спробуйте ще раз.");
            }
        }

        if (rules.Count > 0)
        {
            service.ActiveScale = new CustomGradeScale(name, rules);
            Console.WriteLine($"Встановлено власну шкалу: {name}");
        }
        else
        {
            Console.WriteLine("Не додано жодного правила, шкалу не змінено.");
        }
    }
}
