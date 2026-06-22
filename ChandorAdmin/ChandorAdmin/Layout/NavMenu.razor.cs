using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Department;
using ChandorProject.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace ChandorAdmin.Layout;

public partial class NavMenu
{
    [Inject] private IDepartmentService DepartmentService { get; set; } = null!;

    IReadOnlyList<DepartmentDto> _departmentNavItems = Array.Empty<DepartmentDto>();
    bool _departmentsNavLoading = true;
    string? _departmentsNavError;

    protected override async Task OnInitializedAsync() => await LoadDepartmentsForNavAsync();

    async Task LoadDepartmentsForNavAsync()
    {
        _departmentsNavLoading = true;
        _departmentsNavError = null;

        try
        {
            var response = await DepartmentService.GetDepartmentsAsync();
            if (response is { Success: true, Data: not null })
            {
                _departmentNavItems = response.Data
                    .Where(d => d.Id != Guid.Empty)
                    .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            else
            {
                _departmentsNavError = FormatDepartmentListError(response);
            }
        }
        catch (Exception)
        {
            _departmentsNavError = "Unable to load departments. Please try again later.";
        }
        finally
        {
            _departmentsNavLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    static string FormatDepartmentListError<T>(DataResponse<T>? response)
    {
        if (response?.Message is { Length: > 0 } msg)
            return msg;

        if (response?.Error is { Length: > 0 } errs)
        {
            var parts = errs.Where(static e => !string.IsNullOrWhiteSpace(e)).ToArray();
            if (parts.Length > 0)
                return string.Join(" ", parts);
        }

        return "Departments could not be loaded.";
    }
}
