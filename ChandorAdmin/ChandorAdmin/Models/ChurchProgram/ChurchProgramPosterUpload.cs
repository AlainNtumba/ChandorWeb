namespace ChandorAdmin.Models.ChurchProgram;

public sealed record ChurchProgramPosterUpload(
    string FileName,
    string ContentType,
    byte[] Content);
