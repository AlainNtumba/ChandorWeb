using ChandorProject.Shared.Models;
using Syncfusion.Blazor.Navigations;

namespace ChandorAdmin.Components.Department;

internal static class DepartmentManagementGridSupport
{
    internal const string CreateSuccessMessage = "L'opération a été effectuée avec succès.";
    internal const string UpdateSuccessMessage = "La modification a été effectuée avec succès.";
    internal const string DeleteSuccessMessage = "La suppression a été effectuée avec succès.";
    internal const string LoadErrorMessage = "Une erreur est survenue lors du chargement des données.";
    internal const string SaveErrorMessage = "Une erreur est survenue lors de l'enregistrement.";
    internal const string DeleteErrorMessage = "Une erreur est survenue lors de la suppression.";
    internal const string ValidationErrorMessage = "Veuillez corriger les erreurs avant de continuer.";
    internal const string NoDataMessage = "Aucune donnée trouvée.";

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

    internal static readonly List<object> ReadOnlyToolbarItems =
    [
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
