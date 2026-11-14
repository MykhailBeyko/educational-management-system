namespace systemforstudent.Models;

public class Teacher
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = "Викладач"; // доцент, професор, асистент тощо
    public List<Subject> Subjects { get; set; } = new();

    public Teacher()
    {
    }

    public Teacher(int id, string fullName, string department, string position = "Викладач")
    {
        Id = id;
        FullName = fullName;
        Department = department;
        Position = position;
    }

    public override string ToString()
    {
        return $"[#{Id}] {FullName} | Посада: {Position} | Кафедра: {Department} | Предметів: {Subjects.Count}";
    }
}
