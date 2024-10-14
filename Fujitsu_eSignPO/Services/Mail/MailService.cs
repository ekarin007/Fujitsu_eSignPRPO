using Fujitsu_eSignPO.Data;
using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.Mail;
using Fujitsu_eSignPO.Services.PRPO;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using MimeKit;
using NLog.Time;
using Org.BouncyCastle.Crypto.Engines;

namespace Fujitsu_eSignPO.Services.Mail
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly FgdtESignPoContext _eSignPrpoContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<MailService> _logger;
        private readonly IConfiguration _config;
        public MailService(IOptions<MailSettings> mailSettings, FgdtESignPoContext eSignPrpoContext, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment, ILogger<MailService> logger, IConfiguration config)
        {
            _mailSettings = mailSettings.Value;
            _eSignPrpoContext = eSignPrpoContext;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _config = config;
        }

        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));

            var splitMailTo = mailRequest.ToEmail.Split(",");

            for (int i = 0; i < splitMailTo.Length; i++)
            {
                email.To.Add(MailboxAddress.Parse(splitMailTo[i]));
            }

            if (mailRequest.ccEmail != null)
            {
                var splitMailCC = mailRequest.ccEmail.Split(",");

                for (int i = 0; i < splitMailCC.Length; i++)
                {
                    email.Cc.Add(MailboxAddress.Parse(splitMailCC[i]));
                }

            }

            //email.To.Add(MailboxAddress.Parse("eakarin.pint@gmail.com"));
            //email.To.Add(MailboxAddress.Parse("worawut@itms.com"));

            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();
            if (mailRequest.Attachments != null)
            {
                #region ListFile
                //byte[] fileBytes;
                foreach (var file in mailRequest.Attachments)
                {
                    //if (file.Length > 0)
                    //{
                    //    using (var ms = new MemoryStream())
                    //    {
                    //        file.CopyTo(ms);
                    //        fileBytes = ms.ToArray();
                    //    }
                    //    builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    //}

                    // builder.Attachments.Add(file.fileName, file.fileByte, new ContentType("application", "pdf"));

                    builder.Attachments.Add(file.fileName, file.fileByte);
                }
                #endregion

            }
            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();
            var smtp = new SmtpClient();
            // smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.None);
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.None);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

            var responseSMTP = await smtp.SendAsync(email).ConfigureAwait(false);
            smtp.Disconnect(true);
        }

        public async Task<bool> sendEmail(string prNo, int stepFlow, int type, byte[] poFile , double calTotalVat)
        {
            try
            {
                var getStepTo = await _eSignPrpoContext.TbPrReviewers.Where(x => x.NRwSteps == stepFlow && x.SPoNo == prNo).FirstOrDefaultAsync();

                if (getStepTo == null && stepFlow != 7)
                {
                    return false;
                }


                string getMailByUser = "";

                if (stepFlow == 0)
                {
                    getMailByUser = await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpUsername == getStepTo.SRwApproveId).Select(x => x.SEmpEmail).FirstOrDefaultAsync();
                }
                else
                {
                    if (stepFlow < 3)
                    {
                        var listMail = await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpTitle == getStepTo.SRwApproveTitle && x.BSendMail == true).Select(x => x.SEmpEmail).ToListAsync();
                        getMailByUser = String.Join(",", listMail.ToArray());
                    }
                    else if (stepFlow == 3)
                    {
                        var listMail = await _eSignPrpoContext.TbCustomers.Where(x => x.SCusUsername == getStepTo.SRwApproveId).Select(x => x.SCusEmail).FirstOrDefaultAsync();

                        string[] splitMail = listMail?.Split(';');
                        getMailByUser = String.Join(",", splitMail);
                    }
                    else
                    {
                        //var getPr = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == prNo).FirstOrDefaultAsync();


                        //getMailByUser = await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpUsername == getPr.SCreatedBy).Select(x => x.SEmpEmail).FirstOrDefaultAsync();

                        //var listMail = await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpTitle == "Purchasing Officer").Select(x => x.SEmpEmail).ToListAsync();

                        //getMailByUser += "," + String.Join(",", listMail.ToArray());

                        var listMail = await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpUsername == getStepTo.SRwApproveId).Select(x => x.SEmpEmail).FirstOrDefaultAsync();

                        string[] splitMail = listMail?.Split(';');
                        getMailByUser = String.Join(",", splitMail);
                    }
                }

                if (getMailByUser == null)
                {
                    return false;
                }

                var getPrData = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == prNo).FirstOrDefaultAsync();
              

                var getMailTemplate = await _eSignPrpoContext.TbMailTemplates.Where(x => x.NType == type).FirstOrDefaultAsync();

                //var paymentCondition = await _eSignPrpoContext.TbPaymentConditions.Where(x => x.TIfbp == getPrData.SSupplierCode).Select(x => x.TDsca).FirstOrDefaultAsync();

                // var requestURL = _httpContextAccessor.HttpContext?.Request;
                //var url = $"{requestURL.Scheme}://{requestURL.Host}/esignPRPO/PRPO/Worklist";

                var url_INTERNAL = $"{_config.GetValue<string>("ipSettings:INTERNAL_IP")}PRPO/Worklist";
                var url_EXTERNAL = $"{_config.GetValue<string>("ipSettings:EXTERNAL_IP")}PRPO/Worklist";
                if (stepFlow == 6)
                {
                    url_INTERNAL = $"{_config.GetValue<string>("ipSettings:INTERNAL_IP")}Supplier";
                    url_EXTERNAL = $"{_config.GetValue<string>("ipSettings:EXTERNAL_IP")}Supplier";
                }


                var strURL = $"</br>Internal URL : <a href='{url_INTERNAL}'>{url_INTERNAL}</a> </br>External URL : <a href='{url_EXTERNAL}'>{url_EXTERNAL}</a>";

                if (stepFlow == 6)
                {
                    strURL = $"External URL : <a href='{url_EXTERNAL}'>{url_EXTERNAL}</a>";
                }

                var request = new MailRequest();
                if (type == 1)
                {
                    request = new MailRequest
                    {
                        Body = string.Format(getMailTemplate?.SBody, getPrData?.SPoNo, getPrData?.DPoDate?.ToString("dd/MM/yyyy"), calTotalVat.ToString("N2"), getPrData?.SCreatedName, strURL),
                        Subject = string.Format(getMailTemplate?.SSubject, getPrData?.SPoNo),
                        ToEmail = getMailByUser,
                        Attachments = null
                    };
                }
                //else if (type == 2)
                //{
                //    var mailReq = new MailRequest();

                //    var listAttInfo = new List<attachmentInfo>()
                //    {
                //        new attachmentInfo {
                //        fileByte = poFile,
                //        fileName = $"{getPrData?.SPoNo}.pdf"
                //    }
                //    };



                //    mailReq.Attachments = listAttInfo;

                //    #region comment getAttachList

                //    //var getAttachFile = await _eSignPrpoContext.TbAttachments.Where(x => x.UPrId == getPrData.UPrId && x.BIsSendSupplier == true).ToListAsync();

                //    //if (getAttachFile.Count > 0)
                //    //{
                //    //    foreach (var fileItem in getAttachFile)
                //    //    {

                //    //        string pathFile = $"{this._webHostEnvironment.WebRootPath}\\uploadfile\\";

                //    //        var filePath = Path.Combine(pathFile, $"{fileItem.UPrId + "_" + fileItem.SAttachName}");

                //    //        var fileContent = System.IO.File.ReadAllBytes(filePath);

                //    //        mailReq.Attachments.Add(new attachmentInfo
                //    //        {
                //    //            fileByte = fileContent,
                //    //            fileName = fileItem.SAttachName
                //    //        });
                //    //    }
                //    //}

                //    #endregion


                //    request = new MailRequest
                //    {
                //        Body = string.Format(getMailTemplate?.SBody, getStepTo.SRwApproveName, getPrData?.SPoNo, getPrData?.DPoDate?.ToString("dd/MM/yyyy"), getPrData?.FSumAmtThb?.ToString("N2"), "", additionalNotes, strURL, getEmpData?.SEmpName, getEmpData?.SEmpTitle, getEmpData?.Telephone, getEmpData?.Mobile, getEmpData?.SEmpEmail),
                //        Subject = string.Format(getMailTemplate?.SSubject, getPrData?.SPoNo),
                //        ToEmail = getMailByUser,
                //        ccEmail = ccMail,
                //        Attachments = mailReq.Attachments
                //    };
                //}
               
                else if (type == 2)
                {

                    var mailReq = new MailRequest();

                    var listAttInfo = new List<attachmentInfo>()
                    {
                        new attachmentInfo {
                        fileByte = poFile,
                        fileName = $"{getPrData?.SPoNo}.pdf"
                    }
                    };



                    mailReq.Attachments = listAttInfo;

                    var getTbReviewer = await _eSignPrpoContext.TbPrReviewers.Where(x => x.SPoNo == prNo && (x.NRwSteps == 2)).ToListAsync();

                    var getPurchOfiicer = getTbReviewer.Where(x => x.NRwSteps == 2).FirstOrDefault();

                    var getEmpData = await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpUsername == getPrData.SCreatedBy).FirstOrDefaultAsync();

                    //var getRequestDate = await _eSignPrpoContext.TbPrRequestItems.Where(x => x.SPoNo == prNo).FirstOrDefaultAsync();

                    var additionalNotes = string.Join(" ", getTbReviewer.Select(x => x.SRwRemark));

                    var getPrCreated = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == prNo).Select(x => x.SCreatedBy).FirstOrDefaultAsync();

                    var getCCMail = await _eSignPrpoContext.TbEmployees.Where(x => x.NPositionLevel == 2).Select(x => x.SEmpEmail).ToListAsync();

                    var getCreatedPR = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == prNo).FirstOrDefaultAsync();

                    var getMailCreatedPR = await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpUsername == getCreatedPR.SCreatedBy).FirstOrDefaultAsync();

                   

                    if (getMailCreatedPR != null)
                    {
                        getCCMail.Add(getMailCreatedPR.SEmpEmail);
                    }

                    var ccMail = string.Join(",", getCCMail);

                    request = new MailRequest
                    {
                        Body = string.Format(getMailTemplate?.SBody, getStepTo.SRwApproveName, getPrData?.SPoNo, getPrData?.DPoDate?.ToString("dd/MM/yyyy"), calTotalVat.ToString("N2"), "", additionalNotes, getEmpData?.SEmpName, getEmpData?.SEmpTitle, getEmpData?.Telephone, getEmpData?.Mobile, getEmpData?.SEmpEmail),
                        Subject = string.Format(getMailTemplate?.SSubject, getPrData?.SPoNo),
                        ToEmail = getMailByUser,
                        ccEmail = ccMail,
                        Attachments = mailReq.Attachments
                    };
                }
                else if (type == 3)
                {
                    var getVendor = await _eSignPrpoContext.TbVendors.Where(x=>x.VendorCode == getPrData.SVendorCode).FirstOrDefaultAsync();
                    request = new MailRequest
                    {
                        Body = string.Format(getMailTemplate?.SBody, getPrData?.SPoNo , getVendor?.VendorName, getPrData?.DDeliveryDate?.ToString("dd/MM/yyyy"), strURL ),
                        Subject = string.Format(getMailTemplate?.SSubject, getPrData?.SPoNo),
                        ToEmail = getMailByUser,
                        Attachments = null
                    };
                }

                Task.Run(() => SendEmailAsync(request));
                //await SendEmailAsync(request);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Send Mail :" + ex.Message);
                return false;
            }
        }

        public async Task<bool> sendRejectEmail(string poNo , double calTotalVat)
        {
            try
            {

                var getPrData = await _eSignPrpoContext.TbPrRequests.Where(x => x.SPoNo == poNo).FirstOrDefaultAsync();

                string getMailByUser = "";


                getMailByUser = await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpUsername == getPrData.SCreatedBy).Select(x => x.SEmpEmail).FirstOrDefaultAsync();


                if (getMailByUser == null)
                {
                    return false;
                }



                var getMailTemplate = await _eSignPrpoContext.TbMailTemplates.Where(x => x.NType == 4).FirstOrDefaultAsync();

                var paymentCondition = await _eSignPrpoContext.TbPaymentConditions.Where(x => x.TIfbp == getPrData.SVendorCode).Select(x => x.TDsca).FirstOrDefaultAsync();

                var requestURL = _httpContextAccessor.HttpContext?.Request;
                var url_INTERNAL = $"{_config.GetValue<string>("ipSettings:INTERNAL_IP")}PRPO/Worklist";
                var url_EXTERNAL = $"{_config.GetValue<string>("ipSettings:EXTERNAL_IP")}PRPO/Worklist";

                var strURL = $"</br>Internal URL : <a href='{url_INTERNAL}'>{url_INTERNAL}</a> </br><a href='{url_EXTERNAL}'>External URL : {url_EXTERNAL}</a>";
                var request = new MailRequest();
                if (getMailTemplate != null)
                {
                    request = new MailRequest
                    {
                        Body = string.Format(getMailTemplate?.SBody, getPrData?.SPoNo, getPrData?.DPoDate?.ToString("dd/MM/yyyy"), calTotalVat.ToString("N2"), strURL),
                        Subject = string.Format(getMailTemplate?.SSubject, getPrData?.SPoNo),
                        ToEmail = getMailByUser,
                        Attachments = null
                    };
                }

                await SendEmailAsync(request);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
