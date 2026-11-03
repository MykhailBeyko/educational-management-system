namespace systemforstudent.Models;

public class Teacher
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public List<Subject> Subjects { get; set; } = new();

    public Teacher()
    {
    }

    public Teacher(int id, string fullName, string department)
    {
        Id = id;
        FullName = fullName;
        Department = department;
    }

    public override string ToString()
    {
        return $"[#{Id}] {FullName} | Кафедра: {Department} | Предметів: {Subjects.Count}";
    }
}
