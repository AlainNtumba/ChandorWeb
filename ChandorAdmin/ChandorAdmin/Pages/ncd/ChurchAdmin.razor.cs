using ChandorAdmin.Components.Dashboard;
using ChandorAdmin.Interfaces.ChurchAdmin;
using ChandorAdmin.ViewModels.ChurchAdmin;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Calendars;

namespace ChandorAdmin.Pages.ncd;

public partial class ChurchAdmin
{
    [Inject] public IChurchAdminDashboardService DashboardService { get; set; } = null!;

    static readonly DateTime PickerMinDate = new(2018, 1, 1);

    DateTime _pickerStart;
    DateTime _pickerEnd;
    ChurchAdminDashboardModel? _model;
    bool _renderRangePicker;

    DateTime PickerMaxDate => new(DateTime.Today.Year + 2, 12, 31);

    string ChartRefreshKey => _model is null
        ? "init"
        : $"{_model.Period.End:yyyyMM}-{_model.Period.Start:yyyyMMdd}-{_model.Period.End:yyyyMMdd}";

    List<Presets> DateRangePresets => DashboardDateRangePresets.StandardThreePresets();

    protected override async Task OnInitializedAsync()
    {
        var (start, end) = GetCurrentMonthRange();
        _pickerStart = start;
        _pickerEnd = end;
        try
        {
            _model = await DashboardService.GetDashboardAsync(start, end);
        }
        catch
        {
            _model = null;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(1);
            _renderRangePicker = true;
            StateHasChanged();
        }
    }

    async Task OnDateRangeChangeAsync(RangePickerEventArgs<DateTime> args)
    {
        if (args.StartDate == default || args.EndDate == default)
            return;

        var start = args.StartDate.Date;
        var end = args.EndDate.Date;
        if (start > end)
            return;

        await LoadDashboardAsync(start, end);
    }

    async Task LoadDashboardAsync(DateTime start, DateTime end)
    {
        try
        {
            _model = await DashboardService.GetDashboardAsync(start, end);
            _pickerStart = _model!.Period.Start;
            _pickerEnd = _model.Period.End;
        }
        catch
        {
            _model = null;
        }

        await InvokeAsync(StateHasChanged);
    }

    static (DateTime Start, DateTime End) GetCurrentMonthRange()
    {
        var t = DateTime.Today;
        var start = new DateTime(t.Year, t.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return (start, end);
    }

    static string FormatInt(int v) => v.ToString("N0");
}
