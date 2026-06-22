using Syncfusion.Blazor.Calendars;

namespace ChandorAdmin.Components.Dashboard;

/// <summary>
/// Preset ranges for dashboard <see cref="Syncfusion.Blazor.Calendars.SfDateRangePicker"/> controls
/// (see <c>Presets</c> on <c>DateRangePickerFeatures.razor</c>).
/// </summary>
public static class DashboardDateRangePresets
{
    /// <summary>This month, last month, and rolling three calendar months ending on the current month’s last day.</summary>
    public static List<Presets> StandardThreePresets()
    {
        var thisMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);
        var lastMonthEnd = thisMonthStart.AddDays(-1);
        var lastMonthStart = new DateTime(lastMonthEnd.Year, lastMonthEnd.Month, 1);
        var last3Start = thisMonthStart.AddMonths(-2);

        return new List<Presets>
        {
            new() { Label = "This Month", Start = thisMonthStart, End = thisMonthEnd },
            new() { Label = "Last Month", Start = lastMonthStart, End = lastMonthEnd },
            new() { Label = "Last 3 Months", Start = last3Start, End = thisMonthEnd },
        };
    }
}
