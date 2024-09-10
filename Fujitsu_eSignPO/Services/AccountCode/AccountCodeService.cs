using Fujitsu_eSignPO.Data;
using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.AccountCode;
//using Fujitsu_eSignPO.Models.;
using Fujitsu_eSignPO.Services.PRPO;
using Microsoft.EntityFrameworkCore;

namespace Fujitsu_eSignPO.Services.AccountCode
{
    public class AccountCodeService : IAccountCodeService
    {
        private readonly IAccountService _accountService;
        private static FgdtESignPoContext _eSignPrpoContext;
        private readonly ILogger<AccountCodeService> _logger;
        public AccountCodeService(IAccountService accountService, FgdtESignPoContext eSignPrpoContext, ILogger<AccountCodeService> logger)
        {
            _accountService = accountService;
            _eSignPrpoContext = eSignPrpoContext;
            _logger = logger;

        }

        public async Task<List<TbAccountCode>> getAccountCode() => await _eSignPrpoContext.TbAccountCodes.Where(x => x.Active == true).ToListAsync();

        public async Task<TbAccountCode> GetAccountCodeByGuid(Guid? guid) => await _eSignPrpoContext.TbAccountCodes.Where(x => x.UAcGuid == guid).FirstOrDefaultAsync();

        public async Task<List<TbNormalCode>> getNormalCode() => await _eSignPrpoContext.TbNormalCodes.Where(x => x.AccountName != "" && x.Section != "").ToListAsync();

        public async Task<Tuple<bool, string>> insertAccountCode(AccCodeInsertUpdateModel request)
        {
            try
            {
                var informationData = _accountService.informationUser();
                var insertAccCode = new TbAccountCode
                {
                    UAcGuid = Guid.NewGuid(),
                    MainCode = request?.mainCode,
                    SubCode1 = request?.subCode1,
                    SubCode2 = request?.subCode2,
                    Budget = request?.budget,
                    Balance = request?.balance,
                    Active = request?.active == "true" ? true : false,
                    DCreatedDate = DateTime.Now,
                    SCreatedBy = informationData?.sID



                };
                _eSignPrpoContext.TbAccountCodes.Add(insertAccCode);
                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;


                return Tuple.Create(response, $"Create\n" +
                    $"Main Code : {request?.mainCode}\n" +
                    $"Sub Code 1 : {request?.subCode1}\n" +
                    $"Sub Code 2 : {request?.subCode2} is success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Tuple.Create(false, ex.Message); ;
            }
        }

        public async Task<Tuple<bool, string>> updateAccountCode(AccCodeInsertUpdateModel request)
        {
            try
            {
                var informationData = _accountService.informationUser();

                var responseCus = await GetAccountCodeByGuid(request.accId);


                responseCus.MainCode = request?.mainCode;
                responseCus.SubCode1 = request?.subCode1;
                responseCus.SubCode2 = request?.subCode2;
                responseCus.Budget = request?.budget;
                responseCus.Balance = request?.balance;
                responseCus.Active = request?.active == "true" ? true : false;
                responseCus.DUpdatedBy = DateTime.Now;
                responseCus.SUpdatedBy = informationData?.sID;


                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;


                return Tuple.Create(response, $"Update\n"+
                    $"Main Code: { request?.mainCode}\n" +
                    $"Sub Code 1 : {request?.subCode1}\n" +
                    $"Sub Code 2 : {request?.subCode2} is success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Tuple.Create(false, ex.Message); ;
            }
        }

        public async Task<Tuple<bool, string>> deleteAccountCode(Guid guid)
        {
            try
            {
                var informationData = _accountService.informationUser();

                var getAccCode = await GetAccountCodeByGuid(guid);

                _eSignPrpoContext.TbAccountCodes.Remove(getAccCode);

                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;


                return Tuple.Create(response, $"Delete account code is success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Tuple.Create(false, ex.Message); ;
            }
        }
        public async Task<List<string>> getSubCode1(string mainCode) => await _eSignPrpoContext.TbNormalCodes.Where(x => x.MainCode == mainCode && x.AccountName != "" && x.Section != "").Select(x => x.AccountName).Distinct().ToListAsync();

        public async Task<List<string>> getSubCode2(string subCode1) => await _eSignPrpoContext.TbNormalCodes.Where(x => x.AccountName == subCode1 && x.Section != "").Select(x => x.Section).Distinct().ToListAsync();

    }
}
