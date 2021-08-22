using DevBin.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DevBin.DTO;
using DevBin.Middleware;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DevBin.API
{
    [Route("api/paste")]
    [ApiController]
    [RequireToken]
    public class PasteController : ControllerBase
    {
        private readonly Context _context;
        public PasteController(Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Fetch a paste information using its code
        /// </summary>
        /// <param name="code">Paste code</param>
        /// <returns>Information about the paste</returns>
        /// <remarks>Raw paste content can be fetched from /raw/{code}</remarks>
        [HttpGet("{code}")]
        [ProducesResponseType(typeof(PasteResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.Unauthorized)]
        [Produces("application/json")]
        public IActionResult Get(string code)
        {
            var authUser = (Models.User)HttpContext.Items["APIUser"];

            if (authUser == null)
            {
                return Unauthorized();
            }

            var paste = _context.Pastes.FirstOrDefault(q => q.Code == code);

            if (paste == null)
            {
                return NotFound();
            }

            if(paste.Exposure.IsPrivate && paste.AuthorId != null && paste.AuthorId != authUser.Id)
            {
                return Forbid();
            }

            var result = new PasteResult
            {
                Title = paste.Title,
                SyntaxId = paste.Syntax.Name,
                ExposureId = paste.ExposureId,
                Author = paste.Author?.Username,
                CreationDate = paste.Datetime,
                UpdateDate = paste.UpdateDatetime,
                Views = paste.Views,
            };

            return new JsonResult(result);
        }

        // POST api/<PasteController>
        [HttpPost]
        public void Post([FromBody] string value)
        {

        }

        /// <summary>
        /// Delete a paste
        /// </summary>
        /// <param name="code">Paste code</param>
        /// <returns>Whether successful</returns>
        [HttpDelete("{code}")]
        [ProducesResponseType(typeof(ActionResult), 200)]
        [ProducesResponseType(typeof(ActionResult), 404)]
        [ProducesResponseType(typeof(ActionResult), 401)]
        [ProducesResponseType(typeof(ActionResult), 403)]
        public async Task<IActionResult> Delete(string code)
        {
            var authUser = (Models.User)HttpContext.Items["APIUser"];

            if (authUser == null)
            {
                return Unauthorized();
            }

            var paste = _context.Pastes.FirstOrDefault(q => q.Code == code);

            if (paste == null)
            {
                return NotFound();
            }

            if (paste.AuthorId.HasValue && paste.AuthorId.Value == authUser.Id)
            {
                _context.Pastes.Remove(paste);
                await _context.SaveChangesAsync();

                return Ok();
            }

            return Forbid();
        }

        /// <summary>
        /// Get a list of available syntaxes
        /// </summary>
        /// <returns>Array of syntaxes</returns>
        [HttpGet("syntaxes")]
        [ProducesResponseType(typeof(Syntaxes[]), 200)]
        [Produces("application/json")]
        public IActionResult GetSyntaxes()
        {
            var syntaxes = _context.Syntaxes.Select(q => new Syntaxes {Id = q.Name, Name = q.Pretty}).ToArray();

            return new JsonResult(syntaxes);
        }

        private Models.User ResolveToken(string token)
        {
            return _context.Users.FirstOrDefault(q => q.ApiToken == token);
        }
    }
}
