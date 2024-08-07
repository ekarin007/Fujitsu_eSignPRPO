using eSignPRPO.Models.PRPO;

namespace eSignPRPO.Models.Profiles
{
    public class UploadFileModel
    {
        public string empId { get; set; }
        public List<fileUpload> fileUploads { get; set; }
    }
}
