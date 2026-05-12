using ChandorAdmin.Components.Finance.Transactions;

namespace ChandorAdmin.Pages.ncd;

public partial class Transactions
{
    TransactionGridPanel? _gridRef;
    TransactionFilterSidebar? _filterRef;
    TransactionEditorDialog? _dialogRef;

    string? _searchValue;
    string _searchVisibility = "none";
    string _searchHeadBg = "#FFFFFF";
    bool _isDataLoaded;
    string _typeFilter = "All";

    readonly List<TypeFilterItem> _typeFilterItems =
    [
        new TypeFilterItem { Text = "All", Value = "All" },
    new TypeFilterItem { Text = "Income", Value = "Income" },
    new TypeFilterItem { Text = "Expense", Value = "Expense" }
    ];

    bool _didInitialWire;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!_isDataLoaded || _didInitialWire || _gridRef is null || _filterRef is null || _dialogRef is null)
            return;

        _didInitialWire = true;
        _gridRef.DialogRef = _dialogRef;
        _filterRef.ContentRef = _gridRef;
        _dialogRef.ContentRef = _gridRef;
        _gridRef.FilterRef = _filterRef;

        _filterRef.RebuildCategoryList();
        _filterRef.ApplyToolbarTypeFilter(_typeFilter);
        await InvokeAsync(StateHasChanged);
    }

    void OnSearchCreated() => (_searchVisibility, _searchHeadBg) = ("", "");

    Task OnTypeFilterValueChanged(string value)
    {
        _typeFilter = string.IsNullOrEmpty(value) ? "All" : value;
        _filterRef?.ApplyToolbarTypeFilter(_typeFilter);
        return Task.CompletedTask;
    }

    async Task OnSearchChanged(Syncfusion.Blazor.Inputs.ChangedEventArgs args)
    {
        if (_gridRef is not null)
            await _gridRef.SearchAsync(_searchValue);
    }

    Task RunSearchAsync() => _gridRef?.SearchAsync(_searchValue) ?? Task.CompletedTask;

    Task OpenAddDialogAsync() => _dialogRef?.ShowAddDialog() ?? Task.CompletedTask;

    void OpenFilterMenu() => _filterRef?.ShowFilterMenu();

    public void Dispose()
    {
        _gridRef = null;
        _filterRef = null;
        _dialogRef = null;
        _isDataLoaded = false;
        _didInitialWire = false;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            _ = LoadAsync();
    }

    async Task LoadAsync()
    {
        await Task.Delay(500);
        _isDataLoaded = true;
        await InvokeAsync(StateHasChanged);
    }

    private sealed class TypeFilterItem
    {
        public string Text { get; set; } = "";
        public string Value { get; set; } = "";
    }
}