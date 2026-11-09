namespace systemforstudent.Services.Scales;

public class EctsGradeScale : IGradeScale
{
    public string Name => "ECTS (A, B, C, D, E, Fx, F)";

    public string ConvertToLetter(int score)
    {
        return score switch
        {
            >= 90 and <= 100 => "A",
            >= 82 and <= 89 => "B",
            >= 74 and <= 81 => "C",
            >= 64 and <= 73 => "D",
            >= 60 and <= 63 => "E",
            >= 35 and <= 59 => "Fx",
            _ => "F"
        };
    }
}
