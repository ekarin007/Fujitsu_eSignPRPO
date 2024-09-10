namespace Fujitsu_eSignPO.Models.Mail
{
    public class MailRequest
    {
        public string ToEmail { get; set; }
        public string ccEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        //public List<IFormFile> Attachments { get; set; }
        public List<attachmentInfo> Attachments { get; set; }
    }

   public class attachmentInfo
    {
        public string fileName { get; set; }
        public byte[] fileByte { get; set; }
    }
}
