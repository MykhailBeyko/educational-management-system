namespace systemforstudent.Models;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Semester { get; set; }
    public int Credits { get; set; }

    public Subject()
    {
    }

    public Subject(int id, string name, int semester, int credits)
    {
        Id = id;
        Name = name;
        Semester = semester;
        Credits = credits;
    }

    public override string ToString()
    {
        return $"[#{Id}] {Name} (Семестр: {Semester}, Кредитів: {Credits})";
    }
}
