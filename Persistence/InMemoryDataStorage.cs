using systemforstudent.Models;

namespace systemforstudent.Persistence;

public class InMemoryDataStorage : IDataStorage
{
    public List<Student> Students { get; private set; } = new();
    public List<Teacher> Teachers { get; private set; } = new();
    public List<Subject> Subjects { get; private set; } = new();
    public List<Grade> Grades { get; private set; } = new();
    public List<Department> Departments { get; private set; } = new();
    public List<string> Groups { get; private set; } = new();

    public InMemoryDataStorage()
    {
        SeedInitialData();
    }

    public void Save()
    {
    }

    private void SeedInitialData()
    {
        // 0. Групи
        Groups.AddRange(new[] { "ІПЗ-21", "КН-22" });

        // 1. Кафедри
        var dep1 = new Department(1, "Інформаційні технології", "проф. Сидоренко В. М.");
        var dep2 = new Department(2, "Вища та прикладна математика", "доц. Петренко Л. І.");
        Departments.AddRange(new[] { dep1, dep2 });

        // 2. Дисципліни
        var s1 = new Subject(1, "Об'єктно-орієнтоване програмування", 2, 5);
        var s2 = new Subject(2, "Вища математика", 1, 4);
        var s3 = new Subject(3, "Бази даних", 3, 5);
        var s4 = new Subject(4, "Алгоритми та структури даних", 2, 4);
        var s5 = new Subject(5, "Дискретна математика", 1, 3);
        var s6 = new Subject(6, "Веб-технології", 3, 4);
        Subjects.AddRange(new[] { s1, s2, s3, s4, s5, s6 });

        // 3. Викладачі
        var t1 = new Teacher(1, "Коваленко Олександр Сергійович", dep1.Name, "Доцент");
        t1.Subjects.AddRange(new[] { s1, s3 });
        dep1.Teachers.Add(t1);

        var t2 = new Teacher(2, "Мельник Ірина Петрівна", dep2.Name, "Професор");
        t2.Subjects.AddRange(new[] { s2, s5 });
        dep2.Teachers.Add(t2);

        var t3 = new Teacher(3, "Гриценко Максим Володимирович", dep1.Name, "Старший викладач");
        t3.Subjects.AddRange(new[] { s4, s6 });
        dep1.Teachers.Add(t3);

        var t4 = new Teacher(4, "Ткаченко Ольга Василівна", dep2.Name, "Асистент");
        t4.Subjects.Add(s2);
        dep2.Teachers.Add(t4);

        Teachers.AddRange(new[] { t1, t2, t3, t4 });

        // 4. Студенти
        var st1 = new Student(1, "Іваненко Петро Олексійович", "ІПЗ-21", 2023);
        st1.Subjects.AddRange(new[] { s1, s2, s3, s4, s5 });

        var st2 = new Student(2, "Шевченко Анна Михайлівна", "ІПЗ-21", 2023);
        st2.Subjects.AddRange(new[] { s1, s2, s3, s4 });

        var st3 = new Student(3, "Ковальчук Артем Денисович", "ІПЗ-21", 2023);
        st3.Subjects.AddRange(new[] { s1, s2, s3, s6 });

        var st4 = new Student(4, "Мороз Софія Ярославівна", "ІПЗ-21", 2023);
        st4.Subjects.AddRange(new[] { s1, s2, s4, s5 });

        var st5 = new Student(5, "Бондаренко Данило Ігорович", "КН-22", 2024);
        st5.Subjects.AddRange(new[] { s2, s5, s1 });

        var st6 = new Student(6, "Кравченко Вікторія Олександрівна", "КН-22", 2024);
        st6.Subjects.AddRange(new[] { s2, s5, s4 });

        var st7 = new Student(7, "Ткач Богдан Миколайович", "КН-22", 2024);
        st7.Subjects.AddRange(new[] { s2, s5 });

        Students.AddRange(new[] { st1, st2, st3, st4, st5, st6, st7 });

        // 5. Оцінки
        Grades.Add(new Grade(1,  st1, s1, t1, 95, "A", DateTime.Now.AddDays(-30)));
        Grades.Add(new Grade(2,  st1, s2, t2, 88, "B", DateTime.Now.AddDays(-28)));
        Grades.Add(new Grade(3,  st1, s3, t1, 92, "A", DateTime.Now.AddDays(-10)));
        Grades.Add(new Grade(4,  st1, s4, t3, 90, "A", DateTime.Now.AddDays(-15)));

        Grades.Add(new Grade(5,  st2, s1, t1, 78, "C", DateTime.Now.AddDays(-25)));
        Grades.Add(new Grade(6,  st2, s2, t2, 82, "B", DateTime.Now.AddDays(-24)));
        Grades.Add(new Grade(7,  st2, s3, t1, 48, "Fx", DateTime.Now.AddDays(-5)));

        Grades.Add(new Grade(8,  st3, s1, t1, 85, "B", DateTime.Now.AddDays(-20)));
        Grades.Add(new Grade(9,  st3, s2, t2, 74, "C", DateTime.Now.AddDays(-19)));
        Grades.Add(new Grade(10, st3, s3, t1, 81, "C", DateTime.Now.AddDays(-8)));

        Grades.Add(new Grade(11, st4, s1, t1, 96, "A", DateTime.Now.AddDays(-18)));
        Grades.Add(new Grade(12, st4, s2, t2, 91, "A", DateTime.Now.AddDays(-17)));
        Grades.Add(new Grade(13, st4, s4, t3, 94, "A", DateTime.Now.AddDays(-12)));

        Grades.Add(new Grade(14, st5, s2, t2, 65, "E", DateTime.Now.AddDays(-14)));
        Grades.Add(new Grade(15, st5, s5, t2, 70, "D", DateTime.Now.AddDays(-13)));

        Grades.Add(new Grade(16, st6, s2, t2, 89, "B", DateTime.Now.AddDays(-11)));
        Grades.Add(new Grade(17, st6, s5, t2, 84, "B", DateTime.Now.AddDays(-10)));

        Grades.Add(new Grade(18, st7, s2, t4, 55, "Fx", DateTime.Now.AddDays(-7)));
        Grades.Add(new Grade(19, st7, s5, t2, 62, "E", DateTime.Now.AddDays(-6)));
    }
}
