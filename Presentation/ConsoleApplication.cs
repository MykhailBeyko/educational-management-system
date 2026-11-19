using System.Text;
using systemforstudent.Persistence;
using systemforstudent.Security;
using systemforstudent.Services;

namespace systemforstudent.Presentation;

public sealed class ConsoleApplication
{
    private readonly EfDataStorage _storage;
    private readonly AccountService _accounts;

    public ConsoleApplication(EfDataStorage storage, AccountService accounts)
    {
        _storage = storage;
        _accounts = accounts;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine("\n=== Система управління навчальним процесом — етапи 1–6 ===");
            Console.WriteLine(
                "1. Увійти\n" +
                "0. Вихід");
            var choice = Console.ReadLine();
            if (choice is null or "0")
            {
                return;
            }

            if (choice != "1")
            {
                continue;
            }

            try
            {
                var username = Read("Логін: ");
                var password = ReadPassword("Пароль: ");
                var session = await _accounts.LoginAsync(username, password);
                await RunSessionAsync(session);
            }
            catch (EndOfStreamException)
            {
                return;
            }
            catch (Exception ex) when (IsUserError(ex))
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private async Task RunSessionAsync(UserSession session)
    {
        var study = new RoleStudyService(_storage, session);
        while (true)
        {
            Console.WriteLine($"\nКористувач: {session.Username} | Роль: {session.Role}");
            switch (session.Role)
            {
                case AppRoles.Admin:
                    Console.WriteLine(
                        "1. Навчальні дані та звіти (етапи 1–4)\n" +
                        "2. Керування акаунтами API");
                    break;
                case AppRoles.Student:
                    Console.WriteLine(
                        "1. Мої предмети та оцінки\n" +
                        "2. Табель за семестр\n" +
                        "3. Підсумковий табель\n" +
                        "4. Рейтинг моєї групи\n" +
                        "5. Завантажити звіт про успішність");
                    break;
                case AppRoles.Teacher:
                    Console.WriteLine(
                        "1. Мої предмети та студенти\n" +
                        "2. Мої оцінки\n" +
                        "3. Виставити оцінку\n" +
                        "4. Редагувати оцінку\n" +
                        "5. Видалити оцінку\n" +
                        "6. Звіт за семестр");
                    break;
                case AppRoles.DepartmentHead:
                    Console.WriteLine(
                        "1. Статистика моєї кафедри та ТОП-5\n" +
                        "2. Зберегти звіт кафедри");
                    break;
            }

            Console.WriteLine(
                "9. Змінити свій пароль\n" +
                "0. Вийти з акаунта");
            var choice = Console.ReadLine();
            if (choice is null or "0")
            {
                return;
            }

            try
            {
                if (choice == "9")
                {
                    var oldPassword = ReadPassword("Старий пароль: ");
                    var newPassword = ReadPassword("Новий пароль: ");
                    await _accounts.ChangePasswordAsync(session, oldPassword, newPassword);
                    Console.WriteLine("Пароль змінено. Увійдіть повторно.");
                    return;
                }

                switch (session.Role)
                {
                    case AppRoles.Admin:
                        if (choice == "1")
                        {
                            AdminConsole.Run(new StudyService(_storage), new ReportingService(_storage), session);
                        }
                        else if (choice == "2")
                        {
                            await ManageAccountsAsync(session);
                        }

                        break;
                    case AppRoles.Student:
                        RunStudent(study, choice);
                        break;
                    case AppRoles.Teacher:
                        RunTeacher(study, choice);
                        break;
                    case AppRoles.DepartmentHead:
                        if (choice == "1")
                        {
                            Console.Write(study.DepartmentReport());
                        }
                        else if (choice == "2")
                        {
                            SaveReport(study.DepartmentReport(), $"department-{session.EntityId}");
                        }

                        break;
                }
            }
            catch (Exception ex) when (IsUserError(ex))
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private static void RunStudent(RoleStudyService study, string choice)
    {
        switch (choice)
        {
            case "1":
                Console.WriteLine(study.CurrentStudent);
                foreach (var subject in study.CurrentStudent.Subjects)
                {
                    Console.WriteLine(subject);
                }

                foreach (var grade in study.GetMyGrades())
                {
                    Console.WriteLine(grade);
                }

                break;
            case "2":
                Console.Write(study.StudentReport(ReadInt("Семестр: ")));
                break;
            case "3":
                Console.Write(study.StudentReport());
                break;
            case "4":
                int place = 1;
                foreach (var row in study.GetMyGroupRanking())
                {
                    Console.WriteLine($"{place++}. {row.Student.FullName}: {row.AverageScore:F2}");
                }

                break;
            case "5":
                SaveReport(study.StudentReport(), $"student-{study.CurrentStudent.Id}");
                break;
        }
    }

    private static void RunTeacher(RoleStudyService study, string choice)
    {
        switch (choice)
        {
            case "1":
                foreach (var subject in study.CurrentTeacher.Subjects)
                {
                    Console.WriteLine(subject);
                }

                foreach (var student in study.GetMyStudents())
                {
                    Console.WriteLine($"[#{student.Id}] {student.FullName} | {student.Group}");
                }

                break;
            case "2":
                foreach (var grade in study.GetTeacherGrades())
                {
                    Console.WriteLine(grade);
                }

                break;
            case "3":
                int studentId = ReadInt("ID студента: ");
                int subjectId = ReadInt("ID предмета: ");
                int numericValue = ReadInt("Оцінка (0–100): ");

                var result = study.AddGrade(studentId, subjectId, numericValue);
                Console.WriteLine(result.Message);
                break;
            case "4":
                int gradeId = ReadInt("ID оцінки: ");
                int newNumericValue = ReadInt("Нова оцінка: ");

                var updateResult = study.UpdateGrade(gradeId, newNumericValue);
                Console.WriteLine(updateResult.Message);
                break;
            case "5":
                int gradeToDeleteId = ReadInt("ID оцінки: ");
                bool deleted = study.DeleteGrade(gradeToDeleteId);

                Console.WriteLine(deleted ? "Оцінку видалено." : "Оцінку не знайдено.");
                break;
            case "6":
                Console.Write(study.TeacherReport(ReadInt("Семестр: ")));
                break;
        }
    }

    private async Task ManageAccountsAsync(UserSession admin)
    {
        admin.RequireRole(AppRoles.Admin);
        Console.WriteLine(
            "1. Список користувачів\n" +
            "2. Створити акаунт\n" +
            "3. Змінити роль\n" +
            "4. Прив'язати наявний акаунт\n" +
            "0. Назад");
        var choice = Console.ReadLine();
        if (choice == "1")
        {
            foreach (var user in await _accounts.GetUsersAsync(admin))
            {
                Console.WriteLine($"{user.UserName} | {user.Role}");
            }

            return;
        }

        if (choice is not ("2" or "3" or "4"))
        {
            return;
        }

        Console.WriteLine("Студенти:");
        foreach (var student in _storage.Students)
        {
            Console.WriteLine(student);
        }

        Console.WriteLine("Викладачі:");
        foreach (var teacher in _storage.Teachers)
        {
            Console.WriteLine(teacher);
        }

        Console.WriteLine("Кафедри:");
        foreach (var department in _storage.Departments)
        {
            Console.WriteLine(department);
        }

        var username = Read("Логін акаунта: ").Trim();
        if (choice == "4")
        {
            await _accounts.BindAsync(admin, username, ReadInt("ID профілю (для Admin — 0): "));
            Console.WriteLine("Акаунт прив'язано.");
            return;
        }

        var role = Read("Роль (Student / Teacher / DepartmentHead / Admin): ").Trim();
        var entityId = ReadInt("ID студента / викладача / кафедри (для Admin — 0): ");
        if (choice == "2")
        {
            await _accounts.RegisterAsync(admin, username, ReadPassword("Пароль нового акаунта: "), role, entityId);
        }
        else
        {
            await _accounts.ChangeRoleAsync(admin, username, role, entityId);
        }

        Console.WriteLine("Акаунт збережено. Нова роль діє після повторного входу.");
    }

    public static string Read(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? throw new EndOfStreamException();
    }

    private static int ReadInt(string prompt)
    {
        if (!int.TryParse(Read(prompt), out int value))
        {
            throw new ArgumentException("Введіть ціле число.");
        }

        return value;
    }

    public static string ReadPassword(string prompt)
    {
        Console.Write(prompt);
        if (Console.IsInputRedirected)
        {
            return Console.ReadLine() ?? throw new EndOfStreamException();
        }

        var password = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return password.ToString();
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Length--;
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
            }
        }
    }

    private static void SaveReport(string content, string prefix)
    {
        var directory = Path.GetFullPath("reports");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{prefix}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.txt");
        File.WriteAllText(path, content, new UTF8Encoding(false));
        Console.WriteLine($"Звіт збережено: {path}");
    }

    private static bool IsUserError(Exception ex)
    {
        return ex is HttpRequestException
            or TaskCanceledException
            or UnauthorizedAccessException
            or ArgumentException
            or InvalidOperationException
            or IOException
            or System.Text.Json.JsonException
            or Microsoft.Data.Sqlite.SqliteException;
    }
}
