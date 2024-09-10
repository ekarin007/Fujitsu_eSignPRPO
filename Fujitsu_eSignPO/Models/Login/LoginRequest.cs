using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.ComponentModel.DataAnnotations;

namespace Fujitsu_eSignPO.Models.Login
{
    public class LoginRequest
    {
        [BindProperty]
        public Credential Credential { get; set; }

    }

    public class Credential
    {
        [Required(ErrorMessage = "กรุณากรอก Username")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "กรุณากรอก Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
