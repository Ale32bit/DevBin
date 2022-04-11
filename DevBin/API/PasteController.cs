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

namespace DevBin.API
{
    [Route("api/v3/[controller]")]
    [ApiController]
    [RequireApiKey(ApiPermission.None)]
    public class PasteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PasteController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        // GET: api/Paste/5
        [HttpGet("{code}")]
        [RequireApiKey(ApiPermission.Get)]
        public async Task<ActionResult<Paste>> GetPaste(string code)
        {
            var paste = await _context.Pastes.FirstOrDefaultAsync(q => q.Code == code);
            if (paste == null)
            {
                return NotFound();
            }

            return paste;
        }

        // PUT: api/Paste/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaste(int id, Paste paste)
        {
            if (id != paste.Id)
            {
                return BadRequest();
            }

            _context.Entry(paste).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PasteExists(id))
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
        public async Task<ActionResult<Paste>> PostPaste(Paste paste)
        {
            _context.Pastes.Add(paste);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPaste", new { id = paste.Id }, paste);
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

        private bool PasteExists(int id)
        {
            return _context.Pastes.Any(e => e.Id == id);
        }
    }
}
