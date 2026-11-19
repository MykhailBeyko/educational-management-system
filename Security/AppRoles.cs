namespace systemforstudent.Security;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Student = "Student";
    public const string Teacher = "Teacher";
    public const string DepartmentHead = "DepartmentHead";

    public static bool IsKnown(string role)
    {
        return role is Admin or Student or Teacher or DepartmentHead;
    }
}

public sealed record UserSession(string Username, string Token, string Role, int? EntityId)
{
    public void RequireRole(string role)
    {
        if (Role != role)
        {
            throw new UnauthorizedAccessException("Дія недоступна для вашої ролі.");
        }
    }
}