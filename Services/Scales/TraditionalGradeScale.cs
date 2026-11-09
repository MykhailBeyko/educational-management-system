namespace systemforstudent.Services.Scales;

public class TraditionalGradeScale : IGradeScale
{
    public string Name => "Національна шкала";

    public string ConvertToLetter(int score)
    {
        return score switch
        {
            >= 90 and <= 100 => "Відмінно",
            >= 74 and <= 89 => "Добре",
            >= 60 and <= 73 => "Задовільно",
            _ => "Незадовільно"
        };
    }
}
