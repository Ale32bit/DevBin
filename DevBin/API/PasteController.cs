using DevBin.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        // GET: api/<PasteController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<PasteController>/5
        [HttpGet("{code}")]
        public DTO.PasteResult Get(string code)
        {
            var authUser = (Models.User)HttpContext.Items["APIUser"];

            var paste = _context.Pastes.FirstOrDefault(q => q.Code == code);

            if(paste.Exposure.IsPrivate && paste.AuthorId != authUser.Id)
            {
                return new DTO.PasteResult();
            }

            var result = new DTO.PasteResult
            {
                Title = paste.Title,
                SyntaxId = paste.SyntaxId,
                ExposureId = paste.ExposureId,
                Author = paste.Author?.Username,
                CreationDate = paste.Datetime,
                UpdateDate = paste.UpdateDatetime,
                Views = paste.Views,
            };

            return result;
        }

        // POST api/<PasteController>
        [HttpPost]
        public void Post([FromBody] string value)
        {

        }

        // PUT api/<PasteController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PasteController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private Models.User ResolveToken(string token)
        {
            return _context.Users.FirstOrDefault(q => q.ApiToken == token);
        }
    }
}
