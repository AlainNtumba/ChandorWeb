using ChandorProject.Shared.Models;
using Syncfusion.Blazor.Navigations;

namespace ChandorAdmin.Components.Member.Management;

internal static class MemberManagementGridSupport
{
    internal static readonly List<object> CrudToolbarItems =
    [
        "Add",
        new ItemModel { Text = "Edit", PrefixIcon = "e-edit e-icons", TooltipText = "Edit", Id = "Edit" },
        new ItemModel { Text = "Delete", PrefixIcon = "e-delete e-icons", TooltipText = "Delete", Id = "Delete" },
        "Update",
        "Cancel",
        "ExcelExport",
        "Search"
    ];

    internal static bool IsSaveAddAction(string? action) =>
        string.Equals(action, "add", StringComparison.OrdinalIgnoreCase);

    internal static bool IsSaveEditAction(string? action) =>
        string.Equals(action, "edit", StringComparison.OrdinalIgnoreCase);

    internal static bool IsExcelExport(ClickEventArgs args) =>
        string.Equals(args.Item.Id, "Grid_excelexport", StringComparison.OrdinalIgnoreCase)
        || string.Equals(args.Item.Id, "ExcelExport", StringComparison.OrdinalIgnoreCase)
        || args.Item.Text?.Contains("Excel", StringComparison.OrdinalIgnoreCase) == true;

    internal static string ComputeAdaptiveGridHeight(int rowCount, int pageSize = 10)
    {
        const int toolbarHeight = 42;
        const int headerHeight = 38;
        const int rowHeight = 36;
        const int pagerHeight = 42;
        const int chrome = 8;
        const int minHeight = 220;

        var visibleRows = Math.Clamp(rowCount, 1, pageSize);
        var contentHeight = toolbarHeight + headerHeight + (visibleRows * rowHeight) + pagerHeight + chrome;

        return $"min({Math.Max(contentHeight, minHeight)}px, calc(80vh - 5.5rem))";
    }

    internal static string FormatApiErrorMessage<T>(DataResponse<T>? response, string fallback)
    {
        if (response?.Message is { Length: > 0 } msg)
        {
            var errors = response.Error?.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
            return errors is { Length: > 0 } ? $"{msg} {string.Join(" ", errors)}" : msg;
        }

        if (response?.Error is { Length: > 0 } errs)
        {
            var parts = errs.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
            if (parts.Length > 0)
                return string.Join(" ", parts);
        }

        return fallback;
    }
}
