using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data;

namespace PentestHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KnowledgeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public KnowledgeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublishedArticles()
    {
        var articles = await _context.KnowledgeBases
            .Where(k => k.IsPublished)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new
            {
                k.KnowledgeBaseId,
                k.Title,
                k.Category,
                k.Content,
                k.Tags,
                k.CreatedAt,
                k.UpdatedAt
            })
            .ToListAsync();

        return Ok(articles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetArticle(int id)
    {
        var article = await _context.KnowledgeBases
            .Where(k => k.KnowledgeBaseId == id && k.IsPublished)
            .Select(k => new
            {
                k.KnowledgeBaseId,
                k.Title,
                k.Category,
                k.Content,
                k.Tags,
                k.CreatedAt,
                k.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (article == null)
        {
            return NotFound();
        }

        return Ok(article);
    }
}

