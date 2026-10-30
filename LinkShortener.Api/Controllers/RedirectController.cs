using LinkShortener.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkShortener.Api.Controllers;

[ApiController]
public class RedirectController : ControllerBase
{
    private readonly AppDbContext _db;

    public RedirectController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> RedirectToOriginal(string code)
    {
        var link = await _db.ShortLinks.FirstOrDefaultAsync(l => l.Code == code);

        if (link == null)
        {
            return NotFound("Ссылка не найдена");
        }

        link.ClicksCount = link.ClicksCount + 1;
        await _db.SaveChangesAsync();

        return Redirect(link.OriginalUrl);
    }
}