#nullable disable
using DevBin.Attributes;
using DevBin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevBin.API;

[Route("api/v3/[controller]")]
[ApiController]
[RequireApiKey(ApiPermission.None)]
public class DataController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DataController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Complete list of available exposures
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("exposures")]
    public async Task<ActionResult<IEnumerable<Exposure>>> GetExposures()
    {
        return await _context.Exposures.ToListAsync();
    }

    /// <summary>
    /// Complete list of available syntaxes
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("syntaxes")]
    public async Task<ActionResult<IEnumerable<Syntax>>> GetSyntaxes()
    {
        return await _context.Syntaxes.ToListAsync();
    }
}
