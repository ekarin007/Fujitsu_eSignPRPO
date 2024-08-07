using eSignPRPO.Models.Mail;

namespace eSignPRPO.interfaces
{
    public interface IMailService
    {
        //Task SendEmailAsync(MailRequest mailRequest);
        Task<bool> sendEmail(string prNo, int stepFlow, int type, byte[] poFile);
        Task<bool> sendRejectEmail(string prNo);
    }
}
