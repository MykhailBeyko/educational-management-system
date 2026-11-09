namespace systemforstudent.Services.Scales;

public interface IGradeScale
{
    string Name { get; }
    string ConvertToLetter(int score);
}
