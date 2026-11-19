namespace IdeaVault.Web.Services;

/// <summary>Tiny notification bus for transient messages ("Ideia capturada", errors...).</summary>
public sealed class ToastService
{
    public event Action<string, bool>? Shown;

    public void Success(string message) => Shown?.Invoke(message, false);

    public void Error(string message) => Shown?.Invoke(message, true);

    public void Error(Exception exception) =>
        Error(exception is ApiException api ? api.Message : "Algo deu errado. Tente novamente.");
}
