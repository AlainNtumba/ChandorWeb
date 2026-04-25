using System.Globalization;
using System.Text.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.ChurchProgram;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;

namespace AdimSystem.Data;

public sealed class CalendarDataAdaptor(IChurchProgramService churchPrograms) : DataAdaptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public override async Task<object> ReadAsync(DataManagerRequest dm, string? key = null)
    {
        var (start, end) = TryGetRange(dm);
        var response = await churchPrograms.GetUpcomingEventsAsync(start, end).ConfigureAwait(false);
        var data = response?.Data?.ToList() ?? [];

        return dm.RequiresCounts
            ? new DataResult { Result = data, Count = data.Count }
            : data;
    }

    public override async Task<object> InsertAsync(DataManager dm, object data, string key)
    {
        var item = CoerceTo<ChurchProgramDto>(data);
        var dto = new CongregationProgramDto
        {
            StartTime = item.StartTime,
            EndTime = item.EndTime,
            Theme = item.Theme,
            Lieu = item.Lieu,
            Description = item.Description,
            RecurrenceRule = item.RecurrenceRule,
            RecurrenceException = item.RecurrenceException,
            PosterLink = item.PosterLink,
            IsApproved = item.IsApproved
        };

        var response = await churchPrograms.AddCongregationProgramAsync(dto).ConfigureAwait(false);
        return response?.Data ?? item;
    }

    public override async Task<object> UpdateAsync(DataManager dm, object data, string keyField, string key)
    {
        var item = CoerceTo<ChurchProgramDto>(data);
        var response = await churchPrograms.UpdateProgramAsync(item).ConfigureAwait(false);
        return response?.Data ?? item;
    }

    public override async Task<object> RemoveAsync(DataManager dm, object data, string keyField, string key)
    {
        var id = TryGetId(data, keyField);
        if (id == Guid.Empty)
            return data;

        var response = await churchPrograms.DeleteProgramAsync(id).ConfigureAwait(false);
        return response?.Data ?? true;
    }

    private static (DateTime start, DateTime end) TryGetRange(DataManagerRequest dm)
    {
        // Scheduler passes StartDate/EndDate via params in many scenarios.
        if (dm.Params is IDictionary<string, object> p)
        {
            var start = TryGetDateTime(p, "StartDate") ?? TryGetDateTime(p, "startDate") ?? TryGetDateTime(p, "StartTime");
            var end = TryGetDateTime(p, "EndDate") ?? TryGetDateTime(p, "endDate") ?? TryGetDateTime(p, "EndTime");
            if (start is { } s && end is { } e)
                return (s, e);
        }

        // Fallback: current month window.
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
        return (monthStart, monthEnd);
    }

    private static DateTime? TryGetDateTime(IDictionary<string, object> p, string key)
    {
        if (!p.TryGetValue(key, out var v) || v is null)
            return null;

        if (v is DateTime dt)
            return dt;

        if (v is DateTimeOffset dto)
            return dto.UtcDateTime;

        if (v is string s && DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
            return parsed;

        return null;
    }

    private static T CoerceTo<T>(object data) where T : new()
    {
        if (data is T t)
            return t;

        // Syncfusion may pass JsonElement/Dictionary/etc. during CRUD.
        var json = JsonSerializer.Serialize(data, JsonOptions);
        return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();
    }

    private static Guid TryGetId(object data, string keyField)
    {
        if (data is Guid g)
            return g;

        if (data is ChurchProgramDto dto)
            return dto.Id;

        try
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty(keyField, out var prop) || doc.RootElement.TryGetProperty("Id", out prop))
                {
                    if (prop.ValueKind == JsonValueKind.String && Guid.TryParse(prop.GetString(), out var id))
                        return id;
                }
            }
        }
        catch
        {
            // ignore
        }

        return Guid.Empty;
    }
}

