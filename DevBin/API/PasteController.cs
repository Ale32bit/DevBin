using DevBin.Data;
using DevBin.DTO;
using DevBin.Middleware;
using DevBin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Syntaxes = DevBin.DTO.Syntaxes;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DevBin.API
{
    [Route("api/v2/paste")]
    [ApiController]
    [RequireToken]
    public class PasteController : ControllerBase
    {
        private readonly Context _context;
        private readonly PasteStore _pasteStore;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        /// <summary>
        /// Paste API
        /// </summary>
        public PasteController(Context context, PasteStore pasteStore, IMemoryCache cache, IConfiguration configuration)
        {
            _context = context;
            _pasteStore = pasteStore;
            _cache = cache;
            _configuration = configuration;
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
            var authUser = (User)HttpContext.Items["APIUser"];

            if (authUser == null)
            {
                return Unauthorized();
            }

            var paste = _context.Pastes.FirstOrDefault(q => q.Code == code);

            if (paste == null)
            {
                return NotFound();
            }

            if (paste.Exposure.IsPrivate && paste.AuthorId != null && paste.AuthorId != authUser.Id)
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
        /// <summary>
        /// Upload a paste
        /// </summary>
        /// <returns>Information about the newly created paste</returns>
        /// <remarks>Raw paste content can be fetched from /raw/{code}</remarks>
        [HttpPost]
        [ProducesResponseType(typeof(PasteResult),  (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.Unauthorized)]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IActionResult> Post([FromBody] UserPaste userPaste)
        {
            var authUser = (User)HttpContext.Items["APIUser"];

            if (authUser == null)
            {
                return Unauthorized();
            }

            var paste = new Paste
            {
                Title = userPaste.Title ?? "Unnamed Paste",
                Content = userPaste.Content ?? "",
                AuthorId = authUser.Id,
            };

            // User input checks

            if (userPaste.Content.Length > _configuration.GetValue<long>("PasteMaxSize"))
            {
                return BadRequest("Content length exceeded");
            }

            paste.ExposureId = _context.Exposures.FirstOrDefault(q => q.Id == userPaste.Exposure)?.Id ?? 1;
            paste.SyntaxId = _context.Syntaxes.FirstOrDefault(q => q.Name == userPaste.Syntax)?.Id ?? 1;

            do
            {
                paste.Code = Utils.RandomAlphaString(_configuration.GetValue<int>("PasteCodeLength"));
            } while (_context.Pastes.Any(q => q.Code == paste.Code));

            paste.Datetime = DateTime.Now;
            paste.Cache = paste.Content[..Math.Min(paste.Content.Length, 255)];

            _pasteStore.Write(paste.Code, paste.Content);
            _context.Pastes.Add(paste);
            await _context.SaveChangesAsync();

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

        /// <summary>
        /// Update a paste data
        /// </summary>
        /// <returns>Information about the updated paste</returns>
        /// <remarks>Raw paste content can be fetched from /raw/{code}</remarks>
        [HttpPatch("{code}")]
        [ProducesResponseType(typeof(PasteResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.Unauthorized)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string code, [FromBody] UserPaste userPaste)
        {
            var authUser = (User)HttpContext.Items["APIUser"];

            if (authUser == null)
            {
                return Unauthorized();
            }

            var paste = _context.Pastes.FirstOrDefault(q => q.Code == code);

            if (paste == null)
            {
                return NotFound();
            }

            if (paste.Exposure.IsPrivate && paste.AuthorId != null && paste.AuthorId != authUser.Id)
            {
                return Forbid();
            }

            // Update content
            if (userPaste.Content != null)
            {
                if (userPaste.Content.Length > _configuration.GetValue<long>("PasteMaxSize"))
                {
                    return BadRequest("Content length exceeded");
                }

                _pasteStore.Write(paste.Code, userPaste.Content);
            }

            if (userPaste.Title != null)
            {
                paste.Title = userPaste.Title;
            }

            paste.ExposureId = _context.Exposures.FirstOrDefault(q => q.Id == userPaste.Exposure)?.Id ?? paste.ExposureId;
            paste.SyntaxId = _context.Syntaxes.FirstOrDefault(q => q.Name == userPaste.Syntax)?.Id ?? paste.SyntaxId;

            _context.Update(paste);
            await _context.SaveChangesAsync();

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
            var authUser = (User)HttpContext.Items["APIUser"];

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

                _pasteStore.Delete(paste.Code);
                _cache.Remove("PASTE:" + paste.Code);

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
            var syntaxes = _context.Syntaxes.Select(q => new Syntaxes
            {
                Id = q.Name,
                Name = q.Pretty
            }).ToArray();

            return new JsonResult(syntaxes);
        }

        /// <summary>
        /// Get a list of available exposures
        /// </summary>
        /// <returns>Array of exposures</returns>
        [HttpGet("exposures")]
        [ProducesResponseType(typeof(Exposures[]), 200)]
        [Produces("application/json")]
        public IActionResult GetExposures()
        {
            var exposures = _context.Exposures.Select(q => new DTO.Exposures
            {
                Id = q.Id,
                Name = q.Name,
                IsPublic = q.IsPublic,
                AllowEdit = q.AllowEdit,
                IsPrivate = q.IsPrivate,
                RegisteredOnly = q.RegisteredOnly,
            }).ToArray();

            return new JsonResult(exposures);
        }

        private User ResolveToken(string token)
        {
            return _context.Users.FirstOrDefault(q => q.ApiToken == token);
        }
    }
}
