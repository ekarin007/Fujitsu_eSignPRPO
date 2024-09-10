using System.ComponentModel.DataAnnotations;

namespace Fujitsu_eSignPO.Models.Customer
{
    public class CustomerInsertUpdateModel
    {
        [Required(ErrorMessage = "Supplier Code is required.")]
        public string cusUserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string cusPassword { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string cusMail { get; set; }

        public string cusActive { get; set; }
    }
}
