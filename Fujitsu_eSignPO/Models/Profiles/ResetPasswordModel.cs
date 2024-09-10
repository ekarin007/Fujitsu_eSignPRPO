using System.ComponentModel.DataAnnotations;

namespace Fujitsu_eSignPO.Models.Profiles
{
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Supplier Code is required.")]
        public string userName { get; set; }

        [Required(ErrorMessage = "Old Password is required.")]
        [DataType(DataType.Password)]
        public string oldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [StringLength(100, ErrorMessage = "The New Password must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string newPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("newPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string confirmPassword { get; set; }
    }
}
