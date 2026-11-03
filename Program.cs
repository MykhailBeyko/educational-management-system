using System.Text;
using systemforstudent.Persistence;
using systemforstudent.Services;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

IDataStorage storage = new InMemoryDataStorage();
StudyService service = new StudyService(storage);

while (true)
{
    Console.WriteLine("\n=== СИСТЕМА УПРАВЛІННЯ НАВЧАЛЬНИМ ПРОЦЕСОМ (Етап 1) ===");
    Console.WriteLine("1. Список студентів");
    Console.WriteLine("2. Список предметів");
    Console.WriteLine("3. Список викладачів");
    Console.WriteLine("4. Призначити предмет студенту");
    Console.WriteLine("5. Список предметів студента");
    Console.WriteLine("6. Додати оцінку студенту");
    Console.WriteLine("7. Список оцінок студента з конкретного предмету");
    Console.WriteLine("8. Редагувати оцінку");
    Console.WriteLine("9. Видалити оцінку");
    Console.WriteLine("10. Всі поточні оцінки в системі");
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
    var students = service.GetAllStudents();
    foreach (var s in students)
    {
        Console.WriteLine(s);
    }
}

static void ShowSubjects(StudyService service)
{
    Console.WriteLine("--- Список предметів ---");
    var subjects = service.GetAllSubjects();
    foreach (var sub in subjects)
    {
        Console.WriteLine(sub);
    }
}

static void ShowTeachers(StudyService service)
{
    Console.WriteLine("--- Список викладачів ---");
    var teachers = service.GetAllTeachers();
    foreach (var t in teachers)
    {
        Console.WriteLine(t);
    }
}

static void AssignSubject(StudyService service)
{
    Console.WriteLine("--- Призначення предмету студенту ---");
    ShowStudents(service);
    Console.Write("Введіть ID студента: ");
    if (!int.TryParse(Console.ReadLine(), out int studentId))
    {
        Console.WriteLine("Некоректний ID.");
        return;
    }

    ShowSubjects(service);
    Console.Write("Введіть ID предмету: ");
    if (!int.TryParse(Console.ReadLine(), out int subjectId))
    {
        Console.WriteLine("Некоректний ID.");
        return;
    }

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
    if (!int.TryParse(Console.ReadLine(), out int studentId))
    {
        Console.WriteLine("Некоректний ID.");
        return;
    }

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

    Console.Write("Введіть літерну оцінку (A, B, C, D, E, F): ");
    string letterGrade = Console.ReadLine()?.Trim().ToUpper() ?? "F";

    var grade = service.AddGrade(studentId, subjectId, teacherId, numericValue, letterGrade);
    if (grade != null)
    {
        Console.WriteLine($"Оцінку успішно додано! ID оцінки: {grade.Id}");
    }
    else
    {
        Console.WriteLine("Помилка при додаванні оцінки. Перевірте введені ID.");
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

    Console.Write("Нова числова оцінка: ");
    if (!int.TryParse(Console.ReadLine(), out int newNumeric)) return;

    Console.Write("Нова літерна оцінка: ");
    string newLetter = Console.ReadLine()?.Trim().ToUpper() ?? "F";

    bool success = service.UpdateGrade(gradeId, newNumeric, newLetter);
    if (success)
    {
        Console.WriteLine("Оцінку успішно оновлено!");
    }
    else
    {
        Console.WriteLine("Оцінку з таким ID не знайдено.");
    }
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
