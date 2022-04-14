using DevBin.Attributes;
using DevBin.Data;
using DevBin.UserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevBin.API
{
    [Route("api/v3/[controller]")]
    [ApiController]
    [RequireApiKey(ApiPermission.None)]
    public class MeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Get all owned pastes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Pastes")]
        public async Task<ActionResult<IEnumerable<ResultPaste>>> GetPastes()
        {
            var user = await _userManager.GetUserAsync(User);
            return user.Pastes.Select(x => ResultPaste.From(x)).ToList();
        }


        /// <summary>
        /// Get all owned folders
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Folders")]
        public async Task<ActionResult<IEnumerable<ResultFolder>>> GetFolders()
        {
            var user = await _userManager.GetUserAsync(User);
            return user.Folders.Select(x => ResultFolder.From(x)).ToList();
        }

        /// <summary>
        /// Get information about a folder
        /// </summary>
        /// <param name="id">Folder ID</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Folder/{id}")]
        public async Task<ActionResult<ResultFolder>> GetFolder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var folder = user.Folders.FirstOrDefault(q => q.Id == id && q.OwnerId == user.Id);
            if(folder == null)
                return NotFound();

            return ResultFolder.From(folder);
        }

        /// <summary>
        /// Create a new folder
        /// </summary>
        /// <param name="userFolder">Folder data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Folder")]
        [RequireApiKey(ApiPermission.CreateFolder)]
        public async Task<ActionResult<ResultFolder>> CreateFolder(UserFolder userFolder)
        {
            var user = await _userManager.GetUserAsync(User);

            var folder = new Folder
            {
                Name = userFolder.Name,
                OwnerId = user.Id,
                DateTime = DateTime.UtcNow,
            };
            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFolder", new { id = folder.Id }, ResultFolder.From(folder));
        }


        /// <summary>
        /// Delete a folder
        /// </summary>
        /// <param name="id">Folder ID</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Folder/{id}")]
        [RequireApiKey(ApiPermission.DeleteFolder)]
        public async Task<ActionResult<ResultFolder>> DeleteFolder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var folder = user.Folders.FirstOrDefault(q => q.Id == id && q.OwnerId == user.Id);
            if (folder == null)
                return NotFound();

            _context.Remove(folder);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        /// <summary>
        /// Get information about the token in use
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetToken")]
        [RequireApiKey(ApiPermission.None)]
        public async Task<ActionResult<ResultToken>> GetToken()
        {
            var tokenKey = HttpContext.Request.Headers.Authorization.ToString();
            var token = await _context.ApiTokens.Where(q => q.Token == tokenKey).FirstOrDefaultAsync();
            if (token == null)
                return BadRequest("Please contact devbin@alexdevs.me immediately!");

            return ResultToken.From(token);
        }
    }
}
