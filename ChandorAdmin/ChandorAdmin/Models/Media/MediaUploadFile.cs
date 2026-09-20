namespace ChandorAdmin.Models.Media;

public sealed record MediaUploadFile(string FileName, string ContentType, byte[] Content);
