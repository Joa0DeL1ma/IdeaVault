using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using IdeaVault.Core.Dtos;
using Microsoft.AspNetCore.Components;

namespace IdeaVault.Web.Services;

/// <summary>Typed client for the IdeaVault API. Adds the JWT and turns errors into <see cref="ApiException"/>.</summary>
public sealed class ApiClient(HttpClient http, TokenStore tokens, NavigationManager nav)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    // Auth
    public async Task LoginAsync(string password)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", new { password }, Json);
        var login = await ReadAsync<LoginResult>(response);
        await tokens.SaveAsync(login.Token, login.ExpiresAt);
    }

    public async Task LogoutAsync()
    {
        await tokens.ClearAsync();
        nav.NavigateTo("login");
    }

    // Categories
    public Task<List<CategoryDto>> GetCategoriesAsync() => SendAsync<List<CategoryDto>>(HttpMethod.Get, "api/categories");
    public Task<CategoryDto> CreateCategoryAsync(SaveCategoryRequest r) => SendAsync<CategoryDto>(HttpMethod.Post, "api/categories", r);
    public Task<CategoryDto> UpdateCategoryAsync(Guid id, SaveCategoryRequest r) => SendAsync<CategoryDto>(HttpMethod.Put, $"api/categories/{id}", r);
    public Task DeleteCategoryAsync(Guid id) => SendAsync(HttpMethod.Delete, $"api/categories/{id}");

    // Ideas
    public Task<List<IdeaSummaryDto>> SearchIdeasAsync(IdeaQuery q)
    {
        var query = new Dictionary<string, string?>
        {
            ["categoryId"] = q.CategoryId?.ToString(),
            ["status"] = q.Status?.ToString(),
            ["tag"] = q.Tag,
            ["search"] = q.Search,
            ["sort"] = q.Sort.ToString(),
        };
        var qs = string.Join('&', query.Where(p => !string.IsNullOrWhiteSpace(p.Value))
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}"));
        return SendAsync<List<IdeaSummaryDto>>(HttpMethod.Get, $"api/ideas?{qs}");
    }

    public Task<IdeaDetailDto> GetIdeaAsync(Guid id) => SendAsync<IdeaDetailDto>(HttpMethod.Get, $"api/ideas/{id}");
    public Task<IdeaDetailDto> CreateIdeaAsync(CreateIdeaRequest r) => SendAsync<IdeaDetailDto>(HttpMethod.Post, "api/ideas", r);
    public Task<IdeaDetailDto> QuickCaptureAsync(string text) => SendAsync<IdeaDetailDto>(HttpMethod.Post, "api/ideas/quick", new QuickCaptureRequest(text));
    public Task<IdeaDetailDto> UpdateIdeaAsync(Guid id, UpdateIdeaRequest r) => SendAsync<IdeaDetailDto>(HttpMethod.Put, $"api/ideas/{id}", r);
    public Task DeleteIdeaAsync(Guid id) => SendAsync(HttpMethod.Delete, $"api/ideas/{id}");

    // Sections
    public Task<IdeaSectionDto> AddSectionAsync(Guid ideaId, SaveSectionRequest r) => SendAsync<IdeaSectionDto>(HttpMethod.Post, $"api/ideas/{ideaId}/sections", r);
    public Task<IdeaSectionDto> UpdateSectionAsync(Guid ideaId, Guid id, SaveSectionRequest r) => SendAsync<IdeaSectionDto>(HttpMethod.Put, $"api/ideas/{ideaId}/sections/{id}", r);
    public Task DeleteSectionAsync(Guid ideaId, Guid id) => SendAsync(HttpMethod.Delete, $"api/ideas/{ideaId}/sections/{id}");
    public Task<List<IdeaSectionDto>> ReorderSectionsAsync(Guid ideaId, IReadOnlyList<Guid> ids) =>
        SendAsync<List<IdeaSectionDto>>(HttpMethod.Put, $"api/ideas/{ideaId}/sections/order", new ReorderSectionsRequest(ids));

    // Export (raw JSON, saved as a file by the browser)
    public async Task<string> ExportAsync()
    {
        var response = await SendRawAsync(HttpMethod.Get, "api/export");
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string url, object? body = null) =>
        await ReadAsync<T>(await SendRawAsync(method, url, body));

    private async Task SendAsync(HttpMethod method, string url, object? body = null) =>
        await SendRawAsync(method, url, body);

    private async Task<HttpResponseMessage> SendRawAsync(HttpMethod method, string url, object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);
        if (await tokens.GetAsync() is { } token)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: Json);
        }

        var response = await http.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await LogoutAsync(); // expired or invalid token
            throw new ApiException(401, "Sua sessão expirou. Entre novamente.");
        }

        await EnsureSuccessAsync(response);
        return response;
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        Problem? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<Problem>(Json);
        }
        catch (JsonException)
        {
            // not a ProblemDetails body
        }

        var message = response.StatusCode switch
        {
            HttpStatusCode.TooManyRequests => "Muitas tentativas. Aguarde um minuto.",
            HttpStatusCode.Unauthorized => "Senha incorreta.",
            _ => problem?.Detail ?? problem?.Title ?? $"Erro {(int)response.StatusCode}",
        };

        throw new ApiException((int)response.StatusCode, message, problem?.Errors);
    }

    private sealed record LoginResult(string Token, DateTime ExpiresAt);

    private sealed record Problem(string? Title, string? Detail, Dictionary<string, string[]>? Errors);
}
