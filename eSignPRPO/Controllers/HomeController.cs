using eSignPRPO.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using eSignPRPO.Models.Mail;
using eSignPRPO.interfaces;
using MimeKit;
using System.IO;
using AspNetCore.Reporting;
using System.Security;
namespace eSignPRPO.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private IMailService _mailService;
        private IWebHostEnvironment _webHostEnvironment;
        public HomeController(ILogger<HomeController> logger, IMailService mailService, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _mailService = mailService;
            _webHostEnvironment = webHostEnvironment;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        //[Authorize(Roles = "0")]
        public IActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public IActionResult GetOptions()
        {
            // Retrieve the options from a data source
            // and return them as JSON

            // Example code:
            var options = new List<string> { "Option 1", "Option 2", "Option 3" };

            return Json(options);
        }

        [Authorize(Roles = "1,2,3")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //public async Task<IActionResult> TestSendMail()

        //{
        //    var request = new MailRequest
        //    {
        //        Body = "Test 1234 krub.",
        //        Subject = "Test Email krub",
        //        ToEmail = "eakarin.pint@gmail.com;worawut@itms.co.th"
        //    };
        //    await _mailService.SendEmailAsync(request);

        //    return Ok();
        //}
        [HttpGet]
        public IActionResult PrintAsync()
        {
            var path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\PO_Report.rdlc";
            LocalReport localReport = new LocalReport(path);

            string mimTypes = "";
            int extension = (int)(DateTime.Now.Ticks >> 10);

            DataTable dt1 = new DataTable("ResponsePOReport");
            dt1.Columns.Add("poNo");
            dt1.Columns.Add("datePo");
            dt1.Columns.Add("capex");
            dt1.Columns.Add("supplierCode");
            dt1.Columns.Add("currency");
            dt1.Columns.Add("shipVia");
            dt1.Columns.Add("termCondition");
            dt1.Columns.Add("paymentCondition");
            dt1.Columns.Add("subAmount");
            dt1.Columns.Add("vatAmount");
            dt1.Columns.Add("totalAmount");
            dt1.Columns.Add("remarks");

            dt1.Rows.Add(
                "PC3019552",
                "Date : 22.Sep.2020",
                null,
                "SUP000986",
                "THB",
                "TRUCK",
                "TERM TEST",
                "30 Days End of Month",
                "10,000.00",
                "115.00",
                "10,115.00",
                "TEST Remarks"
                );

            DataTable dt2 = new DataTable("POItem");
            dt2.Columns.Add("no");
            dt2.Columns.Add("itemCode");
            dt2.Columns.Add("description");
            dt2.Columns.Add("quantity");
            dt2.Columns.Add("uom");
            dt2.Columns.Add("unitPrice");
            dt2.Columns.Add("amount");
            dt2.Columns.Add("deliveryDate");

            var rowCnt = 1;
            for (int i = 1; i <= 17; i++)
            {
                dt2.Rows.Add(
                    $"{i}",
                    "1008010062",
                    "A-ARCH P/U SEAL INLET",
                    "10.00",
                    "ea",
                    "150.00",
                    "1,500.00",
                    "12.Oct.2020"
                    );

                rowCnt++;
            }

            //for (int i = 1; i <= 24-rowCnt; i++)
            //{
            //    dt2.Rows.Add(
            //       null,
            //       null,
            //       null,
            //       null,
            //       null,
            //       null,
            //       null,
            //       null
            //       );

            //}
            localReport.AddDataSource("DataSet1", dt1);
            localReport.AddDataSource("DataSet2", dt2);

            var result = localReport.Execute(RenderType.Pdf, extension, null, mimTypes);
            return File(result.MainStream, "application/pdf");
        }
    }
}