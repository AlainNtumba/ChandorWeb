using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Services;
using ChandorProject.Shared.DTOs.ChurchProgram;
using ChandorProject.Shared.Models;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Globalization;
using System.Text.Json;

namespace ChandorAdmin.Data;

public sealed class CalendarDataAdaptor(
    IChurchProgramService churchPrograms,
    IDepartmentService _departmentService,
    IProgramTypeService programTypeService,
    ChurchProgramPosterEditorState posterEditor,
    ILogger<CalendarDataAdaptor> logger) : DataAdaptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Extra buffer so periodic queries include the edited occurrence.</summary>
    private static readonly TimeSpan MergeQueryPadding = TimeSpan.FromDays(14);

    public override async Task<object> ReadAsync(DataManagerRequest dataManagerRequest, string? key = null)
    {
        await Task.Delay(100);

        var (start, end) = TryGetRange(dataManagerRequest);

        var response = await churchPrograms.GetPeriodicCongregationProgramsAsync(start, end).ConfigureAwait(false);

        if (response is not null && !response.Success)
        {
            logger.LogWarning("Calendar read failed: {Message}. Errors: {Errors}", response.Message, FormatApiErrors(response));
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.Message)
                ? "Impossible de charger le calendrier."
                : response.Message!);
        }

        var data = response?.Data?.ToList() ?? [];

        return dataManagerRequest.RequiresCounts
            ? new DataResult { Result = data, Count = data.Count }
            : data;
    }

    public override async Task<object> InsertAsync(DataManager dm, object data, string key)
    {
        await Task.Delay(100);

        var item = (ChurchProgramDto)data;

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
            VideoLink = item.VideoLink,
            IsApproved = item.IsApproved
        };
        await ApplyChurchDepartmentKeysAsync(dto).ConfigureAwait(false);
        await ApplyCongregationProgramTypeAsync(dto).ConfigureAwait(false);

        var poster = posterEditor.SelectedPoster
            ?? throw new InvalidOperationException("Le poster est obligatoire pour créer un programme.");
        var response = await churchPrograms.CreateCongregationProgramAsync(dto, poster).ConfigureAwait(false);
        ThrowIfApiFailed(response);
        if (response?.Data is null)
        {
            logger.LogError("Add congregation program returned success but null data.");
            throw new InvalidOperationException("Le serveur n'a pas renvoyé l'événement créé.");
        }

        posterEditor.Reset();
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

        item.RecurrenceRule = item.RecurrenceRule ?? "";

        item.RecurrenceException = item.RecurrenceException ?? "";

        var response = await churchPrograms.UpdateProgramAsync(item).ConfigureAwait(false);

        ThrowIfApiFailed(response);

        if (response?.Data is null)
        {
            logger.LogError("Update program returned success but null data. Id: {Id}", item.Id);
            throw new InvalidOperationException("Le serveur n'a pas renvoyé l'événement mis à jour.");
        }

        var saved = response.Data;
        await ApplyPosterChangeAfterUpdateAsync(saved).ConfigureAwait(false);
        posterEditor.Reset();
        return saved;
    }

    public override async Task<object> BatchUpdateAsync(DataManager dataManager, object changedRecords, object addedRecords, object deletedRecords, string primaryColumnName, string key, int? dropIndex)
    {
        // Delete
        if (deletedRecords is List<ChurchProgramDto> deletedItems && deletedItems.Count > 0)
        {
            foreach (var item in deletedItems)
            {
                await churchPrograms.DeleteProgramAsync(item.Id).ConfigureAwait(false);
            }

            // return deletedItems;
        }

        // Add
        if (addedRecords is List<ChurchProgramDto> addedItems && addedItems.Count > 0)
        {
            foreach (var item in addedItems)
            {
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
                    VideoLink = item.VideoLink,
                    IsApproved = item.IsApproved
                };
                await ApplyChurchDepartmentKeysAsync(dto).ConfigureAwait(false);
                await ApplyCongregationProgramTypeAsync(dto).ConfigureAwait(false);

                var poster = posterEditor.SelectedPoster
                    ?? throw new InvalidOperationException("Le poster est obligatoire pour créer un programme.");
                var response = await churchPrograms.CreateCongregationProgramAsync(dto, poster).ConfigureAwait(false);
                ThrowIfApiFailed(response);
                posterEditor.Reset();
            }

            return addedItems;
        }

        // Update
        if (changedRecords is List<ChurchProgramDto> updatedItems && updatedItems.Count > 0)
        {
            foreach (var data in updatedItems)
            {
                data.RecurrenceException = data.RecurrenceException ?? "";
                data.RecurrenceRule = data.RecurrenceRule ?? "";
                var item = CoerceTo<ChurchProgramDto>(data);
                var response = await churchPrograms.UpdateProgramAsync(item).ConfigureAwait(false);
                ThrowIfApiFailed(response);
                if (response?.Data is null)
                    throw new InvalidOperationException("Le serveur n'a pas renvoyé l'événement mis à jour.");
                await ApplyPosterChangeAfterUpdateAsync(response.Data).ConfigureAwait(false);
                posterEditor.Reset();
            }

            return updatedItems;
        }

        return null!;
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

        var response = await churchPrograms.DeleteProgramAsync((Guid)id).ConfigureAwait(false);
        ThrowIfApiFailed(response);
        return true;
    }

    private async Task ApplyChurchDepartmentKeysAsync(CongregationProgramDto dto)
    {
        var keys = await _departmentService.GetChurchDepartmentKeysAsync().ConfigureAwait(false);
        ThrowIfApiFailed(keys);
        if (keys?.Data is null)
        {
            logger.LogError("Church department keys returned success but null data.");
            throw new InvalidOperationException("Le serveur n'a pas renvoyé le département de l'église.");
        }

        dto.DepartmentId = keys.Data.DepartmentId;
        dto.DepartmentTeamId = keys.Data.DepartmentTeamId;
    }

    private async Task ApplyCongregationProgramTypeAsync(CongregationProgramDto dto)
    {
        var types = await programTypeService.GetAllProgramTypesAsync().ConfigureAwait(false);
        ThrowIfApiFailed(types);

        var congregation = types?.Data?.FirstOrDefault(type =>
            string.Equals(type.Name, "Congrégation", StringComparison.OrdinalIgnoreCase));
        if (congregation is null || congregation.Id == Guid.Empty)
        {
            logger.LogError("Program type 'Congrégation' was not found.");
            throw new InvalidOperationException("Le type de programme Congrégation est introuvable.");
        }

        dto.ProgramTypeId = congregation.Id;
    }

    private async Task ApplyPosterChangeAfterUpdateAsync(ChurchProgramDto program)
    {
        if (posterEditor.SelectedPoster is { } poster)
        {
            var posterResponse = await churchPrograms
                .AddOrReplacePosterAsync(program.Id, poster)
                .ConfigureAwait(false);
            if (posterResponse is null || !posterResponse.Success)
            {
                throw new InvalidOperationException(
                    "Le programme a été modifié, mais le nouveau poster n’a pas pu être enregistré. Vous pouvez réessayer l’envoi du poster.");
            }

            program.PosterLink = posterResponse.Data?.PosterLink ?? program.PosterLink;
        }
        else if (posterEditor.DeleteExistingPoster)
        {
            var deleteResponse = await churchPrograms.DeletePosterAsync(program.Id).ConfigureAwait(false);
            if (deleteResponse is null || !deleteResponse.Success)
            {
                throw new InvalidOperationException(
                    "Le programme a été modifié, mais le poster n’a pas pu être supprimé. Vous pouvez réessayer la suppression.");
            }

            program.PosterLink = string.Empty;
        }
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
