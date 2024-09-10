using System.ComponentModel.DataAnnotations;

namespace Fujitsu_eSignPO.Models.AccountCode
{
    public class AccCodeInsertUpdateModel
    {
        public Guid accId { get; set; }
        [Required(ErrorMessage = "Main Code is required.")]
        public string mainCode { get; set; }

        [Required(ErrorMessage = "Sub Code 1 is required.")]
        public string subCode1 { get; set; }

        [Required(ErrorMessage = "Sub Code 2 is required.")]
        public string subCode2 { get; set; }

        public double? budget { get; set; }
        public double? balance { get; set; }
        public string active { get; set; }
    }
}
