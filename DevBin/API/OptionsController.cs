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
using DevBin.Attributes;

namespace DevBin.API
{
    [Route("api/v3/[controller]")]
    [ApiController]
    [RequireApiKey(ApiPermission.None)]
    public class OptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OptionsController(ApplicationDbContext context)
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
}
