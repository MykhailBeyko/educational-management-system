using System.Text.Json;
using systemforstudent.Integration;
using systemforstudent.Models;
using systemforstudent.Persistence;
using systemforstudent.Security;
using systemforstudent.Services;
using systemforstudent.Services.Scales;

System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

int checks = 0;

void Check(bool success, string message)
{
    if (!success)
    {
        throw new Exception(message);
    }

    checks++;
}

void Denied(Action action, string message)
{
    try
    {
        action();
    }
    catch (UnauthorizedAccessException)
    {
        checks++;
        return;
    }

    throw new Exception(message);
}

async Task HttpFails(Func<Task> action, string message)
{
    try
    {
        await action();
    }
    catch (HttpRequestException)
    {
        checks++;
        return;
    }

    throw new Exception(message);
}

string Capture(Action<TextWriter> print)
{
    using var output = new StringWriter();
    print(output);
    return output.ToString();
}

async Task<string> RunConsole(string database, string apiUrl, string input, string workingDirectory)
{
    var start = new System.Diagnostics.ProcessStartInfo("dotnet")
    {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
        WorkingDirectory = workingDirectory,
        StandardInputEncoding = new System.Text.UTF8Encoding(false),
        StandardOutputEncoding = System.Text.Encoding.UTF8,
        StandardErrorEncoding = System.Text.Encoding.UTF8
    };
    start.ArgumentList.Add(typeof(StudyService).Assembly.Location);
    start.Environment["STUDY_DB"] = database;
    start.Environment["SECURITY_API_URL"] = apiUrl;
    using var process = System.Diagnostics.Process.Start(start)!;
    var output = process.StandardOutput.ReadToEndAsync();
    var errors = process.StandardError.ReadToEndAsync();
    await process.StandardInput.WriteAsync(input);
    process.StandardInput.Close();
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    try
    {
        await process.WaitForExitAsync(timeout.Token);
    }
    catch (OperationCanceledException)
    {
        process.Kill(entireProcessTree: true);
        throw;
    }

    Check(
        process.ExitCode == 0,
        "Console exits successfully: " + await errors);
    return await output;
}

var storage = new InMemoryDataStorage();

var study = new StudyService(storage);

Check(
    storage.Students.Count == 7 && storage.Grades.Count == 19,
    "Stage 1: seed entities");

Check(
    !study.AddGrade(1, 1, 1, 101).Success && !study.AddGrade(1, 1, 1, -1).Success,
    "Stage 2: grade range");

Check(
    !study.AddGrade(1, 6, 3, 80).Success,
    "Stage 2: enrollment validation");

Check(
    !study.AddGrade(1, 1, 2, 80).Success,
    "Stage 1: teacher subject validation");

Check(
    study.AssignSubjectToStudent(1, 6) && !study.AssignSubjectToStudent(1, 6),
    "Stage 1: assignment/duplicate");

var added = study.AddGrade(1, 6, 3, 90);

Check(
    added.Success && added.Grade?.LetterGrade == "A",
    "Stages 1–2: numeric to letter");

Check(
    study.UpdateGrade(added.Grade!.Id, 59).Success && added.Grade.LetterGrade == "Fx",
    "Stage 1: update");

Check(
    study.DeleteGrade(added.Grade.Id) && !study.DeleteGrade(added.Grade.Id),
    "Stage 1: delete");

Check(
    study.GetStudentOverallAverage(1) == 91.25
    && study.GetStudentSemesterAverage(1, 2) == 92.5,
    "Stage 2: averages");

Check(
    study.GetStudentSubjectAverage(1, 1) == 95
    && study.GetStudentSubjectAverage(7, 1) == null,
    "Stage 2: subject/empty averages");

var scale = new EctsGradeScale();

var gradeBoundaries = new[]
{
    (0, "F"),
    (34, "F"),
    (35, "Fx"),
    (59, "Fx"),
    (60, "E"),
    (63, "E"),
    (64, "D"),
    (73, "D"),
    (74, "C"),
    (81, "C"),
    (82, "B"),
    (89, "B"),
    (90, "A"),
    (100, "A")
};

foreach (var (score, letter) in gradeBoundaries)
{
    Check(
        scale.ConvertToLetter(score) == letter,
        $"Stage 2: ECTS boundary {score}");
}

Check(
    new CustomGradeScale("Pass", [(60, "Pass"), (0, "Fail")]).ConvertToLetter(70) == "Pass",
    "Stage 2: custom scale");

Check(
    new TraditionalGradeScale().ConvertToLetter(95) == "Відмінно",
    "Stage 2: traditional scale");

var report = Capture(w => new ReportingService(storage, w).PrintStudentSemesterReport(1, 2));

Check(
    report.Contains("92.5") && report.Contains(storage.Subjects[0].Name),
    "Stage 3: semester report");

report = Capture(w => new ReportingService(storage, w).PrintStudentFullAcademicReport(1));

Check(
    report.Contains("2023") && report.Contains("2024") && report.Contains("91.25"),
    "Stage 3: all years/semesters");

report = Capture(w => new ReportingService(storage, w).PrintGroupStatistics("ІПЗ-21", 3));

Check(
    report.Contains("не склали предмет") && report.Contains("48"),
    "Stage 3: failing students");

report = Capture(w => new ReportingService(storage, w).PrintTeacherReport(1, 3));

Check(
    report.Contains("Кількість оцінок за семестр: 3") && report.Contains("73.67"),
    "Stage 3: teacher report");

report = Capture(w => new ReportingService(storage, w).PrintDepartmentReport(storage.Departments[0].Name));

Check(
    report.Contains("Кількість викладачів: 2")
    && report.Contains("Кількість предметів: 4"),
    "Stage 3: department report");

Check(
    study.GetGroupRanking("ІПЗ-21")[0].Student.Id == 4,
    "Stage 4: ranking");

Check(
    study.FilterStudents("ІПЗ-21", 2, 90)
        .Select(s => s.Id)
        .SequenceEqual(new[] { 1, 4 }),
    "Stage 4: combined filters");

Check(
    study.UpdateTeacherPosition(1, "Професор")
    && storage.Teachers[0].Position == "Професор",
    "Stage 4: position");

Check(
    study.AssignTeacherToDepartment(1, 2)
    && !storage.Departments[0].Teachers.Any(t => t.Id == 1)
    && storage.Departments[1].Teachers.Any(t => t.Id == 1),
    "Stage 4: transfer removes old membership");

var studentSession = new UserSession("student", "token", AppRoles.Student, 1);

var teacherSession = new UserSession("teacher", "token", AppRoles.Teacher, 1);

var headSession = new UserSession("head", "token", AppRoles.DepartmentHead, 1);

var studentView = new RoleStudyService(storage, studentSession);

var teacherView = new RoleStudyService(storage, teacherSession);

Check(
    studentView.GetMyGrades().All(g => g.Student.Id == 1),
    "Stage 6: own grades");

Check(
    studentView.GetMyGroupRanking().All(r => r.Student.Group == "ІПЗ-21"),
    "Stage 6: own group");

Check(
    !studentView.StudentReport().Contains(storage.Students[1].FullName),
    "Stage 6: report privacy");

Denied(
    () => studentView.AddGrade(1, 1, 90),
    "Student cannot add grades");

Denied(
    () => studentView.UpdateGrade(1, 20),
    "Student cannot edit grades");

Denied(
    () => studentView.DeleteGrade(1),
    "Student cannot delete grades");

Denied(
    () => studentView.GetTeacherGrades(),
    "Student cannot read teacher grades");

Denied(
    () => studentView.DepartmentReport(),
    "Student cannot read department report");

Denied(
    () => teacherView.GetMyGrades(),
    "Teacher cannot use student mode");

Denied(
    () => teacherView.AddGrade(1, 2, 90),
    "Teacher cannot grade another subject");

Denied(
    () => teacherView.UpdateGrade(2, 90),
    "Teacher cannot edit another teacher's grade");

Denied(
    () => teacherView.DeleteGrade(2),
    "Teacher cannot delete another teacher's grade");

Check(
    teacherView.GetTeacherGrades().All(g => g.Teacher.Id == 1),
    "Stage 6: own teacher grades");

Check(
    teacherView.GetMyStudents().All(s => s.Subjects.Any(sub => sub.Id is 1 or 3)),
    "Stage 6: students of own subjects");

Check(
    teacherView.AddGrade(1, 1, 91).Success && teacherView.UpdateGrade(1, 93).Success,
    "Stage 6: teacher can grade/edit");

var headView = new RoleStudyService(storage, headSession);

Check(
    headView.DepartmentReport().Contains(storage.Departments[0].Name),
    "Stage 6: head report");

Denied(
    () => headView.AddGrade(1, 1, 90),
    "Head cannot grade");

string testDirectory = Path.Combine(Path.GetTempPath(), "study-checks-" + Guid.NewGuid().ToString("N"));

Directory.CreateDirectory(testDirectory);

try
{
    string database = $"Data Source={Path.Combine(testDirectory, "checks.db")};Pooling=False";
    int newGradeId;
    using (var ef = new EfDataStorage(database, null))
    {
        var service = new StudyService(ef);
        Check(
            ef.Grades.Count == 19,
            "Stage 5: first SQLite seed");
        newGradeId = service.AddGrade(1, 1, 1, 77).Grade!.Id;
        service.UpdateGrade(newGradeId, 81);
        service.AssignSubjectToStudent(7, 1);
        service.AddGroup("TEST");
        service.AssignTeacherToDepartment(1, 2);
        ef.SetAccountLink("test", AppRoles.Student, 1);
    }

    using (var ef = new EfDataStorage(database, null))
    {
        Check(
            ef.Grades.Single(g => g.Id == newGradeId).NumericValue == 81,
            "Stage 5: update survives restart");
        Check(
            ef.Students.Single(s => s.Id == 7).Subjects.Any(s => s.Id == 1)
            && ef.Groups.Contains("TEST"),
            "Stage 5: relations/groups survive restart");
        Check(
            ReferenceEquals(ef.Grades[0].Student, ef.Students[0]),
            "Stage 5: navigation identity");
        Check(
            ef.GetAccountLink("TEST")?.EntityId == 1,
            "Stage 5: case-insensitive account link persists");
        Check(
            !ef.Departments[0].Teachers.Any(t => t.Id == 1)
            && ef.Departments[1].Teachers.Any(t => t.Id == 1),
            "Stage 5: teacher transfer persists");
        new StudyService(ef).DeleteGrade(newGradeId);
        ef.Load();
        Check(
            ef.Grades.Count == 19,
            "Stage 5: deletion survives reload");
    }

    string jsonPath = Path.Combine(testDirectory, "legacy.json");
    var jsonStorage = new JsonDataStorage(jsonPath);
    new StudyService(jsonStorage).UpdateTeacherPosition(1, "Legacy position");
    jsonStorage = new JsonDataStorage(jsonPath);
    Check(
        ReferenceEquals(jsonStorage.Grades[0].Student, jsonStorage.Students[0]),
        "Stages 1–2: JSON restores object references");
    using (var imported = new EfDataStorage($"Data Source={Path.Combine(testDirectory, "imported.db")};Pooling=False", jsonPath))
    {
        Check(
            imported.Teachers[0].Position == "Legacy position" && imported.Grades.Count == 19,
            "Stage 5: legacy JSON import");
    }

    using (var empty = new EfDataStorage(database, null))
    {
        empty.Grades.Clear();
        empty.Students.Clear();
        empty.Save();
    }

    using (var empty = new EfDataStorage(database, null))
    {
        Check(
            empty.Students.Count == 0 && empty.Grades.Count == 0,
            "Stage 5: empty database is not reseeded");
    }

    if (args.Length > 0)
    {
        string baseUrl = args[0];
        using var http = new HttpClient();
        using var spec = JsonDocument.Parse(await http.GetStringAsync(baseUrl + "/swagger/v1/swagger.json"));
        Check(
            spec.RootElement.GetProperty("paths").EnumerateObject().Count() == 5,
            "Stage 5: live Swagger endpoints");
        using var ef = new EfDataStorage($"Data Source={Path.Combine(testDirectory, "api.db")};Pooling=False", null);
        using var api = new AuthApiClient
        {
            BaseUrl = baseUrl
        };
        var accounts = new AccountService(api, ef);
        var admin = await accounts.LoginAsync("admin", "Admin_password1");
        Check(
            admin.Role == AppRoles.Admin,
            "Stage 5: actual admin login");
        Check(
            (await accounts.GetUsersAsync(admin)).Any(u => u.UserName == "admin"),
            "Stage 5: actual user list handles upstream wrapper");
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var studentName = "checkstudent" + suffix;
        var teacherName = "checkteacher" + suffix;
        var headName = "checkhead" + suffix;
        await accounts.RegisterAsync(admin, studentName, "Initial_password1", AppRoles.Student, 1);
        await accounts.RegisterAsync(admin, teacherName, "Initial_password1", AppRoles.Teacher, 1);
        await accounts.RegisterAsync(admin, headName, "Initial_password1", AppRoles.DepartmentHead, 1);
        var student = await accounts.LoginAsync(studentName, "Initial_password1");
        var teacher = await accounts.LoginAsync(teacherName, "Initial_password1");
        var head = await accounts.LoginAsync(headName, "Initial_password1");
        Check(
            student.EntityId == 1 && teacher.Role == AppRoles.Teacher
            && head.Role == AppRoles.DepartmentHead,
            "Stage 6: real logins/profile bindings for all three roles");
        var consoleDb = $"Data Source={Path.Combine(testDirectory, "api.db")};Pooling=False";
        var consoleOutput = await RunConsole(
            consoleDb,
            baseUrl,
            $"1\n{studentName}\nInitial_password1\n1\n2\n2\n3\n4\n5\n0\n0\n",
            testDirectory);
        Check(
            consoleOutput.Contains("Рейтинг моєї групи")
            && !consoleOutput.Contains("Керування акаунтами API"),
            "Stage 6: actual student menu is restricted");
        Check(
            Directory.GetFiles(Path.Combine(testDirectory, "reports"), "student-1-*.txt").Length == 1,
            "Stage 6: report download creates a file");
        var exported = File.ReadAllText(Directory.GetFiles(Path.Combine(testDirectory, "reports"))[0]);
        Check(
            exported.Contains(ef.Students[0].FullName)
            && !exported.Contains(ef.Students[1].FullName),
            "Stage 6: exported report only contains own records");
        consoleOutput = await RunConsole(
            consoleDb,
            baseUrl,
            $"1\n{teacherName}\nInitial_password1\n1\n2\n3\n1\n1\n88\n4\n1\n90\n6\n2\n0\n0\n",
            testDirectory);
        Check(
            consoleOutput.Contains("Оцінку успішно виставлено")
            && consoleOutput.Contains("Оцінку оновлено"),
            "Stage 6: actual teacher console grading");
        consoleOutput = await RunConsole(consoleDb, baseUrl, $"1\n{headName}\nInitial_password1\n1\n2\n0\n0\n", testDirectory);
        Check(
            consoleOutput.Contains("ТОП-5")
            && Directory.GetFiles(Path.Combine(testDirectory, "reports"), "department-1-*.txt").Length == 1,
            "Stage 6: actual head console and export");
        consoleOutput = await RunConsole(consoleDb, baseUrl, "1\nadmin\nAdmin_password1\n1\n1\n0\n2\n1\n0\n0\n", testDirectory);
        Check(
            consoleOutput.Contains("Список студентів") && consoleOutput.Contains(studentName),
            "Stages 1–6: admin retains old menu and lists API accounts");
        Denied(
            () => accounts.GetUsersAsync(student).GetAwaiter().GetResult(),
            "Stage 6: student cannot administer accounts");
        await HttpFails(
            async () =>
            {
                await api.GetUsersAsync(student.Token);
            },
            "API must reject student token for user list");
        await HttpFails(
            async () =>
            {
                await api.RegisterUserAsync("forbidden" + suffix, "pass", AppRoles.Admin, student.Token);
            },
            "API must reject student token for registration");
        await HttpFails(
            async () =>
            {
                await api.SetUserRoleAsync(studentName, AppRoles.Admin, student.Token);
            },
            "API must reject student token for role update");
        await accounts.ChangePasswordAsync(student, "Initial_password1", "Changed_password2");
        await HttpFails(
            async () =>
            {
                await api.LoginAsync(studentName, "Initial_password1");
            },
            "Old password should no longer work");
        Check(
            (await accounts.LoginAsync(studentName, "Changed_password2")).Role == AppRoles.Student,
            "Stage 5: user-token password change");
        await accounts.ChangeRoleAsync(admin, studentName, AppRoles.Teacher, 2);
        Check(
            (await accounts.LoginAsync(studentName, "Changed_password2")).EntityId == 2,
            "Stage 5: admin role update and new login");
        await accounts.BindAsync(admin, teacherName, 2);
        Check(
            (await accounts.LoginAsync(teacherName, "Initial_password1")).EntityId == 2,
            "Stage 6: rebind existing API account");
        await api.RegisterUserAsync("unbound" + suffix, "pass", AppRoles.Student, admin.Token);
        try
        {
            await accounts.LoginAsync("unbound" + suffix, "pass");
            throw new Exception("Unbound login should fail");
        }
        catch (UnauthorizedAccessException)
        {
            checks++;
        }

        Console.WriteLine("Live API integration passed (isolated test accounts created in local Security API).");
    }

    Console.WriteLine($"PASS: {checks} checks for stages 1–6.");
}
finally
{
    Directory.Delete(testDirectory, recursive: true);
}
