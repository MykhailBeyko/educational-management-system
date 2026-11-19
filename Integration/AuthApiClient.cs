using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using systemforstudent.Integration.Models;

namespace systemforstudent.Integration;

public sealed class AuthApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string BaseUrl { get; set; } = "http://localhost:5252";

    public AuthApiClient(HttpClient? httpClient = null)
    {
        _ownsClient = httpClient == null;
        _httpClient = httpClient ?? new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    private async Task<string> SendAsync(HttpMethod method, string path, object? body = null, string? token = null)
    {
        using var request = new HttpRequestMessage(method, $"{BaseUrl.TrimEnd('/')}/{path}");
        if (body != null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await _httpClient.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            string detail = text.Length <= 500 ? text : "Сервер повернув помилку. Перевірте його журнал.";
            throw new HttpRequestException($"Security API: HTTP {(int)response.StatusCode}. {detail}", null, response.StatusCode);
        }

        return text;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var json = await SendAsync(HttpMethod.Post, "api/Users/login", request);
        var result = JsonSerializer.Deserialize<LoginResponse>(json, JsonOptions);

        if (result == null
            || string.IsNullOrWhiteSpace(result.Token)
            || string.IsNullOrWhiteSpace(result.Role))
        {
            throw new InvalidDataException("API не повернув токен або роль.");
        }

        return result;
    }

    public Task RegisterUserAsync(string username, string password, string role, string adminToken)
    {
        var request = new RegisterRequest
        {
            Username = username,
            Password = password,
            Role = role
        };

        return SendAsync(HttpMethod.Post, "api/Users/register", request, adminToken);
    }

    public async Task<List<UserDto>> GetUsersAsync(string adminToken)
    {
        var json = await SendAsync(HttpMethod.Get, "api/Users", token: adminToken);
        using var document = JsonDocument.Parse(json);
        var users = new List<UserDto>();
        foreach (var element in document.RootElement.EnumerateArray())
        {
            // API повертає користувача всередині поля result.
            var user = element.TryGetProperty("result", out var result) ? result : element;
            users.Add(user.Deserialize<UserDto>(JsonOptions) ?? throw new InvalidDataException("Некоректна відповідь списку користувачів."));
        }

        return users;
    }

    public Task SetUserRoleAsync(string username, string newRole, string adminToken)
    {
        var request = new SetRoleRequest
        {
            Username = username,
            Role = newRole
        };

        return SendAsync(HttpMethod.Put, "api/Users/role/set", request, adminToken);
    }

    // Пароль змінюється за токеном власника акаунта.
    public Task ChangePasswordAsync(string oldPassword, string newPassword, string userToken)
    {
        var request = new ChangePasswordRequest
        {
            OldPassword = oldPassword,
            NewPassword = newPassword
        };

        return SendAsync(HttpMethod.Put, "api/Users/password/change", request, userToken);
    }

    public void Dispose()
    {
        if (_ownsClient)
        {
            _httpClient.Dispose();
        }
    }
}
