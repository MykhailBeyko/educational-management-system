using Microsoft.EntityFrameworkCore;
using systemforstudent.Models;

namespace systemforstudent.Persistence;

public class EfDataStorage : IDataStorage, IDisposable
{
    private readonly UniversityDbContext _context;

    public List<Student> Students { get; private set; } = new();
    public List<Teacher> Teachers { get; private set; } = new();
    public List<Subject> Subjects { get; private set; } = new();
    public List<Grade> Grades { get; private set; } = new();
    public List<Department> Departments { get; private set; } = new();
    public List<string> Groups { get; private set; } = new();

    public EfDataStorage(string connectionString = "Data Source=university.db", string? legacyJsonPath = "data.json")
    {
        _context = new UniversityDbContext(connectionString);
        bool created = _context.Database.EnsureCreated();
        // Додаємо прив'язки акаунтів до наявної бази без видалення навчальних даних.
        _context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "AccountLinks" (
                "Username" TEXT COLLATE NOCASE NOT NULL PRIMARY KEY,
                "Role" TEXT NOT NULL,
                "EntityId" INTEGER NOT NULL
            );
            """);
        Load();
        if (created)
        {
            IDataStorage initialData;

            if (legacyJsonPath != null && File.Exists(legacyJsonPath))
            {
                initialData = new JsonDataStorage(legacyJsonPath);
            }
            else
            {
                initialData = new InMemoryDataStorage();
            }

            CopyFrom(initialData);
            Save();
        }
    }

    public void Save()
    {
        SyncCollection(_context.Subjects, Subjects);
        SyncCollection(_context.Teachers, Teachers);
        SyncCollection(_context.Students, Students);
        SyncCollection(_context.Grades, Grades);
        SyncCollection(_context.Departments, Departments);

        var dbGroups = _context.AcademicGroups.ToList();
        foreach (var grpName in Groups)
        {
            if (!dbGroups.Any(g => g.Name.Equals(grpName, StringComparison.OrdinalIgnoreCase)))
            {
                _context.AcademicGroups.Add(new AcademicGroup { Name = grpName });
            }
        }

        _context.AcademicGroups.RemoveRange(dbGroups.Where(g => !Groups.Contains(g.Name, StringComparer.OrdinalIgnoreCase)));
        _context.SaveChanges();
    }

    public void Load()
    {
        _context.ChangeTracker.Clear();
        var dbStudents = _context.Students.Include(s => s.Subjects).ToList();
        Subjects = _context.Subjects.ToList();
        Teachers = _context.Teachers.Include(t => t.Subjects).ToList();
        Students = dbStudents;
        Grades = _context.Grades
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .Include(g => g.Teacher)
            .ToList();
        Departments = _context.Departments.Include(d => d.Teachers).ToList();
        Groups = _context.AcademicGroups.Select(g => g.Name).ToList();
        // Забезпечуємо наявність груп зі студентів
        foreach (var grp in Students.Select(s => s.Group).Distinct())
        {
            if (!string.IsNullOrWhiteSpace(grp) && !Groups.Contains(grp))
            {
                Groups.Add(grp);
            }
        }
    }

    public AccountLink? GetAccountLink(string username)
    {
        return _context.AccountLinks.SingleOrDefault(a => a.Username == username);
    }

    public void SetAccountLink(string username, string role, int entityId)
    {
        var link = GetAccountLink(username);
        if (link == null)
        {
            link = new AccountLink
            {
                Username = username.Trim()
            };
            _context.AccountLinks.Add(link);
        }

        link.Role = role;
        link.EntityId = entityId;
        _context.SaveChanges();
    }

    private void SyncCollection<T>(DbSet<T> dbSet, List<T> localList) where T : class
    {
        var dbItems = dbSet.ToList();
        // Додаємо нові елементи, яких ще немає в базі даних
        foreach (var item in localList)
        {
            if (!dbItems.Contains(item))
            {
                dbSet.Add(item);
            }
        }
        // Видаляємо з бази елементи, видалені з локального списку
        foreach (var dbItem in dbItems)
        {
            if (!localList.Contains(dbItem))
            {
                dbSet.Remove(dbItem);
            }
        }
    }

    private void CopyFrom(IDataStorage source)
    {
        Subjects = source.Subjects;
        Teachers = source.Teachers;
        Students = source.Students;
        Grades = source.Grades;
        Departments = source.Departments;
        Groups = source.Groups;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
