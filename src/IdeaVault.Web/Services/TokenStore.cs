using Microsoft.JSInterop;

namespace IdeaVault.Web.Services;

/// <summary>Persists the JWT in localStorage so the PWA stays logged in between launches.</summary>
public sealed class TokenStore(IJSRuntime js)
{
    private const string TokenKey = "ideavault.token";
    private const string ExpiresKey = "ideavault.expires";

    private string? _token;
    private bool _loaded;

    public async ValueTask<string?> GetAsync()
    {
        if (!_loaded)
        {
            _token = await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            var expires = await js.InvokeAsync<string?>("localStorage.getItem", ExpiresKey);
            if (DateTime.TryParse(expires, null, System.Globalization.DateTimeStyles.RoundtripKind, out var at)
                && at <= DateTime.UtcNow)
            {
                await ClearAsync();
            }

            _loaded = true;
        }

        return _token;
    }

    public async ValueTask SaveAsync(string token, DateTime expiresAt)
    {
        _token = token;
        _loaded = true;
        await js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        await js.InvokeVoidAsync("localStorage.setItem", ExpiresKey, expiresAt.ToString("O"));
    }

    public async ValueTask ClearAsync()
    {
        _token = null;
        await js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await js.InvokeVoidAsync("localStorage.removeItem", ExpiresKey);
    }
}
