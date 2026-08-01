using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.ChurchProgram;
using ChandorProject.Shared.Models;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Text.Json;

namespace ChandorAdmin.Data;

public sealed class DepartmentCalendarDataAdaptor(
    IChurchProgramService churchPrograms,
    ILogger<DepartmentCalendarDataAdaptor> logger) : DataAdaptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Set by <see cref="Components.DepartmentManagement.DepartmentCalendar"/> before the schedule loads.
    /// Scoped instance property (not static) so department context stays request/circuit-local.
    /// </summary>
    public Guid DepartmentId { get; set; }

    public override async Task<object> ReadAsync(DataManagerRequest dataManagerRequest, string? key = null)
    {
        await Task.Delay(100).ConfigureAwait(false);

        if (DepartmentId == Guid.Empty)
        {
            logger.LogWarning("Department calendar read skipped: DepartmentId is empty.");
            return dataManagerRequest.RequiresCounts
                ? new DataResult { Result = Array.Empty<ChurchProgramDto>(), Count = 0 }
                : Array.Empty<ChurchProgramDto>();
        }

        var response = await churchPrograms.GetDepartmentProgramAsync(DepartmentId).ConfigureAwait(false);

        if (response is not null && !response.Success)
        {
            logger.LogWarning(
                "Department calendar read failed for {DepartmentId}: {Message}. Errors: {Errors}",
                DepartmentId,
                response.Message,
                FormatApiErrors(response));
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(response.Message)
                ? "Impossible de charger le calendrier du département."
                : response.Message!);
        }

        var data = response?.Data?.ToList() ?? [];

        return dataManagerRequest.RequiresCounts
            ? new DataResult { Result = data, Count = data.Count }
            : data;
    }

    public override async Task<object> InsertAsync(DataManager dm, object data, string key)
    {
        await Task.Delay(100).ConfigureAwait(false);

        EnsureDepartmentId();

        var item = CoerceTo<ChurchProgramDto>(data);
        var dto = ToNewProgramDto(item);

        var response = await churchPrograms.AddProgramAsync(dto).ConfigureAwait(false);
        ThrowIfApiFailed(response);
        if (response?.Data is null)
        {
            logger.LogError("Add department program returned success but null data. DepartmentId: {DepartmentId}", DepartmentId);
            throw new InvalidOperationException("Le serveur n'a pas renvoyé l'événement créé.");
        }

        return response.Data;
    }

    public override async Task<object> UpdateAsync(DataManager dm, object data, string keyField, string key)
    {
        await Task.Delay(100).ConfigureAwait(false);

        var item = CoerceTo<ChurchProgramDto>(data);

        if (item.Id == Guid.Empty && !string.IsNullOrWhiteSpace(key) && Guid.TryParse(key, out var keyId))
            item.Id = keyId;
        if (item.Id == Guid.Empty)
        {
            logger.LogError("Update rejected: event Id is empty after coercion. Key: {Key}, keyField: {KeyField}.", key, keyField);
            throw new InvalidOperationException("Identifiant d'événement manquant; impossible d'enregistrer les changements.");
        }

        EnsureDepartmentId();
        item.DepartmentId = DepartmentId;
        item.RecurrenceRule = item.RecurrenceRule ?? "";
        item.RecurrenceException = item.RecurrenceException ?? "";

        var response = await churchPrograms.UpdateProgramAsync(item).ConfigureAwait(false);
        ThrowIfApiFailed(response);

        if (response?.Data is null)
        {
            logger.LogError("Update program returned success but null data. Id: {Id}", item.Id);
            throw new InvalidOperationException("Le serveur n'a pas renvoyé l'événement mis à jour.");
        }

        return data;
    }

    public override async Task<object> BatchUpdateAsync(
        DataManager dataManager,
        object changedRecords,
        object addedRecords,
        object deletedRecords,
        string primaryColumnName,
        string key,
        int? dropIndex)
    {
        // Delete
        if (deletedRecords is List<ChurchProgramDto> deletedItems && deletedItems.Count > 0)
        {
            foreach (var item in deletedItems)
                await churchPrograms.DeleteProgramAsync(item.Id).ConfigureAwait(false);
        }

        // Add
        // NOTE: Spec asked to use GetDepartmentProgramAsync for addedRecords, but that method is read-only
        // (GET department programmes). Persist via AddProgramAsync — the same write path as InsertAsync.
        if (addedRecords is List<ChurchProgramDto> addedItems && addedItems.Count > 0)
        {
            EnsureDepartmentId();
            foreach (var item in addedItems)
            {
                var dto = ToNewProgramDto(item);
                await churchPrograms.AddProgramAsync(dto).ConfigureAwait(false);
            }

            return addedItems;
        }

        // Update
        if (changedRecords is List<ChurchProgramDto> updatedItems && updatedItems.Count > 0)
        {
            EnsureDepartmentId();
            foreach (var data in updatedItems)
            {
                data.RecurrenceException = data.RecurrenceException ?? "";
                data.RecurrenceRule = data.RecurrenceRule ?? "";
                data.DepartmentId = DepartmentId;
                var item = CoerceTo<ChurchProgramDto>(data);
                await churchPrograms.UpdateProgramAsync(item).ConfigureAwait(false);
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

        var response = await churchPrograms.DeleteProgramAsync(id).ConfigureAwait(false);
        ThrowIfApiFailed(response);
        return true;
    }

    private NewChurchProgramDto ToNewProgramDto(ChurchProgramDto item) =>
        new()
        {
            StartTime = item.StartTime,
            EndTime = item.EndTime,
            Theme = item.Theme,
            Lieu = item.Lieu,
            Description = item.Description,
            RecurrenceRule = item.RecurrenceRule ?? "",
            RecurrenceException = item.RecurrenceException ?? "",
            PosterLink = item.PosterLink ?? "",
            IsApproved = item.IsApproved,
            ProgramTypeId = item.ProgramTypeId,
            DepartmentId = DepartmentId,
            DepartmentTeamId = item.DepartmentTeamId
        };

    private void EnsureDepartmentId()
    {
        if (DepartmentId != Guid.Empty)
            return;

        logger.LogError("Department calendar write rejected: DepartmentId is empty.");
        throw new InvalidOperationException("Identifiant de département manquant; impossible d'enregistrer le programme.");
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
