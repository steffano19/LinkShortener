using LinkShortener.Api.Data;
using LinkShortener.Api.Dtos;
using LinkShortener.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkShortener.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LinksController : ControllerBase
{
    private readonly AppDbContext _db;

    
    public LinksController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLinkRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OriginalUrl))
        {
            return BadRequest("Ссылка не может быть пустой");
        }

        bool isUrl = Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out Uri? uri);
        if (!isUrl || uri == null || (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            return BadRequest("Нужна корректная ссылка, которая начинается с http:// или https://");
        }

        string code = await GenerateUniqueCode();

        var link = new ShortLink();
        link.Code = code;
        link.OriginalUrl = request.OriginalUrl;
        link.CreatedAt = DateTime.UtcNow;
        link.ClicksCount = 0;

        _db.ShortLinks.Add(link);
        await _db.SaveChangesAsync();

        var response = new LinkResponse();
        response.Code = link.Code;
        response.OriginalUrl = link.OriginalUrl;
        response.ShortUrl = $"{Request.Scheme}://{Request.Host}/{link.Code}";

        return Created(response.ShortUrl, response);
    }

    [HttpGet("{code}/stats")]
    public async Task<IActionResult> GetStats(string code)
    {
        var link = await _db.ShortLinks.FirstOrDefaultAsync(l => l.Code == code);

        if (link == null)
        {
            return NotFound("Ссылка не найдена");
        }

        var response = new StatsResponse();
        response.Code = link.Code;
        response.OriginalUrl = link.OriginalUrl;
        response.CreatedAt = link.CreatedAt;
        response.ClicksCount = link.ClicksCount;

        return Ok(response);
    }

    private async Task<string> GenerateUniqueCode()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        while (true)
        {
            string code = "";
            for (int i = 0; i < 6; i++)
            {
                int index = Random.Shared.Next(chars.Length);
                code = code + chars[index];
            }

            bool alreadyExists = await _db.ShortLinks.AnyAsync(l => l.Code == code);
            if (!alreadyExists)
            {
                return code;
            }
        }
    }
}