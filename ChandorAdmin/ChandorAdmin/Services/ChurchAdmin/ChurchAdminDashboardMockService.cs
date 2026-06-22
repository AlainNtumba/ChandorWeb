using ChandorAdmin.Interfaces.ChurchAdmin;
using ChandorAdmin.ViewModels.ChurchAdmin;

namespace ChandorAdmin.Services.ChurchAdmin;

public sealed class ChurchAdminDashboardMockService : IChurchAdminDashboardService
{
    static readonly string[] MinistryPalette =
    [
        "#6366f1", "#8b5cf6", "#a855f7", "#d946ef", "#ec4899",
        "#f43f5e", "#f97316"
    ];

    public Task<ChurchAdminDashboardModel> GetDashboardAsync(DateTime rangeStart, DateTime rangeEnd, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var start = rangeStart.Date;
        var end = rangeEnd.Date;
        // HashCode.Combine may be negative; keep seed non-negative so % indexing stays in range.
        var seed = HashCode.Combine(start.Year, start.Month, start.Day, end.Year, end.Month, end.Day) & int.MaxValue;

        var summary = new ChurchAdminSummaryModel
        {
            TotalRegisteredMembers = 1842 + (seed % 120),
            TotalServingMembers = 286 + (seed % 40),
            TotalOutreachSouls = 512 + (seed % 80),
            TotalBornAgain = 148 + (seed % 25),
            TotalVisitors = 96 + (seed % 30)
        };

        var growth = BuildMemberGrowth(end, seed);
        var ministries = BuildMinistrySlices(seed);
        var activities = BuildActivities(start, end, seed);
        var events = BuildUpcomingEvents(end, seed);

        var label = $"{start:MMM d, yyyy} – {end:MMM d, yyyy}";
        var model = new ChurchAdminDashboardModel
        {
            Period = new ChurchAdminPeriod(start, end, label),
            Summary = summary,
            MemberGrowth = growth,
            MinistryDistribution = ministries,
            RecentMemberActivities = activities,
            UpcomingEvents = events,
            MemberGrowthChartHint = $"Last 7 months (through {end:MMMM yyyy}) · new registrations vs cumulative membership",
            MinistryCompositionPalette = MinistryPalette
        };

        return Task.FromResult(model);
    }

    /// <summary>Seven calendar months ending at the range end month (inclusive), for grouped column chart.</summary>
    static IReadOnlyList<MemberGrowthSeriesPoint> BuildMemberGrowth(DateTime rangeEnd, int seed)
    {
        var list = new List<MemberGrowthSeriesPoint>();
        var endMonth = new DateTime(rangeEnd.Year, rangeEnd.Month, 1);
        var runningTotal = 1540 + (seed % 60);

        for (var i = 0; i < 7; i++)
        {
            var d = endMonth.AddMonths(-6 + i);
            var regs = 12 + (d.Month * 2 + i * 3 + seed) % 26;
            runningTotal += regs;
            list.Add(new MemberGrowthSeriesPoint
            {
                MonthLabel = d.ToString("MMM yyyy"),
                NewRegistrations = regs,
                MembershipTotal = runningTotal
            });
        }

        return list;
    }

    static IReadOnlyList<MinistryDistributionSlice> BuildMinistrySlices(int seed) =>
    [
        new() { Ministry = "Youth Ministry", MemberCount = 320 + seed % 40 },
        new() { Ministry = "Women Ministry", MemberCount = 410 + seed % 35 },
        new() { Ministry = "Men Ministry", MemberCount = 280 + seed % 30 },
        new() { Ministry = "Choir", MemberCount = 118 + seed % 20 },
        new() { Ministry = "Evangelism", MemberCount = 95 + seed % 25 },
        new() { Ministry = "Sunday School", MemberCount = 210 + seed % 28 },
        new() { Ministry = "Prayer Group", MemberCount = 156 + seed % 18 }
    ];

    static IReadOnlyList<MemberActivityGridRow> BuildActivities(DateTime start, DateTime end, int seed)
    {
        var types = new[]
        {
            "Registration", "Baptism Approval", "Marriage Approval", "Event Registration",
            "Ministry Assignment", "Counseling Request"
        };
        var ministries = new[] { "Youth", "Women", "Men", "Choir", "Evangelism", "Sunday School", "Prayer" };
        var admins = new[] { "Pastor J. Martin", "Elder A. Nkosi", "Sis. L. Dubois", "Admin C. Okafor", "Coord. M. Reyes" };
        var statuses = new[] { "Completed", "Pending", "In review", "Scheduled" };
        var firstNames = new[] { "Grace", "Daniel", "Chloe", "Samuel", "Naomi", "Isaac", "Ruth", "David", "Hannah", "Joel" };
        var lastNames = new[] { "Mbala", "Tremblay", "Osei", "Bernard", "Kumar", "Silva", "Nguyen", "Patel", "Cohen", "Adams" };

        var list = new List<MemberActivityGridRow>();
        var rnd = new Random(seed);
        for (var i = 0; i < 24; i++)
        {
            var dayOffset = rnd.Next(0, Math.Min(90, Math.Max(1, (end - start).Days + 30)));
            var dt = end.AddDays(-dayOffset).Date.AddHours(9 + rnd.Next(0, 8));
            list.Add(new MemberActivityGridRow
            {
                MemberId = $"M-{8200 + seed + i * 17:D5}",
                FullName = $"{firstNames[rnd.Next(firstNames.Length)]} {lastNames[rnd.Next(lastNames.Length)]}",
                ActivityType = types[rnd.Next(types.Length)],
                Ministry = ministries[rnd.Next(ministries.Length)],
                ActivityDate = dt,
                ResponsibleAdmin = admins[rnd.Next(admins.Length)],
                Status = statuses[rnd.Next(statuses.Length)]
            });
        }

        return list.OrderByDescending(x => x.ActivityDate).ToList();
    }

    static IReadOnlyList<ChurchEventListItem> BuildUpcomingEvents(DateTime periodEnd, int seed)
    {
        var rnd = new Random(seed ^ 0x5a5a5a5);
        var templates = new (string Title, string Ministry, string Location, string Coordinator)[]
        {
            ("Youth Conference", "Youth Ministry", "Fellowship Hall", "Pastor J. Martin"),
            ("Choir Rehearsal", "Choir", "Sanctuary balcony", "Sis. L. Dubois"),
            ("Evangelism Outreach", "Evangelism", "City park pavilion", "Elder A. Nkosi"),
            ("Marriage Seminar", "Men & Women", "Conference room A", "Pastor J. Martin"),
            ("Leadership Meeting", "Board", "Admin office", "Admin C. Okafor"),
            ("Prayer Vigil", "Prayer Group", "Main sanctuary", "Coord. M. Reyes"),
            ("Sunday School Rally", "Sunday School", "Education wing", "Sis. L. Dubois")
        };
        var list = new List<ChurchEventListItem>();
        for (var i = 0; i < 6; i++)
        {
            var t = templates[(seed + i) % templates.Length];
            var start = periodEnd.Date.AddDays(3 + i * 4 + rnd.Next(0, 3)).AddHours(18 + rnd.Next(0, 3));
            list.Add(new ChurchEventListItem
            {
                Title = t.Title,
                Ministry = t.Ministry,
                StartsAtUtc = DateTime.SpecifyKind(start, DateTimeKind.Local),
                Location = t.Location,
                Coordinator = t.Coordinator,
                Status = i % 3 == 0 ? "Confirmed" : i % 3 == 1 ? "Planning" : "Awaiting venue"
            });
        }

        return list.OrderBy(x => x.StartsAtUtc).ToList();
    }
}
