namespace LinkShortener.Api.Dtos;

public class CreateLinkRequest
{
    public string OriginalUrl { get; set; } = "";

    public string? CustomCode { get; set; }
}