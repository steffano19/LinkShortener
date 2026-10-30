namespace LinkShortener.Api.Models;

public class ShortLink
{
    public int Id { get; set; }

    public string Code { get; set; } = "";

    public string OriginalUrl { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public int ClicksCount { get; set; }
}