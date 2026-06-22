namespace ChandorAdmin.ViewModels.ChurchAdmin;

/// <summary>Reporting window for church admin dashboards (mirrors API date-range contracts).</summary>
public sealed record ChurchAdminPeriod(DateTime Start, DateTime End, string Label);

/// <summary>Top-level KPI strip returned by the church admin dashboard API.</summary>
public sealed class ChurchAdminSummaryModel
{
    public int TotalRegisteredMembers { get; init; }
    public int TotalServingMembers { get; init; }
    public int TotalOutreachSouls { get; init; }
    public int TotalBornAgain { get; init; }
    public int TotalVisitors { get; init; }
}

/// <summary>Monthly point for member growth chart; series is built for the 7 months ending at the selected range end.</summary>
public sealed class MemberGrowthSeriesPoint
{
    public string MonthLabel { get; init; } = "";
    public int NewRegistrations { get; init; }
    public int MembershipTotal { get; init; }
}

/// <summary>Slice for ministry distribution donut.</summary>
public sealed class MinistryDistributionSlice
{
    public string Ministry { get; init; } = "";
    public int MemberCount { get; init; }
}

/// <summary>Row for recent member activities grid.</summary>
public sealed class MemberActivityGridRow
{
    public string MemberId { get; init; } = "";
    public string FullName { get; init; } = "";
    public string ActivityType { get; init; } = "";
    public string Ministry { get; init; } = "";
    public DateTime ActivityDate { get; init; }
    public string ResponsibleAdmin { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Upcoming church event for operations sidebar.</summary>
public sealed class ChurchEventListItem
{
    public string Title { get; init; } = "";
    public string Ministry { get; init; } = "";
    public DateTime StartsAtUtc { get; init; }
    public string Location { get; init; } = "";
    public string Coordinator { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Aggregate payload for the church administration dashboard (ready for API mapping).</summary>
public sealed class ChurchAdminDashboardModel
{
    public ChurchAdminPeriod Period { get; init; } = null!;
    public ChurchAdminSummaryModel Summary { get; init; } = null!;
    public IReadOnlyList<MemberGrowthSeriesPoint> MemberGrowth { get; init; } = Array.Empty<MemberGrowthSeriesPoint>();
    public IReadOnlyList<MinistryDistributionSlice> MinistryDistribution { get; init; } = Array.Empty<MinistryDistributionSlice>();
    public IReadOnlyList<MemberActivityGridRow> RecentMemberActivities { get; init; } = Array.Empty<MemberActivityGridRow>();
    public IReadOnlyList<ChurchEventListItem> UpcomingEvents { get; init; } = Array.Empty<ChurchEventListItem>();
    public string MemberGrowthChartHint { get; init; } = "";
    public IReadOnlyList<string> MinistryCompositionPalette { get; init; } = Array.Empty<string>();
}
