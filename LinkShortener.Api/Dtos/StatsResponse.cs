namespace LinkShortener.Api.Dtos;

public class StatsResponse
{
    public string Code { get; set; } = "";
    public string OriginalUrl { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int ClicksCount { get; set; }
}