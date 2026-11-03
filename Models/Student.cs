namespace systemforstudent.Models;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public int AdmissionYear { get; set; }
    public List<Subject> Subjects { get; set; } = new();

    public Student()
    {
    }

    public Student(int id, string fullName, string group, int admissionYear)
    {
        Id = id;
        FullName = fullName;
        Group = group;
        AdmissionYear = admissionYear;
    }

    public override string ToString()
    {
        return $"[#{Id}] {FullName} | Група: {Group} | Рік вступу: {AdmissionYear} | Предметів: {Subjects.Count}";
    }
}
