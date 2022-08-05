using DevBin.Attributes;
using DevBin.Data;
using DevBin.UserModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DevBin.API;

[Route("api/v3/[controller]")]
[ApiController]
[RequireApiKey(ApiPermission.None)]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public UserController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }

    /// <summary>
    /// Get information about a user
    /// </summary>
    /// <param name="username">User's username</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{username}")]
    public async Task<ActionResult<ResultUser>> GetUser(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return NotFound();

        return new ResultUser
        {
            Username = user.UserName,
        };
    }

    /// <summary>
    /// Get the user's public list of pastes
    /// </summary>
    /// <param name="username">User's username</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{username}/Pastes")]
    [RequireApiKey(ApiPermission.GetUser)]
    public async Task<ActionResult<IEnumerable<ResultPaste>>> GetUserPastes(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return NotFound();


        return user.Pastes.Where(q => q.Exposure.IsListed).Select(x => ResultPaste.From(x)).ToList();
    }

    /// <summary>
    /// Get the user's public list of folders
    /// </summary>
    /// <param name="username">User's username</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{username}/folders")]
    [RequireApiKey(ApiPermission.GetUser)]
    public async Task<ActionResult<IEnumerable<ResultFolder>>> GetUserFolders(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return NotFound();

        return user.Folders.Where(q => q.Pastes.Any(x => x.Exposure.IsListed)).Select(x => ResultFolder.From(x)).ToList();
    }
}
