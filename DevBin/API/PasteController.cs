#nullable disable
using DevBin.Attributes;
using DevBin.Data;
using DevBin.UserModels;
using DevBin.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevBin.API
{
    [Route("api/v3/[controller]")]
    [ApiController]
    [RequireApiKey(ApiPermission.None)]
    public class PasteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public PasteController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;

            PasteSpace = User != null && signInManager.IsSignedIn(User) ? _configuration.GetValue<int>("Paste:MaxContentSize:Member") : _configuration.GetValue<int>("Paste:MaxContentSize:Guest", 1024 * 2);
        }

        public int PasteSpace { get; set; }

        /// <summary>
        /// Get information about a paste 
        /// </summary>
        /// <param name="code">Paste code</param>
        /// <returns></returns>
        [HttpGet("{code}")]
        [RequireApiKey(ApiPermission.Get)]
        public async Task<ActionResult<ResultPaste>> GetPaste(string code)
        {
            var paste = await _context.Pastes.FirstOrDefaultAsync(q => q.Code == code);
            if (paste == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (paste.Author != null && paste.Exposure.IsAuthorOnly && paste.AuthorId != user.Id)
            {
                return NotFound();
            }

            return ResultPaste.From(paste);
        }

        /// <summary>
        /// Upload a new paste
        /// </summary>
        /// <param name="userPaste">Filled paste</param>
        /// <returns></returns>
        [HttpPost]
        [RequireApiKey(ApiPermission.Create)]
        public async Task<ActionResult<ResultPaste>> UploadPaste(UserPaste userPaste)
        {
            if (userPaste.Content.Length > PasteSpace)
                return BadRequest("Maximum content length exceeded.");

            var paste = new Paste
            {
                Title = userPaste.Title ?? "Unnamed Paste",
                Cache = PasteUtils.GetShortContent(userPaste.Content, 250),
                Content = userPaste.Content,
                DateTime = DateTime.UtcNow,
                UploaderIPAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
                Views = 0,
            };

            var user = await _userManager.GetUserAsync(User);
            if (await _context.Syntaxes.AnyAsync(q => q.Name == userPaste.SyntaxName))
                paste.SyntaxName = userPaste.SyntaxName;

            if (await _context.Exposures.AnyAsync(q => q.Id == userPaste.ExposureId))
                paste.ExposureId = userPaste.ExposureId;

            string code;
            do
            {
                code = PasteUtils.GenerateRandomCode(_configuration.GetValue<int>("Paste:CodeLength"));
            } while (await _context.Pastes.AnyAsync(q => q.Code.ToLower() == code.ToLower()));

            paste.Code = code;

            if (!userPaste.AsGuest.Value)
            {
                paste.AuthorId = user.Id;
                if (userPaste.FolderId.HasValue)
                {
                    if (_context.Folders.Any(q => q.Id == userPaste.FolderId && q.OwnerId == user.Id))
                        paste.FolderId = userPaste.FolderId.Value;
                }
            }

            _context.Pastes.Add(paste);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPaste", new { code = paste.Code }, ResultPaste.From(paste));
        }

        /// <summary>
        /// Update information and/or content of your paste
        /// </summary>
        /// <param name="code">Paste code</param>
        /// <param name="userPaste">Updated parameters</param>
        /// <returns></returns>
        [HttpPatch("{code}")]
        [RequireApiKey(ApiPermission.Update)]
        public async Task<IActionResult> UpdatePaste(string code, UserPaste userPaste)
        {
            if (userPaste.Content.Length > PasteSpace)
                return BadRequest("Maximum content length exceeded.");

            var paste = await _context.Pastes.FirstOrDefaultAsync(q => q.Code == code);
            if (paste == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (paste.Author != null && paste.Exposure.IsAuthorOnly && paste.AuthorId != user.Id)
            {
                return NotFound();
            }

            if (paste.AuthorId != user.Id)
                return Unauthorized();

            paste.UpdateDatetime = DateTime.UtcNow;
            paste.Content = userPaste.Content ?? paste.Content;
            paste.Title = userPaste.Title ?? paste.Title;

            if (await _context.Syntaxes.AnyAsync(q => q.Name == userPaste.SyntaxName))
                paste.SyntaxName = userPaste.SyntaxName;

            if (await _context.Exposures.AnyAsync(q => q.Id == userPaste.ExposureId))
                paste.ExposureId = userPaste.ExposureId;

            if (userPaste.FolderId != 0 && userPaste.FolderId != null && user.Folders.Any(q => q.Id == userPaste.FolderId))
                paste.FolderId = userPaste.FolderId;

            if (userPaste.FolderId == 0)
                paste.FolderId = null;

            paste.Cache = PasteUtils.GetShortContent(paste.Content, 250);

            _context.Entry(paste).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PasteExists(code))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPaste", new { code = paste.Code }, ResultPaste.From(paste));
        }

        /// <summary>
        /// Delete your own paste
        /// </summary>
        /// <param name="code">Paste code</param>
        /// <returns></returns>
        [HttpDelete("{code}")]
        [RequireApiKey(ApiPermission.Delete)]
        public async Task<IActionResult> DeletePaste(string code)
        {
            var paste = await _context.Pastes.FirstOrDefaultAsync(q => q.Code == code);
            if (paste == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (paste.AuthorId != user.Id)
                return Unauthorized();

            _context.Pastes.Remove(paste);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PasteExists(string code)
        {
            return _context.Pastes.Any(e => e.Code == code);
        }
    }
}
