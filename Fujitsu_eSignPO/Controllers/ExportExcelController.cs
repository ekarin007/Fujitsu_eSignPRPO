using ClosedXML.Excel;
using Fujitsu_eSignPO.Data;
using Fujitsu_eSignPO.interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Fujitsu_eSignPO.Controllers
{
    public class ExportExcelController : Controller
    {

        private readonly IPRPOService _PRPOService;
        private readonly FgdtESignPoContext _eSignPrpoContext;
        public ExportExcelController(IPRPOService pRPOService, FgdtESignPoContext eSignPoContext)
        {
            _PRPOService = pRPOService;
            _eSignPrpoContext = eSignPoContext;
        }
        public async Task<IActionResult> Index()
        {
            var getDeparment = await _PRPOService.getDepData();
            ViewBag.departments = getDeparment;
            return View();
        }

        public async Task<IActionResult> ExportCompareExcel(string datestart, string dateend, string department)
        {
            try
            {
                XLWorkbook wbook2 = new XLWorkbook();

                var wb = wbook2.Worksheets.Add("Compare Data");

                wb.PageSetup.PaperSize = XLPaperSize.A4Paper;
                wb.Range("A1:H1").Columns().Style.Fill.BackgroundColor = XLColor.BabyBlueEyes;

                wb.Cell("A1").Value = "PO No.";
                wb.Cell("B1").Value = "Supplier A";
                wb.Cell("C1").Value = "Supplier B";
                wb.Cell("D1").Value = "Item Name";
                wb.Cell("E1").Value = "Price A";
                wb.Cell("F1").Value = "Price B";
                wb.Cell("G1").Value = "Price A-B";
                wb.Cell("H1").Value = "Create Date";

                #region worksheets style
                wb.Cell("A1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("B1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("C1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("D1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("E1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("F1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("G1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                wb.Cell("H1").Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);


                #endregion

                wb.RangeUsed().SetAutoFilter();
                wb.Columns().AdjustToContents();

                DateTime? dateSt = null;
                DateTime? dateN = null;
                if (!string.IsNullOrEmpty(datestart) && !string.IsNullOrEmpty(dateend))
                {
                    dateSt = DateTime.ParseExact(datestart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    dateN = DateTime.ParseExact(dateend, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                var query = _eSignPrpoContext.VwPoCompares.AsQueryable();

                if (dateSt.HasValue && dateN.HasValue)
                {
                    TimeSpan tsStart = new TimeSpan(0, 0, 0);
                    TimeSpan tsEnd = new TimeSpan(23, 59, 0);
                    query = query.Where(x => x.DCreateDate >= dateSt + tsStart && x.DCreateDate <= dateN + tsEnd);
                }


                if (!string.IsNullOrEmpty(department))
                {
                    query = query.Where(x => x.SDepartment.Equals(department));
                }

                var queryList = query.ToList();


                if (queryList.Count() > 0)
                {
                    for (int i = 0; i <= (queryList.Count() - 1); i++)
                    {

                        var subAmount = queryList[i]?.FVendorAmount1 - queryList[i]?.FVendorAmount2;

                        wb.Cell("A" + (2 + i)).Value = queryList[i].SPoNo;
                        //wb.Cell("A" + (2 + i)).Style.DateFormat.Format = "dd-MM-yy";
                        wb.Cell("B" + (2 + i)).Value = queryList[i].SVendor1;

                        wb.Cell("C" + (2 + i)).Value = queryList[i].SVendor2;
                        //wb.Cell("C" + (2 + i)).SetDataType(XLDataType.Text);
                        wb.Cell("D" + (2 + i)).Value = queryList[i].SCpItem;
                        wb.Cell("E" + (2 + i)).Value = queryList[i]?.FVendorAmount1?.ToString("#,##0.00");
                        wb.Cell("F" + (2 + i)).Value = queryList[i]?.FVendorAmount2?.ToString("#,##0.00");
                        wb.Cell("G" + (2 + i)).Value = subAmount?.ToString("#,##0.00");
                        wb.Cell("H" + (2 + i)).Value = queryList[i]?.DCreateDate?.ToString("dd/MM/yyyy HH:mm:ss");

                        #region worksheets style
                        wb.Cell("A" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("B" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("C" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("D" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("E" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("F" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("G" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
                        wb.Cell("H" + (2 + i)).Style
    .Border.SetTopBorder(XLBorderStyleValues.Medium)
    .Border.SetRightBorder(XLBorderStyleValues.Medium)
    .Border.SetBottomBorder(XLBorderStyleValues.Medium)
    .Border.SetLeftBorder(XLBorderStyleValues.Medium);
         

                        #endregion
                    }


                }

                wb.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);


                using (MemoryStream memoryStream = new MemoryStream())
                {
                    wbook2.SaveAs(memoryStream);
                    var content = memoryStream.ToArray();

                    return File(
                    content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"ExportCompare_{DateTime.Now.ToString("ddMMyyyy")}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return NotFound("ERROR :" + ex.InnerException.Message);
            }

        }
    }
}
