namespace systemforstudent.Services.Scales;

public class CustomGradeScale : IGradeScale
{
    public string Name { get; }
    private readonly List<(int MinScore, string Label)> _rules;

    public CustomGradeScale(string name, List<(int MinScore, string Label)> rules)
    {
        Name = name;
        _rules = rules.OrderByDescending(r => r.MinScore).ToList();
    }

    public string ConvertToLetter(int score)
    {
        foreach (var rule in _rules)
        {
            if (score >= rule.MinScore)
                return rule.Label;
        }
        return "F";
    }
}
