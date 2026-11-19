namespace systemforstudent.Models;

public class AcademicGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public AcademicGroup()
    {
    }

    public AcademicGroup(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
