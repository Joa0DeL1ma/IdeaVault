namespace IdeaVault.Web.Services;

/// <summary>Runs an action only after calls stop for <c>delay</c> (used for auto-save while typing).</summary>
public sealed class Debouncer(TimeSpan delay) : IDisposable
{
    private CancellationTokenSource? _cts;
    private Func<Task>? _pending;

    public void Run(Func<Task> action)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        _pending = action;
        _ = WaitThenRunAsync(_cts.Token);
    }

    /// <summary>Runs the pending action right away (e.g. when the user leaves the page).</summary>
    public Task FlushAsync()
    {
        _cts?.Cancel();
        return RunPendingAsync();
    }

    /// <summary>Drops the pending action without running it.</summary>
    public void Cancel()
    {
        _cts?.Cancel();
        _pending = null;
    }

    public void Dispose() => _cts?.Cancel();

    private async Task WaitThenRunAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(delay, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        await RunPendingAsync();
    }

    private Task RunPendingAsync()
    {
        var action = _pending;
        _pending = null;
        return action?.Invoke() ?? Task.CompletedTask;
    }
}
