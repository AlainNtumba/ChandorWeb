using ChandorAdmin.Configuration;
using ChandorAdmin.Interfaces.Auth;
using Microsoft.Extensions.Options;

namespace ChandorAdmin.Services.Auth;

/// <summary>Tracks the last time the user sent input while authenticated and enforces a configured idle timeout. Uses in-process-only timers (no <c>localStorage</c>).</summary>
public sealed class InactivityMonitor : IAsyncDisposable
{
    private readonly IAuthState _authState;
    private readonly IAuthService _authService;
    private readonly ILogger<InactivityMonitor> _logger;
    private readonly TimeSpan _timeout;

    private readonly object _lock = new();
    private DateTimeOffset _lastActivityUtc;
    private PeriodicTimer? _timer;
    private Task? _loop;
    private CancellationTokenSource? _cts;
    private bool _started;

    public InactivityMonitor(
        IAuthState authState,
        IAuthService authService,
        IOptions<AuthOptions> options,
        ILogger<InactivityMonitor> logger)
    {
        _authState = authState;
        _authService = authService;
        _logger = logger;
        _timeout = TimeSpan.FromMinutes(Math.Max(1, options.Value.InactivityTimeoutMinutes));
    }

    public void Start()
    {
        if (_started)
            return;
        _started = true;
        _lastActivityUtc = DateTimeOffset.UtcNow;
        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        _loop = CheckLoopAsync(_cts.Token);
    }

    public void RecordActivity()
    {
        lock (_lock)
        {
            _lastActivityUtc = DateTimeOffset.UtcNow;
        }
    }

    private async Task CheckLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_timer is null)
                return;
            while (await _timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                if (string.IsNullOrEmpty(_authState.AccessToken))
                    continue;

                var idle = DateTimeOffset.UtcNow;
                bool timedOut;
                lock (_lock)
                {
                    timedOut = idle - _lastActivityUtc > _timeout;
                }

                if (timedOut)
                {
                    _logger.LogInformation("Session ended due to inactivity.");
                    await _authService.LogoutAsync(cancellationToken, redirectToLogin: true).ConfigureAwait(false);
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_cts is not null)
            await _cts.CancelAsync();
        if (_loop is { } t)
        {
            try
            {
                await t.ConfigureAwait(false);
            }
            catch
            {
                // ignore
            }
        }

        _timer?.Dispose();
        _cts?.Dispose();
    }
}
