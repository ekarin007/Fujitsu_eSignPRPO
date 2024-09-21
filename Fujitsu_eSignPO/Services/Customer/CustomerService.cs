using Azure;
using DocumentFormat.OpenXml.InkML;
using Fujitsu_eSignPO.Data;
using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.Customer;
using Fujitsu_eSignPO.Services.PRPO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.Numerics;

namespace Fujitsu_eSignPO.Services.Customer
{

    public class CustomerService : ICustomerService
    {
        private readonly IAccountService _accountService;
        private static FgdtESignPoContext _eSignPrpoContext;
        private readonly ILogger<CustomerService> _logger;
        public CustomerService(IAccountService accountService, FgdtESignPoContext eSignPrpoContext, ILogger<CustomerService> logger)
        {
            _accountService = accountService;
            _eSignPrpoContext = eSignPrpoContext;
            _logger = logger;

        }

        public async Task<List<TbCustomer>> getCustomer() => await _eSignPrpoContext.TbCustomers.ToListAsync();

        public async Task<TbCustomer> getCustomerBySupID(string supID) => await _eSignPrpoContext.TbCustomers.Where(x => x.SCusUsername == supID).FirstOrDefaultAsync();

        public async Task<Tuple<bool, string>> insertCustomer(CustomerInsertUpdateModel request)
        {
            try
            {
                var informationData = _accountService.informationUser();
                var insertCus = new TbCustomer
                {
                    UCusId = Guid.NewGuid(),
                    SCusUsername = request?.cusUserName.Split("|")[0],
                    SCusName = request?.cusUserName.Split("|")[1],
                    SCusPassword = request?.cusPassword,
                    SCusEmail = request?.cusMail,
                    BActive = request?.cusActive == "true" ? true : false,
                    DCreated = DateTime.Now,
                    SCreatedBy = informationData?.sID


                };
                _eSignPrpoContext.TbCustomers.Add(insertCus);
                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;


                return Tuple.Create(response, $"Create Username : {request?.cusUserName.Split("|")[0]} is success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Tuple.Create(false, ex.Message); ;
            }
        }

        public async Task<Tuple<bool, string>> updateCustomer(CustomerInsertUpdateModel request)
        {
            try
            {
                var informationData = _accountService.informationUser();

                var responseCus = await getCustomerBySupID(request?.cusUserName.Split("|")[0]);


                responseCus.SCusPassword = request?.cusPassword;
                responseCus.SCusEmail = request?.cusMail;
                responseCus.BActive = request?.cusActive == "true" ? true : false;
                responseCus.DUpdated = DateTime.Now;
                responseCus.SUpdatedBy = informationData?.sID;


                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;


                return Tuple.Create(response, $"Update Username : {request?.cusUserName.Split("|")[0]} is success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Tuple.Create(false, ex.Message); ;
            }
        }

        public async Task<Tuple<bool, string>> deleteCustomer(string supID)
        {
            try
            {
                var informationData = _accountService.informationUser();

                var responseCus = await getCustomerBySupID(supID);

                _eSignPrpoContext.TbCustomers.Remove(responseCus);

                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;


                return Tuple.Create(response, $"Delete Username : {supID} is success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Tuple.Create(false, ex.Message); ;
            }
        }

        public async Task<Tuple<bool, string>> ImportExcelFile(IFormFile file)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    // Assuming VendorCode is in column 1 and VendorName is in column 2
                    for (int row = 2; row <= rowCount; row++) // Start at row 2 to skip header
                    {
                        string vendorCode = worksheet.Cells[row, 1].Value?.ToString().Trim();
                        string vendorName = worksheet.Cells[row, 2].Value?.ToString().Trim();

                        if (!string.IsNullOrEmpty(vendorCode) && !string.IsNullOrEmpty(vendorName))
                        {
                            var existingVendor = await _eSignPrpoContext.TbVendors.Where(x=>x.VendorCode == vendorCode).FirstOrDefaultAsync();

                            if (existingVendor != null)
                            {
                                existingVendor.VendorName = vendorName;
                                _eSignPrpoContext.TbVendors.Update(existingVendor);
                            }
                            else
                            {
                                var vendors = new TbVendor
                                {

                                    VendorCode = vendorCode,
                                    VendorName = vendorName
                                };

                                await _eSignPrpoContext.TbVendors.AddAsync(vendors);
                            }
                        }

                            
                           
                    }
                }
            }

            var response = await _eSignPrpoContext.SaveChangesAsync() > 0;

            return Tuple.Create(response, $"File uploaded and data saved successfully!.");
        }
    }
}
