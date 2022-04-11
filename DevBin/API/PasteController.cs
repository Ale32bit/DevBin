#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Authorization;
using DevBin.Attributes;
using DevBin.UserModels;
using Microsoft.AspNetCore.Identity;
using DevBin.Utils;

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
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }


        // GET: api/Paste/5
        [HttpGet("{code}")]
        [RequireApiKey(ApiPermission.Get)]
        public async Task<ActionResult<UserPaste>> GetPaste(string code)
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

            return UserPaste.From(paste);
        }

        // PUT: api/Paste/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPatch("{code}")]
        public async Task<IActionResult> UpdatePaste(string code, UserPaste userPaste)
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

            if (paste.AuthorId != user.Id)
                return Unauthorized();

            paste.UpdateDatetime = DateTime.UtcNow;
            paste.Content = userPaste.Content ?? paste.Content;
            paste.Title = userPaste.Title ?? paste.Title;

            if(await _context.Syntaxes.AnyAsync(q => q.Id == userPaste.SyntaxId))
                paste.SyntaxId = userPaste.SyntaxId;

            if (await _context.Exposures.AnyAsync(q => q.Id == userPaste.ExposureId))
                paste.ExposureId = userPaste.ExposureId;

            if(userPaste.FolderId != 0 && userPaste.FolderId != null && user.Folders.Any(q => q.Id == userPaste.FolderId))
                paste.FolderId = userPaste.FolderId;

            if(userPaste.FolderId == 0)
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

            return NoContent();
        }

        // POST: api/Paste
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserPaste>> UploadPaste(UserPaste userPaste)
        {
            var paste = new Paste
            {
                Title = userPaste.Title ?? "Unnamed Paste",
                Cache = PasteUtils.GetShortContent(userPaste.Content, 250),
                Content = userPaste.Content,
                DateTime = DateTime.UtcNow,
                UploaderIPAddress = HttpContext.Connection.RemoteIpAddress,
                Views = 0,
            };

            var user = await _userManager.GetUserAsync(User);
            if (await _context.Syntaxes.AnyAsync(q => q.Id == userPaste.SyntaxId))
                paste.SyntaxId = userPaste.SyntaxId;

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

            return CreatedAtAction("GetPaste", new { code = paste.Code }, paste);
        }

        // DELETE: api/Paste/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaste(int id)
        {
            var paste = await _context.Pastes.FindAsync(id);
            if (paste == null)
            {
                return NotFound();
            }

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
