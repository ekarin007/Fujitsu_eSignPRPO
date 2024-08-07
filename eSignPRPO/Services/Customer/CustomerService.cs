using eSignPRPO.Data;
using eSignPRPO.interfaces;
using eSignPRPO.Models;
using eSignPRPO.Models.Customer;
using eSignPRPO.Services.PRPO;
using Microsoft.EntityFrameworkCore;

namespace eSignPRPO.Services.Customer
{

    public class CustomerService : ICustomerService
    {
        private readonly IAccountService _accountService;
        private static ESignPrpoContext _eSignPrpoContext;
        private readonly ILogger<CustomerService> _logger;
        public CustomerService(IAccountService accountService, ESignPrpoContext eSignPrpoContext, ILogger<CustomerService> logger)
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
    }
}
