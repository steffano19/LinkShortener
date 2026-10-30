namespace LinkShortener.Api.Dtos;

public class LinkResponse
{
    public string Code { get; set; } = "";
    public string ShortUrl { get; set; } = "";
    public string OriginalUrl { get; set; } = "";
}