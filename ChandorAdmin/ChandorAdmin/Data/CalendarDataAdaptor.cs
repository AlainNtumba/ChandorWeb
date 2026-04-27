using System.Globalization;
using System.Text.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.ChurchProgram;
using ChandorProject.Shared.Models;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;

namespace ChandorAdmin.Data;

public sealed class CalendarDataAdaptor(
    IChurchProgramService churchPrograms,
    ILogger<CalendarDataAdaptor> logger) : DataAdaptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Extra buffer so periodic queries include the edited occurrence.</summary>
    private static readonly TimeSpan MergeQueryPadding = TimeSpan.FromDays(14);

    public override async Task<object> ReadAsync(DataManagerRequest dm, string? key = null)
    {
        await Task.Delay(100);
        var (start, end) = TryGetRange(dm);
        var response = await churchPrograms.GetPeriodicCongregationProgramsAsync(start, end).ConfigureAwait(false);
        if (response is not null && !response.Success)
        {
            logger.LogWarning("Calendar read failed: {Message}. Errors: {Errors}", response.Message, FormatApiErrors(response));
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.Message)
                ? "Impossible de charger le calendrier."
                : response.Message!);
        }

        var data = response?.Data?.ToList() ?? [];

        return dm.RequiresCounts
            ? new DataResult { Result = data, Count = data.Count }
            : data;
    }

    public override async Task<object> InsertAsync(DataManager dm, object data, string key)
    {
        await Task.Delay(100); 

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
        ThrowIfApiFailed(response);
        if (response?.Data is null)
        {
            logger.LogError("Add congregation program returned success but null data.");
            throw new InvalidOperationException("Le serveur n'a pas renvoyé l'événement créé.");
        }

        return response.Data;
    }

    public override async Task<object> UpdateAsync(DataManager dm, object data, string keyField, string key)
    {
        await Task.Delay(100); 

        var item = CoerceTo<ChurchProgramDto>(data);
        if (item.Id == Guid.Empty && !string.IsNullOrWhiteSpace(key) && Guid.TryParse(key, out var keyId))
            item.Id = keyId;
        if (item.Id == Guid.Empty)
        {
            logger.LogError("Update rejected: event Id is empty after coercion. Key: {Key}, keyField: {KeyField}.", key, keyField);
            throw new InvalidOperationException("Identifiant d'événement manquant; impossible d'enregistrer les changements.");
        }

        var merged = await MergeWithServerCopyAsync(item).ConfigureAwait(false);
        var response = await churchPrograms.UpdateProgramAsync(merged).ConfigureAwait(false);
        ThrowIfApiFailed(response);
        if (response?.Data is null)
        {
            logger.LogError("Update program returned success but null data. Id: {Id}", item.Id);
            throw new InvalidOperationException("Le serveur n'a pas renvoyé l'événement mis à jour.");
        }

        return data;
    }

    public override async Task<object> RemoveAsync(DataManager dm, object data, string keyField, string key)
    {
        var id = TryGetId(data, keyField, key);
        if (id == Guid.Empty)
        {
            logger.LogError(
                "Delete rejected: could not resolve event id. keyField: {KeyField}, key: {Key}, dataType: {Type}",
                keyField, key, data?.GetType().FullName ?? "null");
            throw new InvalidOperationException("Identifiant d'événement introuvable; suppression impossible.");
        }

        var response = await churchPrograms.DeleteProgramAsync(id).ConfigureAwait(false);
        ThrowIfApiFailed(response);
        return true;
    }

    private static void ThrowIfApiFailed<T>(DataResponse<T>? response)
    {
        if (response is not null && response.Success)
            return;

        if (response is null)
        {
            throw new InvalidOperationException(
                "Aucune réponse du serveur (connectivité, session ou format de réponse inattendu).");
        }

        var message = string.IsNullOrWhiteSpace(response.Message)
            ? "L'opération a échoué."
            : response.Message;
        var detail = string.Join(" ", (response.Error ?? []).Where(s => !string.IsNullOrWhiteSpace(s))!);
        if (!string.IsNullOrEmpty(detail))
            message = $"{message} {detail}";

        throw new InvalidOperationException(message);
    }

    private static string FormatApiErrors<T>(DataResponse<T> response)
    {
        if (response.Error is null)
            return string.Empty;
        return string.Join(" | ", response.Error.Where(s => !string.IsNullOrWhiteSpace(s))!);
    }

    private async Task<ChurchProgramDto> MergeWithServerCopyAsync(ChurchProgramDto edited, CancellationToken cancellationToken = default)
    {
        var start = (edited.StartTime - MergeQueryPadding);
        var end = (edited.EndTime > edited.StartTime ? edited.EndTime : edited.StartTime) + MergeQueryPadding;
        if (end <= start)
            end = start + TimeSpan.FromDays(1);

        var response = await churchPrograms
            .GetPeriodicCongregationProgramsAsync(start, end, cancellationToken)
            .ConfigureAwait(false);
        if (response is not { Success: true, Data: not null })
            return edited;

        var existing = response.Data.FirstOrDefault(p => p.Id == edited.Id);
        if (existing is null)
            return edited;

        if (string.IsNullOrWhiteSpace(edited.Theme) && !string.IsNullOrWhiteSpace(existing.Theme))
            edited.Theme = existing.Theme;
        if (string.IsNullOrWhiteSpace(edited.Lieu) && !string.IsNullOrWhiteSpace(existing.Lieu))
            edited.Lieu = existing.Lieu;
        if (string.IsNullOrWhiteSpace(edited.Description) && !string.IsNullOrWhiteSpace(existing.Description))
            edited.Description = existing.Description;
        if (string.IsNullOrWhiteSpace(edited.RecurrenceRule) && !string.IsNullOrWhiteSpace(existing.RecurrenceRule))
            edited.RecurrenceRule = existing.RecurrenceRule;
        if (string.IsNullOrWhiteSpace(edited.RecurrenceException) && !string.IsNullOrWhiteSpace(existing.RecurrenceException))
            edited.RecurrenceException = existing.RecurrenceException;
        if (string.IsNullOrWhiteSpace(edited.PosterLink) && !string.IsNullOrWhiteSpace(existing.PosterLink))
            edited.PosterLink = existing.PosterLink;

        if (edited.ProgramTypeId == Guid.Empty)
            edited.ProgramTypeId = existing.ProgramTypeId;
        if (edited.DepartmentId == Guid.Empty)
            edited.DepartmentId = existing.DepartmentId;
        if (edited.DepartmentTeamId == Guid.Empty)
            edited.DepartmentTeamId = existing.DepartmentTeamId;

        if (edited.StartTime == default)
            edited.StartTime = existing.StartTime;
        if (edited.EndTime == default)
            edited.EndTime = existing.EndTime;

        return edited;
    }

    private static (DateTime start, DateTime end) TryGetRange(DataManagerRequest dm)
    {
        if (dm.Params is IDictionary<string, object> p)
        {
            var start = TryGetDateTime(p, "StartDate") ?? TryGetDateTime(p, "startDate") ?? TryGetDateTime(p, "StartTime");
            var end = TryGetDateTime(p, "EndDate") ?? TryGetDateTime(p, "endDate") ?? TryGetDateTime(p, "EndTime");
            if (start is { } s && end is { } e)
                return (s, e);
        }

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
        return (monthStart, monthEnd);
    }

    private static DateTime? TryGetDateTime(IDictionary<string, object> p, string name)
    {
        if (!p.TryGetValue(name, out var v) || v is null)
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

        var json = JsonSerializer.Serialize(data, JsonOptions);
        return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();
    }

    private static Guid TryGetId(object? data, string keyField, string? key)
    {
        if (data is Guid g)
            return g;

        if (data is string s && Guid.TryParse(s, out var fromString))
            return fromString;

        if (!string.IsNullOrWhiteSpace(key) && Guid.TryParse(key, out var fromKey))
            return fromKey;

        if (data is ChurchProgramDto dto && dto.Id != Guid.Empty)
            return dto.Id;

        try
        {
            if (data is null)
                return Guid.Empty;

            var json = JsonSerializer.Serialize(data, JsonOptions);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object
                && TryGetGuidFromJsonObject(doc.RootElement, keyField, out var fromJson))
                return fromJson;
        }
        catch
        {
            // Malformed payload — leave empty so caller can throw a clear error.
        }

        return Guid.Empty;
    }

    private static bool TryGetGuidFromJsonObject(JsonElement obj, string? keyField, out Guid id)
    {
        id = Guid.Empty;
        if (obj.ValueKind != JsonValueKind.Object)
            return false;

        var tryNames = new[] { keyField, "Id", "id", "ID" };
        foreach (var n in tryNames)
        {
            if (string.IsNullOrEmpty(n))
                continue;
            if (obj.TryGetProperty(n, out var el) && el.ValueKind == JsonValueKind.String && Guid.TryParse(el.GetString(), out id))
                return true;
        }

        foreach (var p in obj.EnumerateObject())
        {
            if (!p.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                continue;
            if (p.Value.ValueKind == JsonValueKind.String && Guid.TryParse(p.Value.GetString(), out id))
                return true;
        }

        return false;
    }
}
