using systemforstudent.Integration;
using systemforstudent.Integration.Models;
using systemforstudent.Persistence;
using systemforstudent.Security;

namespace systemforstudent.Services;

public sealed class AccountService
{
    private readonly AuthApiClient _api;
    private readonly EfDataStorage _storage;

    public AccountService(AuthApiClient api, EfDataStorage storage)
    {
        _api = api;
        _storage = storage;
    }

    public async Task<UserSession> LoginAsync(string username, string password)
    {
        username = username.Trim();
        var login = await _api.LoginAsync(username, password);
        if (!AppRoles.IsKnown(login.Role))
        {
            throw new UnauthorizedAccessException($"Невідома роль: {login.Role}.");
        }

        if (login.Role == AppRoles.Admin)
        {
            return new UserSession(username, login.Token, login.Role, null);
        }

        var link = _storage.GetAccountLink(username);
        if (link == null || link.Role != login.Role)
        {
            throw new UnauthorizedAccessException("Адміністратор має прив'язати акаунт до навчального профілю для цієї ролі.");
        }

        ValidateProfile(link.Role, link.EntityId);
        return new UserSession(username, login.Token, login.Role, link.EntityId);
    }

    public void ValidateProfile(string role, int entityId)
    {
        bool valid = role switch
        {
            AppRoles.Admin => entityId == 0,
            AppRoles.Student => _storage.Students.Any(s => s.Id == entityId),
            AppRoles.Teacher => _storage.Teachers.Any(t => t.Id == entityId),
            AppRoles.DepartmentHead => _storage.Departments.Any(d => d.Id == entityId),
            _ => false
        };
        if (!valid)
        {
            throw new ArgumentException("Невідома роль або не знайдено навчальний профіль з таким ID.");
        }
    }

    public Task<List<UserDto>> GetUsersAsync(UserSession admin)
    {
        admin.RequireRole(AppRoles.Admin);
        return _api.GetUsersAsync(admin.Token);
    }

    public async Task RegisterAsync(UserSession admin, string username, string password, string role, int entityId)
    {
        admin.RequireRole(AppRoles.Admin);
        ValidateProfile(role, entityId);
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Логін і пароль обов'язкові.");
        }

        username = username.Trim();
        await _api.RegisterUserAsync(username, password, role, admin.Token);
        SaveLink(username, role, entityId);
    }

    public async Task BindAsync(UserSession admin, string username, int entityId)
    {
        var users = await GetUsersAsync(admin);
        var user = users.SingleOrDefault(u =>
            u.UserName.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            throw new ArgumentException("Користувача не знайдено в API.");
        }

        ValidateProfile(user.Role, entityId);
        SaveLink(user.UserName, user.Role, entityId);
    }

    public async Task ChangeRoleAsync(UserSession admin, string username, string role, int entityId)
    {
        admin.RequireRole(AppRoles.Admin);
        if (admin.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Змінюйте власну роль з іншого адміністративного акаунта.");
        }

        ValidateProfile(role, entityId);
        await _api.SetUserRoleAsync(username.Trim(), role, admin.Token);
        SaveLink(username.Trim(), role, entityId);
    }

    private void SaveLink(string username, string role, int entityId)
    {
        try
        {
            _storage.SetAccountLink(username, role, entityId);
        }
        catch (Exception ex) when (ex is Microsoft.EntityFrameworkCore.DbUpdateException or Microsoft.Data.Sqlite.SqliteException)
        {
            throw new InvalidOperationException("Акаунт оновлено в API, але прив'язку не збережено. Скористайтеся пунктом прив'язки наявного акаунта.", ex);
        }
    }

    public Task ChangePasswordAsync(UserSession session, string oldPassword, string newPassword)
    {
        if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
        {
            throw new ArgumentException("Вкажіть старий і новий пароль.");
        }

        return _api.ChangePasswordAsync(oldPassword, newPassword, session.Token);
    }
}
