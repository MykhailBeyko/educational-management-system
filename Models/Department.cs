namespace systemforstudent.Models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HeadOfDepartment { get; set; } = string.Empty; // Завідувач кафедри
    public List<Teacher> Teachers { get; set; } = new();

    public Department()
    {
    }

    public Department(int id, string name, string headOfDepartment)
    {
        Id = id;
        Name = name;
        HeadOfDepartment = headOfDepartment;
    }

    public override string ToString()
    {
        return $"[#{Id}] Кафедра: \"{Name}\" | Завідувач: {HeadOfDepartment} | Викладачів: {Teachers.Count}";
    }
}
