using DevBin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin.Pages.Account
{
    public class VerifyModel : PageModel
    {
        private readonly Context _context;
        public VerifyModel(Context context)
        {
            _context = context;
        }

#nullable enable
        public async Task<IActionResult> OnGetAsync([FromQuery] string? code)
        {
            var user = _context.Users.FirstOrDefault(q => q.ActionCode == code && !q.Verified);
            if (user == null)
            {
                return NotFound();
            }

            user.Verified = true;

            _context.Update(user);
            await _context.SaveChangesAsync();

            return Page();
        }
    }
}
