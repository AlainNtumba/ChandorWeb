using System.Net.Http.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using ChandorAdmin.Helpers;
using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.ChurchProgram;
using ChandorProject.Shared.DTOs.ChurchProgram;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class ChurchProgramService(ChandorApiHttp api) : IChurchProgramService
{
    private const string C = "ChurchProgram";

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetCongregationProgramsAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get_congration_programs", cancellationToken);

    public Task<DataResponse<ChurchProgramDto>?> AddProgramAsync(NewChurchProgramDto dto, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<ChurchProgramDto>($"{C}/add-church-program", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<ChurchProgramDto>?> UpdateProgramAsync(ChurchProgramDto dto, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<ChurchProgramDto>($"{C}/update-church-program", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<bool>?> DeleteProgramAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete_church_program/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetDepartmentProgramAsync(
        Guid departmentId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        var q =
            $"departmentId={departmentId}" +
            $"&start={ApiDateQueryFormatter.FormatQueryValue(start, asUtc: true)}" +
            $"&end={ApiDateQueryFormatter.FormatQueryValue(end, asUtc: true)}";
        return api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get-department-program?{q}", cancellationToken);
    }

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetTeamProgramAsync(Guid teamId, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get-team-program/{teamId}", cancellationToken);

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetUpcomingEventsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var q = $"start={ApiDateQueryFormatter.FormatQueryValue(start, asUtc: true)}&end={ApiDateQueryFormatter.FormatQueryValue(end, asUtc: true)}";
        return api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get-upcoming-events?{q}", cancellationToken);
    }

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetPeriodicCongregationProgramsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var q = $"start={ApiDateQueryFormatter.FormatQueryValue(start, asUtc: true)}&end={ApiDateQueryFormatter.FormatQueryValue(end, asUtc: true)}";
        return api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get_periodic_congration_programs?{q}", cancellationToken);
    }

    public async Task<DataResponse<ChurchProgramDto>?> AddCongregationProgramAsync(
        CongregationProgramDto dto,
        ChurchProgramPosterUpload poster,
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        AddText(form, "startTime", dto.StartTime.ToString("O", CultureInfo.InvariantCulture));
        AddText(form, "endTime", dto.EndTime.ToString("O", CultureInfo.InvariantCulture));
        AddText(form, "theme", dto.Theme);
        AddText(form, "lieu", dto.Lieu);
        AddText(form, "description", dto.Description);
        AddText(form, "recurrenceRule", dto.RecurrenceRule);
        AddText(form, "recurrenceException", dto.RecurrenceException);
        AddText(form, "videoLink", dto.VideoLink);
        AddText(form, "isApproved", dto.IsApproved.ToString().ToLowerInvariant());
        AddText(form, "programTypeId", dto.ProgramTypeId.ToString("D"));
        AddText(form, "departmentId", dto.DepartmentId.ToString("D"));
        AddText(form, "departmentTeamId", dto.DepartmentTeamId.ToString("D"));
        AddPoster(form, poster);

        return await api.PostMultipartDataResponseAsync<ChurchProgramDto>(
            $"{C}/add-congregation-program",
            form,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<DataResponse<ChurchProgramDto>?> AddOrReplacePosterAsync(
        Guid programId,
        ChurchProgramPosterUpload poster,
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        AddPoster(form, poster);
        return await api.PutDataResponseAsync<ChurchProgramDto>(
            $"{C}/{programId:D}/poster",
            form,
            cancellationToken).ConfigureAwait(false);
    }

    public Task<DataResponse<bool>?> DeletePosterAsync(Guid programId, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/{programId:D}/poster", cancellationToken);

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetPaginatedCongregationProgramsFeedAsync(
        DateTime? fromDate,
        DateTime? toDate,
        int take = 10,
        int skip = 0,
        CancellationToken cancellationToken = default)
    {
        var parts = new List<string> { $"take={take}", $"skip={skip}" };
        if (fromDate is { } fd)
            parts.Add($"fromDate={ApiDateQueryFormatter.FormatQueryValue(fd, asUtc: true)}");
        if (toDate is { } td)
            parts.Add($"toDate={ApiDateQueryFormatter.FormatQueryValue(td, asUtc: true)}");
        return api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get_paginatedfeed_congration_programs?{string.Join("&", parts)}", cancellationToken);
    }

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetCongregationProgramsByKeywordAsync(
        string keyword,
        int take = 10,
        int skip = 0,
        CancellationToken cancellationToken = default)
    {
        var q = $"keyword={Uri.EscapeDataString(keyword ?? string.Empty)}&take={take}&skip={skip}";
        return api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get_congration_programs_bykeyword?{q}", cancellationToken);
    }

    private static void AddText(MultipartFormDataContent form, string name, string? value)
        => form.Add(new StringContent(value ?? string.Empty, Encoding.UTF8), name);

    private static void AddPoster(MultipartFormDataContent form, ChurchProgramPosterUpload poster)
    {
        var fileContent = new ByteArrayContent(poster.Content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(poster.ContentType);
        form.Add(fileContent, "posterLink", poster.FileName);
    }
}
