namespace ChandorAdmin.Models.Member;

public sealed record MemberProfileImageUpload(
    string FileName,
    string ContentType,
    byte[] Content);
