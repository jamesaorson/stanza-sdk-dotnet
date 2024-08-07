using System.Threading;
using System.Threading.Tasks;
using GetStanza.Services.Interfaces;

namespace GetStanza.Models;

internal class Guard : IGuard
{
    private readonly string _guardName;
    private readonly GuardOptions _guardOptions;
    private readonly IHubService _hubService;
    private readonly bool _failOpen;

    public Guard(string guardName, GuardOptions guardOptions, IHubService hubService, bool failOpen)
    {
        _guardName = guardName;
        _guardOptions = guardOptions;
        _hubService = hubService;
        _failOpen = failOpen;
    }

    public async Task<bool> AllowedAsync(CancellationToken cancellationToken = default)
    {
        if (_failOpen)
            return await Task.FromResult(true);

        var config = await _hubService.GetGuardConfigAsync(
            _guardName,
            _guardOptions.Tags,
            cancellationToken
        );
        if (config is not null && !config.CheckQuota)
            return true;
        var quota = await _hubService.GetQuotaTokenAsync(
            _guardName,
            _guardOptions.Feature,
            _guardOptions.Tags,
            _guardOptions.ClientId,
            _guardOptions.PriorityBoost,
            _guardOptions.DefaultWeight,
            cancellationToken
        );
        return (config?.ReportOnly ?? false) || quota.Granted;
    }

    public bool Allowed(CancellationToken cancellationToken = default)
    {
        if (_failOpen)
            return true;

        var config = _hubService.GetGuardConfig(_guardName, _guardOptions.Tags, cancellationToken);
        if (config is not null && !config.CheckQuota)
            return true;
        var quota = _hubService.GetQuotaToken(
            _guardName,
            _guardOptions.Feature,
            _guardOptions.Tags,
            _guardOptions.ClientId,
            _guardOptions.PriorityBoost,
            _guardOptions.DefaultWeight,
            cancellationToken
        );
        return (config?.ReportOnly ?? false) || quota.Granted;
    }
}
