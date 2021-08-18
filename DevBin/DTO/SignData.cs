using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin.DTO
{
    public class SignInData
    {
        [Required]
        [StringLength(32, ErrorMessage = "Length must be between {2} and {1}.", MinimumLength = 3)]
        [RegularExpression(@"[A-Za-z0-9_]+", ErrorMessage = "May only contain alphanumeric characters and underscores.")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Must be at least {1} characters long.")]
        [MaxLength(1024, ErrorMessage = "You somehow exceeded the big length limit of 2^10. why")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Keep me logged in")]
        public bool KeepLoggedIn { get; set; } = false;
    }

    public class SignUpData
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        [PageRemote(
            ErrorMessage = "This email address is already in use.",
            AdditionalFields = "__RequestVerificationToken",
            HttpMethod = "post",
            PageHandler = "CheckEmail"
        )]
        public string Email { get; set; }

        [Required]
        [StringLength(32, ErrorMessage = "Length must be between {2} and {1}.", MinimumLength = 3)]
        [RegularExpression(@"[A-Za-z0-9_]+", ErrorMessage = "May only contain alphanumeric characters and underscores.")]
        [Display(Name = "Username")]
        [PageRemote(
            ErrorMessage = "This username is already in use.",
            AdditionalFields = "__RequestVerificationToken",
            HttpMethod = "post",
            PageHandler = "CheckUsername"
        )]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Must be at least {1} characters long.")]
        [MaxLength(1024, ErrorMessage = "You somehow exceeded the big length limit of 2^10. why")]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
