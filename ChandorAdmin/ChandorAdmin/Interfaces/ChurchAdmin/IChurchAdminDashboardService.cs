using ChandorAdmin.ViewModels.ChurchAdmin;

namespace ChandorAdmin.Interfaces.ChurchAdmin;

/// <summary>Church administration dashboard data (mock today; swap implementation for HTTP client later).</summary>
public interface IChurchAdminDashboardService
{
    Task<ChurchAdminDashboardModel> GetDashboardAsync(DateTime rangeStart, DateTime rangeEnd, CancellationToken cancellationToken = default);
}
