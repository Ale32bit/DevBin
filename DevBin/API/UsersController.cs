using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevBin.Data;
using DevBin.Models;
using DevBin.Middleware;
using DevBin.DTO;
using System.Net;

namespace DevBin.API
{
    [Route("api/v2/user")]
    [ApiController]
    [RequireToken]
    public class UsersController : ControllerBase
    {
        private readonly Context _context;

        public UsersController(Context context)
        {
            _context = context;
        }


        // GET: api/user/:username
        [HttpGet("{username}")]
        [ProducesResponseType(typeof(PasteResult[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.Unauthorized)]
        [Produces("application/json")]
        public async Task<IActionResult> GetUser(string username)
        {
            var authUser = (User)HttpContext.Items["APIUser"];
            if (authUser == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(q => q.Username == username);
            if (user == null)
            {
                return NotFound();
            }

            var pastes = _context.Pastes.Where(q => q.AuthorId == user.Id);
            if (authUser.Id != user.Id)
            {
                pastes = pastes.Where(q => !q.Exposure.IsPrivate);
            }

            List<PasteResult> results = new();

            foreach (var paste in pastes)
            {
                results.Add(new()
                {
                    Title = paste.Title,
                    ExposureId = paste.ExposureId,
                    SyntaxId = paste.Syntax.Name,
                    Views = paste.Views,
                    Author = user.Username,
                    CreationDate = paste.Datetime,
                    UpdateDate = paste.UpdateDatetime,
                });
            }

            return new JsonResult(results);
        }
    }
}
