// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using DevBin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DevBin.Areas.Identity.Pages.Account.Manage
{
    public class ApiTokensModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ApiTokensModel> _logger;

        public ApiTokensModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ApiTokensModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public IList<ApiToken> Tokens { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Tokens = user.ApiTokens.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return RedirectToPage("./ApiTokens");
        }

        public async Task<IActionResult> OnPostCreateTokenAsync([FromForm] ApiToken token)
        {
            var user = await _userManager.GetUserAsync(User);

            var form = HttpContext.Request.Form;
            token.AllowGet = form["AllowGet"] == "on";
            token.AllowCreate = form["AllowCreate"] == "on";
            token.AllowUpdate = form["AllowUpdate"] == "on";
            token.AllowDelete = form["AllowDelete"] == "on";
            token.AllowGetUser = form["AllowGetUser"] == "on";
            token.AllowCreateFolders = form["AllowCreateFolders"] == "on";
            token.AllowDeleteFolders = form["AllowDeleteFolders"] == "on";

            token.CreatedAt = DateTime.UtcNow;
            token.OwnerId = user.Id;

            do
            {
                token.Token = Utils.Utils.GenerateRandomSecureToken();
            } while (_context.ApiTokens.Any(q => q.Token == token.Token));

            _context.Add(token);
            await _context.SaveChangesAsync();

            StatusMessage = "Your developer API token has been created. Keep it a secret!\n" + token.Token;

            return RedirectToPage("./ApiTokens");
        }

        public async Task<IActionResult> OnPostEditTokenAsync([FromForm] ApiToken token, string action)
        {
            var user = await _userManager.GetUserAsync(User);
            var apiToken = await _context.ApiTokens.FirstOrDefaultAsync(q => q.Id == token.Id && q.OwnerId == user.Id);
            if (apiToken == null)
            {
                StatusMessage = "Error finding the API token.";
                return RedirectToPage("./ApiTokens");
            }
            if (action == "update")
            {
                var form = HttpContext.Request.Form;
                
                apiToken.AllowGet = form["token.AllowGet"] == "on";
                apiToken.AllowCreate = form["token.AllowCreate"] == "on";
                apiToken.AllowUpdate = form["token.AllowUpdate"] == "on";
                apiToken.AllowDelete = form["token.AllowDelete"] == "on";
                apiToken.AllowGetUser = form["token.AllowGetUser"] == "on";
                apiToken.AllowCreateFolders = form["token.AllowCreateFolders"] == "on";
                apiToken.AllowDeleteFolders = form["token.AllowDeleteFolders"] == "on";

                apiToken.Name = token.Name;

                _context.Update(apiToken);
                await _context.SaveChangesAsync();

                StatusMessage = "Developer API token updated!";
            }
            else if (action == "delete")
            {
                _context.ApiTokens.Remove(apiToken);
                await _context.SaveChangesAsync();

                StatusMessage = "Developer API token deleted!";
            }

            return RedirectToPage("./ApiTokens");
        }
    }
}
