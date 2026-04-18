using Microsoft.AspNetCore.Components.Routing;

namespace AdimSystem.Components.Layout;

public static class NavMenuItems
{
    public sealed record NavItem(string Href, string Text, string IconCss, NavLinkMatch? Match = null);

    public static readonly NavItem[] All =
    [
        new("", "Home", "e-home", NavLinkMatch.All),
        new("department", "Department", "e-people"),
        new("counter", "Counter", "e-plus"),
        new("weather", "Weather", "e-location"),
        new("datagrid-features", "DataGrid", "e-grid-view"),
        new("charts-features", "Charts", "e-chart"),
        new("stockchart-features", "Stock Chart", "e-chart-insert-line"),
        new("circulargauge-features", "Circular Gauge", "e-circle"),
        new("progressbar-features", "ProgressBar", "e-line"),
        new("scheduler-features", "Scheduler", "e-timeline-week"),
        new("calendar-features", "Calendar", "e-day"),
        new("daterangepicker-features", "DateRangePicker", "e-between"),
        new("datetimepicker-features", "DateTime Picker", "e-clock"),
        new("dropdownlist-features", "Dropdown List", "e-drop-down"),
        new("multiselectdropdown-features", "MultiSelect Dropdown", "e-list-unordered"),
        new("dialog-features", "Dialog", "e-comment-add"),
        new("listview-features", "ListView", "e-list-unordered"),
        new("mediaquery-features", "Media Query", "e-eye"),
        new("dashboardlayout-features", "Dashboard Layout", "e-box"),
        new("card-features", "Card", "e-table-2"),
        new("textbox-features", "TextBox", "e-text-form"),
        new("numerictextbox-features", "Numeric TextBox", "e-sum"),
        new("rating-features", "Rating", "e-top-10"),
        new("radiobutton-features", "Radio Button", "e-radio-button"),
        new("checkbox-features", "Checkbox", "e-check-box"),
        new("toggleswitchbutton-features", "Toggle Switch Button", "e-repeat"),
        new("accordion-features", "Accordion", "e-expand"),
        new("sidebar-features", "Sidebar", "e-menu"),
        new("tabs-features", "Tabs", "e-code-view"),
        new("toolbar-features", "Toolbar", "e-properties-2"),
        new("carousel-features", "Carousel", "e-image"),
        new("breadcrumb-features", "Breadcrumb", "e-chevron-right"),
        new("button-features", "Button", "e-mouse-pointer"),
        new("dropdownmenu-features", "Dropdown Menu", "e-menu"),
        new("progressbutton-features", "Progress Button", "e-print"),
        new("spinner-features", "Spinner", "e-refresh"),
        new("3dchart-features", "3D Chart", "e-chart-2d-pie-2"),
    ];
}
