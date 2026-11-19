using System.Text;
using systemforstudent.Integration;
using systemforstudent.Persistence;
using systemforstudent.Presentation;
using systemforstudent.Services;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

try
{
    using var storage = new EfDataStorage(Environment.GetEnvironmentVariable("STUDY_DB") ?? "Data Source=university.db");
    using var api = new AuthApiClient
    {
        BaseUrl = Environment.GetEnvironmentVariable("SECURITY_API_URL") ?? "http://localhost:5252"
    };
    await new ConsoleApplication(storage, new AccountService(api, storage)).RunAsync();
}
catch (Exception ex) when (ex is IOException or Microsoft.Data.Sqlite.SqliteException or Microsoft.EntityFrameworkCore.DbUpdateException)
{
    Console.Error.WriteLine($"Не вдалося відкрити або зберегти навчальні дані: {ex.Message}");
    Environment.ExitCode = 1;
}
