using systemforstudent.Security;
using systemforstudent.Persistence;
using systemforstudent.Services;
using systemforstudent.Services.Scales;

namespace systemforstudent.Presentation;

public static class AdminConsole
{
    public static void Run(StudyService service, ReportingService reporting, UserSession session)
    {
        session.RequireRole(AppRoles.Admin);
        while (true)
        {
            Console.WriteLine($"\n=== СИСТЕМА УПРАВЛІННЯ НАВЧАЛЬНИМ ПРОЦЕСОМ (Етапи 1–6) ===");
            Console.WriteLine($"Поточна шкала оцінювання: {service.ActiveScale.Name}");
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("1.  Список студентів");
            Console.WriteLine("2.  Список предметів");
            Console.WriteLine("3.  Список викладачів (з посадами)");
            Console.WriteLine("4.  Список кафедр");
            Console.WriteLine("5.  Призначити предмет студенту");
            Console.WriteLine("6.  Список предметів студента");
            Console.WriteLine("7.  Додати оцінку студенту (автовизначення літери + валідація)");
            Console.WriteLine("8.  Список оцінок студента з конкретного предмету");
            Console.WriteLine("9.  Редагувати оцінку");
            Console.WriteLine("10. Видалити оцінку");
            Console.WriteLine("11. Всі поточні оцінки в системі");
            Console.WriteLine("12. Середній бал студента (по предмету / за семестр / загальний)");
            Console.WriteLine("13. Змінити шкалу оцінювання (ECTS / Національна / Власна)");
            Console.WriteLine("14. Звіти та аналітика (Табелі, статистика групи/викладача)");
            Console.WriteLine("15. [Етап 4] Статистика кафедри та ТОП-5 студентів");
            Console.WriteLine("16. [Етап 4] Рейтинг студентів групи за середнім балом");
            Console.WriteLine("17. [Етап 4] Фільтрація студентів (за групою, семестром, балом)");
            Console.WriteLine("18. [Етап 4] Керування кафедрами та посадами викладачів");
            Console.WriteLine("19. [Етап 4] Керування групами та додавання студентів (створити групу / студента)");
            Console.WriteLine("0.  Вихід");
            Console.Write("Оберіть пункт меню: ");
            string? choice = Console.ReadLine();
            if (choice is null)
            {
                return;
            }

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
                    ShowDepartments(service);
                    break;
                case "5":
                    AssignSubject(service);
                    break;
                case "6":
                    ShowStudentSubjects(service);
                    break;
                case "7":
                    AddGrade(service);
                    break;
                case "8":
                    ShowStudentGradesBySubject(service);
                    break;
                case "9":
                    EditGrade(service);
                    break;
                case "10":
                    DeleteGrade(service);
                    break;
                case "11":
                    ShowAllGrades(service);
                    break;
                case "12":
                    ShowAverageScores(service);
                    break;
                case "13":
                    ChangeScale(service);
                    break;
                case "14":
                    ShowReportsMenu(service, reporting);
                    break;
                case "15":
                    ShowDepartmentStats(service);
                    break;
                case "16":
                    ShowGroupRanking(service);
                    break;
                case "17":
                    FilterStudentsMenu(service);
                    break;
                case "18":
                    ManageDepartmentsAndTeachers(service);
                    break;
                case "19":
                    ManageGroupsAndStudents(service);
                    break;
                case "0":
                    Console.WriteLine("Завершення роботи програми.");
                    return;
                default:
                    Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                    break;
            }
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

    static void ShowDepartments(StudyService service)
    {
        Console.WriteLine("--- Список кафедр ---");
        var deps = service.GetAllDepartments();
        if (deps.Count == 0)
        {
            Console.WriteLine("Кафедр ще не зареєстровано.");
        }
        else
        {
            foreach (var d in deps)
            {
                Console.WriteLine(d);
            }
        }
    }

    static void AssignSubject(StudyService service)
    {
        Console.WriteLine("--- Призначення предмету студенту ---");
        ShowStudents(service);
        Console.Write("Введіть ID студента: ");
        if (!int.TryParse(Console.ReadLine(), out int studentId))
        {
            return;
        }

        ShowSubjects(service);
        Console.Write("Введіть ID предмету: ");
        if (!int.TryParse(Console.ReadLine(), out int subjectId))
        {
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
        if (!int.TryParse(Console.ReadLine(), out int studentId))
        {
            return;
        }

        ShowSubjects(service);
        Console.Write("Введіть ID предмету: ");
        if (!int.TryParse(Console.ReadLine(), out int subjectId))
        {
            return;
        }

        ShowTeachers(service);
        Console.Write("Введіть ID викладача: ");
        if (!int.TryParse(Console.ReadLine(), out int teacherId))
        {
            return;
        }

        Console.Write("Введіть числову оцінку (0-100): ");
        if (!int.TryParse(Console.ReadLine(), out int numericValue))
        {
            return;
        }

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
        if (!int.TryParse(Console.ReadLine(), out int studentId))
        {
            return;
        }

        Console.Write("Введіть ID предмету: ");
        if (!int.TryParse(Console.ReadLine(), out int subjectId))
        {
            return;
        }

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
        if (!int.TryParse(Console.ReadLine(), out int gradeId))
        {
            return;
        }

        Console.Write("Нова числова оцінка (0-100): ");
        if (!int.TryParse(Console.ReadLine(), out int newNumeric))
        {
            return;
        }

        var result = service.UpdateGrade(gradeId, newNumeric);
        Console.WriteLine(result.Message);
    }

    static void DeleteGrade(StudyService service)
    {
        Console.WriteLine("--- Видалення оцінки ---");
        ShowAllGrades(service);
        Console.Write("Введіть ID оцінки для видалення: ");
        if (!int.TryParse(Console.ReadLine(), out int gradeId))
        {
            return;
        }

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
        if (!int.TryParse(Console.ReadLine(), out int studentId))
        {
            return;
        }

        Console.WriteLine("1. Середній бал по предмету");
        Console.WriteLine("2. Середній бал за семестр");
        Console.WriteLine("3. Загальний середній бал");
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
                    Console.WriteLine(avg.HasValue ? $"Середній бал: {avg.Value}" : "Оцінок немає.");
                }

                break;
            case "2":
                Console.Write("Введіть номер семестру: ");
                if (int.TryParse(Console.ReadLine(), out int sem))
                {
                    var avg = service.GetStudentSemesterAverage(studentId, sem);
                    Console.WriteLine(avg.HasValue ? $"Середній бал за {sem}-й семестр: {avg.Value}" : "Оцінок немає.");
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
            if (string.IsNullOrEmpty(name))
            {
                name = "Власна шкала";
            }

            var rules = new List<(int MinScore, string Label)>();
            Console.WriteLine("Вводьте пороги (від мінімального балу до літери/оцінки). Натисніть Enter на порожній назві, щоб завершити.");
            while (true)
            {
                Console.Write("Позначка (наприклад A, 5, Зараховано): ");
                string? label = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(label))
                {
                    break;
                }

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

    static void ShowReportsMenu(StudyService service, ReportingService reporting)
    {
        Console.WriteLine("--- МЕНЮ ЗВІТІВ ТА АНАЛІТИКИ ---");
        Console.WriteLine("1. Посеместровий табель успішності студента");
        Console.WriteLine("2. Підсумкова таблиця за всі семестри (за роком і семестром)");
        Console.WriteLine("3. Статистика по академічній групі");
        Console.WriteLine("4. Звіт для викладача за семестр");
        Console.WriteLine("5. Звіт для завідувача кафедри");
        Console.Write("Оберіть тип звіту: ");
        string? rChoice = Console.ReadLine();
        switch (rChoice)
        {
            case "1":
                ShowStudents(service);
                Console.Write("Введіть ID студента: ");
                if (!int.TryParse(Console.ReadLine(), out int sId))
                {
                    return;
                }

                Console.Write("Введіть семестр: ");
                if (!int.TryParse(Console.ReadLine(), out int sem))
                {
                    return;
                }

                reporting.PrintStudentSemesterReport(sId, sem);
                break;
            case "2":
                ShowStudents(service);
                Console.Write("Введіть ID студента: ");
                if (!int.TryParse(Console.ReadLine(), out int stId))
                {
                    return;
                }

                reporting.PrintStudentFullAcademicReport(stId);
                break;
            case "3":
                Console.Write("Введіть шифр групи (наприклад, ІПЗ-21): ");
                string? grp = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(grp))
                {
                    return;
                }

                Console.Write("Введіть семестр: ");
                if (!int.TryParse(Console.ReadLine(), out int grpSem))
                {
                    return;
                }

                reporting.PrintGroupStatistics(grp.Trim(), grpSem);
                break;
            case "4":
                ShowTeachers(service);
                Console.Write("Введіть ID викладача: ");
                if (!int.TryParse(Console.ReadLine(), out int tId))
                {
                    return;
                }

                Console.Write("Введіть семестр: ");
                if (!int.TryParse(Console.ReadLine(), out int tSem))
                {
                    return;
                }

                reporting.PrintTeacherReport(tId, tSem);
                break;
            case "5":
                Console.Write("Введіть назву кафедри (наприклад, Інформаційні технології): ");
                string? dep = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(dep))
                {
                    return;
                }

                reporting.PrintDepartmentReport(dep.Trim());
                break;
        }
    }

    static void ShowDepartmentStats(StudyService service)
    {
        Console.WriteLine("--- Статистика кафедри та ТОП-5 студентів ---");
        ShowDepartments(service);
        Console.Write("Введіть ID кафедри: ");
        if (int.TryParse(Console.ReadLine(), out int depId))
        {
            service.PrintDepartmentAdvancedStats(depId);
        }
    }

    static void ShowGroupRanking(StudyService service)
    {
        Console.WriteLine("--- Рейтинг студентів групи за середнім балом ---");
        Console.Write("Введіть назву групи (наприклад, ІПЗ-21): ");
        string? group = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(group))
        {
            return;
        }

        var ranking = service.GetGroupRanking(group.Trim());
        Console.WriteLine($"\nРЕЙТИНГ ГРУПИ {group.ToUpper()}:");
        if (ranking.Count == 0)
        {
            Console.WriteLine("Студентів у цій групі не знайдено.");
            return;
        }

        int rank = 1;
        foreach (var item in ranking)
        {
            Console.WriteLine($"{rank++,2}. {item.Student.FullName,-32} | Середній бал: {item.AverageScore:F2}");
        }
    }

    static void FilterStudentsMenu(StudyService service)
    {
        Console.WriteLine("--- Фільтрація студентів ---");
        Console.Write("Фільтр за групою (Enter щоб пропустити): ");
        string? group = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(group))
        {
            group = null;
        }

        Console.Write("Фільтр за семестром (Enter щоб пропустити): ");
        string? semInput = Console.ReadLine();
        int? semester = int.TryParse(semInput, out int sVal) ? sVal : null;
        Console.Write("Мінімальний середній бал (наприклад 75, Enter щоб пропустити): ");
        string? minInput = Console.ReadLine();
        double? minGpa = double.TryParse(minInput, out double gVal) ? gVal : null;
        var filtered = service.FilterStudents(group, semester, minGpa);
        Console.WriteLine($"\nЗнайдено студентів: {filtered.Count}");
        if (filtered.Count == 0)
        {
            Console.WriteLine("Жоден студент не відповідає критеріям фільтра.");
        }
        else
        {
            foreach (var st in filtered)
            {
                double gpa = service.GetStudentOverallAverage(st.Id) ?? 0.0;
                Console.WriteLine($" - {st.FullName} ({st.Group}) | Рік вступу: {st.AdmissionYear} | Сер. бал: {gpa:F2}");
            }
        }
    }

    static void ManageDepartmentsAndTeachers(StudyService service)
    {
        Console.WriteLine("--- Керування кафедрами та посадами викладачів ---");
        Console.WriteLine("1. Створити нову кафедру");
        Console.WriteLine("2. Змінити посаду викладача");
        Console.WriteLine("3. Призначити викладача до кафедри");
        Console.Write("Вибір: ");
        string? opt = Console.ReadLine();
        switch (opt)
        {
            case "1":
                Console.Write("Назва кафедри: ");
                string? name = Console.ReadLine()?.Trim();
                Console.Write("Завідувач кафедри: ");
                string? head = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(head))
                {
                    var dep = service.AddDepartment(name, head);
                    Console.WriteLine($"Кафедру успішно створено! ID: {dep.Id}");
                }

                break;
            case "2":
                ShowTeachers(service);
                Console.Write("Введіть ID викладача: ");
                if (int.TryParse(Console.ReadLine(), out int tId))
                {
                    Console.Write("Нова посада (наприклад, Професор, Доцент, Асистент): ");
                    string? pos = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrEmpty(pos) && service.UpdateTeacherPosition(tId, pos))
                    {
                        Console.WriteLine("Посаду викладача успішно оновлено!");
                    }
                    else
                    {
                        Console.WriteLine("Помилка оновлення посади.");
                    }
                }

                break;
            case "3":
                ShowTeachers(service);
                Console.Write("Введіть ID викладача: ");
                if (!int.TryParse(Console.ReadLine(), out int teachId))
                {
                    return;
                }

                ShowDepartments(service);
                Console.Write("Введіть ID кафедри: ");
                if (!int.TryParse(Console.ReadLine(), out int depId))
                {
                    return;
                }

                if (service.AssignTeacherToDepartment(teachId, depId))
                {
                    Console.WriteLine("Викладача успішно закріплено за кафедрою!");
                }
                else
                {
                    Console.WriteLine("Помилка прив'язки.");
                }

                break;
        }
    }

    static void ManageGroupsAndStudents(StudyService service)
    {
        Console.WriteLine("--- Керування групами та студентами ---");
        Console.WriteLine("1. Переглянути всі академічні групи та студентів");
        Console.WriteLine("2. Створити нову академічну групу");
        Console.WriteLine("3. Додати нового студента (і записати в групу)");
        Console.WriteLine("4. Перевести студента в іншу групу");
        Console.Write("Вибір: ");
        string? opt = Console.ReadLine();
        switch (opt)
        {
            case "1":
                var groups = service.GetAllGroups();
                if (groups.Count == 0)
                {
                    Console.WriteLine("Груп ще немає.");
                    return;
                }

                foreach (var grp in groups)
                {
                    var students = service.GetStudentsByGroup(grp);
                    Console.WriteLine($"\nГрупа \"{grp}\" ({students.Count} студентів):");
                    if (students.Count == 0)
                    {
                        Console.WriteLine("  (порожня група)");
                    }
                    else
                    {
                        foreach (var st in students)
                        {
                            Console.WriteLine($"  - [#{st.Id}] {st.FullName} (Рік вступу: {st.AdmissionYear}, Предметів: {st.Subjects.Count})");
                        }
                    }
                }

                break;
            case "2":
                Console.Write("Введіть назву нової групи (наприклад, КБ-23): ");
                string? newGrp = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(newGrp))
                {
                    Console.WriteLine("Назва не може бути порожньою.");
                    return;
                }

                if (service.AddGroup(newGrp))
                {
                    Console.WriteLine($"Групу \"{newGrp}\" успішно створено та збережено в data.json!");
                }
                else
                {
                    Console.WriteLine("Така група вже існує.");
                }

                break;
            case "3":
                Console.Write("Введіть ПІБ студента: ");
                string? name = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    Console.WriteLine("ПІБ не може бути порожнім.");
                    return;
                }

                var currentGroups = service.GetAllGroups();
                Console.WriteLine("Наявні групи: " + string.Join(", ", currentGroups));
                Console.Write("Введіть назву групи (зі списку або нову): ");
                string? studentGrp = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(studentGrp))
                {
                    Console.WriteLine("Група не може бути порожньою.");
                    return;
                }

                Console.Write("Введіть рік вступу (наприклад, 2024): ");
                if (!int.TryParse(Console.ReadLine(), out int year))
                {
                    Console.WriteLine("Некоректний рік.");
                    return;
                }

                var newStudent = service.AddStudent(name, studentGrp, year);
                Console.WriteLine($"\nСтудента [#{newStudent.Id}] {newStudent.FullName} успішно записано в групу \"{newStudent.Group}\" та збережено в data.json!");
                break;
            case "4":
                ShowStudents(service);
                Console.Write("Введіть ID студента: ");
                if (!int.TryParse(Console.ReadLine(), out int sId))
                {
                    return;
                }

                var existingStudent = service.GetStudentById(sId);
                if (existingStudent == null)
                {
                    Console.WriteLine("Студента не знайдено.");
                    return;
                }

                Console.WriteLine($"Поточна група студента: {existingStudent.Group}");
                Console.WriteLine("Наявні групи: " + string.Join(", ", service.GetAllGroups()));
                Console.Write("Введіть нову групу: ");
                string? targetGroup = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(targetGroup) && service.ChangeStudentGroup(sId, targetGroup))
                {
                    Console.WriteLine($"Студента успішно переведено в групу \"{targetGroup}\" та оновлено в data.json!");
                }
                else
                {
                    Console.WriteLine("Помилка зміни групи.");
                }

                break;
        }
    }
}