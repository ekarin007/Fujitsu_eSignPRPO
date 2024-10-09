using Fujitsu_eSignPO.Models.Mail;

namespace Fujitsu_eSignPO.interfaces
{
    public interface IMailService
    {
        //Task SendEmailAsync(MailRequest mailRequest);
        Task<bool> sendEmail(string prNo, int stepFlow, int type, byte[] poFile,double calTotalVat);
        Task<bool> sendRejectEmail(string prNo , double calTotalVat);
    }
}
