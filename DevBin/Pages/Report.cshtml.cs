using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DevBin.Pages
{
    public class ReportModel : PageModel
    {
        private readonly Context _context;
        private readonly IConfiguration _configuration;
        public ReportModel(Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public Paste Paste { get; set; }

        [BindProperty]
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync(string? code)
        {
            Paste = await _context.Pastes.FirstOrDefaultAsync(m => m.Code == code);
            if(Paste == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? code)
        {
            var paste = await _context.Pastes.FirstOrDefaultAsync(m => m.Code == code);
            if (paste == null) return NotFound();

            var currentUser = HttpContext.Items["User"] as User;
            string fromUser;

            if (currentUser == null)
            {
                fromUser = HttpContext.Connection.RemoteIpAddress.ToString();
            }
            else
            {
                fromUser = currentUser.Username;
            }

            var webhookUrl = _configuration.GetValue<string>("DiscordReportWebhook");

            var webhookClient = new Discord.Webhook.DiscordWebhookClient(webhookUrl);

            IPAddress? pasteIp = null;
            if (paste.IpAddress != null)
            {
                pasteIp = new IPAddress(paste.IpAddress);
            }

            var builder = new Discord.EmbedBuilder();
            builder.WithTitle(paste.Title);
            builder.WithDescription(paste.Cache);
            builder.WithUrl($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{paste.Code}");
            builder.WithColor(Discord.Color.LighterGrey);
            builder.WithAuthor(new Discord.EmbedAuthorBuilder()
                .WithName(paste.AuthorId.HasValue ? paste.Author.Username : (pasteIp != null ? pasteIp.ToString() : "Guest (No IP)")));
            builder.WithFooter(new Discord.EmbedFooterBuilder()
                .WithText("Reported by: " + fromUser));
            builder.WithCurrentTimestamp();
            builder.AddField("Code", paste.Code, true);
            builder.AddField("Syntax", paste.Syntax?.Pretty, true);
            builder.AddField("Exposure", paste.Exposure.Name, true);
            builder.AddField("User Report Message", Message ?? "Unknown message", false);

            var embed = builder.Build();
            var list = new List<Discord.Embed>
            {
                embed
            };

            await webhookClient.SendMessageAsync(null, false, list, null, null, null, Discord.AllowedMentions.None);

            return Redirect("/" + code);
        }
    }
}
