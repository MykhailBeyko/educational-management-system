using System.Text.Encodings.Web;
using System.Text.Json;
using systemforstudent.Models;

namespace systemforstudent.Persistence;

public class JsonDataStorage : IDataStorage
{
    private readonly string _filePath;

    public List<Student> Students { get; private set; } = new();
    public List<Teacher> Teachers { get; private set; } = new();
    public List<Subject> Subjects { get; private set; } = new();
    public List<Grade> Grades { get; private set; } = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public JsonDataStorage(string filePath = "data.json")
    {
        _filePath = filePath;
        Load();
    }

    public void Save()
    {
        var container = new StorageContainer
        {
            Students = this.Students,
            Teachers = this.Teachers,
            Subjects = this.Subjects,
            Grades = this.Grades
        };

        string json = JsonSerializer.Serialize(container, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    public void Load()
    {
        if (!File.Exists(_filePath))
        {
            SeedInitialData();
            Save();
            return;
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            var container = JsonSerializer.Deserialize<StorageContainer>(json, _jsonOptions);
            if (container != null)
            {
                Students = container.Students ?? new List<Student>();
                Teachers = container.Teachers ?? new List<Teacher>();
                Subjects = container.Subjects ?? new List<Subject>();
                Grades = container.Grades ?? new List<Grade>();
            }
        }
        catch
        {
            SeedInitialData();
            Save();
        }
    }

    private void SeedInitialData()
    {
        var s1 = new Subject(1, "Об'єктно-орієнтоване програмування", 2, 5);
        var s2 = new Subject(2, "Вища математика", 1, 4);
        var s3 = new Subject(3, "Бази даних", 3, 5);

        Subjects.AddRange(new[] { s1, s2, s3 });

        var t1 = new Teacher(1, "Коваленко Олександр Сергійович", "Інформаційні технології");
        t1.Subjects.Add(s1);
        t1.Subjects.Add(s3);

        var t2 = new Teacher(2, "Мельник Ірина Петрівна", "Вища математика");
        t2.Subjects.Add(s2);

        Teachers.AddRange(new[] { t1, t2 });

        var st1 = new Student(1, "Іваненко Петро Олексійович", "ІПЗ-21", 2023);
        st1.Subjects.Add(s1);
        st1.Subjects.Add(s2);

        var st2 = new Student(2, "Шевченко Анна Михайлівна", "ІПЗ-21", 2023);
        st2.Subjects.Add(s1);
        st2.Subjects.Add(s3);

        var st3 = new Student(3, "Бондаренко Данило Ігорович", "КН-22", 2024);
        st3.Subjects.Add(s2);

        Students.AddRange(new[] { st1, st2, st3 });

        Grades.Add(new Grade(1, st1, s1, t1, 95, "A", DateTime.Now.AddDays(-10)));
        Grades.Add(new Grade(2, st1, s2, t2, 82, "B", DateTime.Now.AddDays(-5)));
        Grades.Add(new Grade(3, st2, s1, t1, 74, "C", DateTime.Now.AddDays(-2)));
    }

    private class StorageContainer
    {
        public List<Student> Students { get; set; } = new();
        public List<Teacher> Teachers { get; set; } = new();
        public List<Subject> Subjects { get; set; } = new();
        public List<Grade> Grades { get; set; } = new();
    }
}
