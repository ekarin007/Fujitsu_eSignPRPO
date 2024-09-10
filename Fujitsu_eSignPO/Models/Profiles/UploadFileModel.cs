using Fujitsu_eSignPO.Models.PRPO;

namespace Fujitsu_eSignPO.Models.Profiles
{
    public class UploadFileModel
    {
        public string empId { get; set; }
        public List<fileUpload> fileUploads { get; set; }
    }
}
