namespace ChandorAdmin.Models.ChurchDirectory;

public sealed record DirectoryImageUpload(string FileName, string ContentType, byte[] Content);
