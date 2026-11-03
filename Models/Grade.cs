namespace systemforstudent.Models;

public class Grade
{
    public int Id { get; set; }
    public Student Student { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public int NumericValue { get; set; }
    public string LetterGrade { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    public Grade()
    {
    }

    public Grade(int id, Student student, Subject subject, Teacher teacher, int numericValue, string letterGrade, DateTime date)
    {
        Id = id;
        Student = student;
        Subject = subject;
        Teacher = teacher;
        NumericValue = numericValue;
        LetterGrade = letterGrade;
        Date = date;
    }

    public override string ToString()
    {
        return $"[#{Id}] Студент: {Student.FullName} | Предмет: {Subject.Name} | Оцінка: {NumericValue} ({LetterGrade}) | Викладач: {Teacher.FullName} | Дата: {Date:dd.MM.yyyy}";
    }
}
