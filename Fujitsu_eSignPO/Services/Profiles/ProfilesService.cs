using Fujitsu_eSignPO.Data;
using Fujitsu_eSignPO.interfaces;
using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.Profiles;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Fujitsu_eSignPO.Services.Profiles
{
    public class ProfilesService : IProfilesService
    {
        private readonly IAccountService _accountService;
        private static FgdtESignPoContext _eSignPrpoContext;
        private ICustomerService _customerService;
        private readonly ILogger<ProfilesService> _logger;

        public ProfilesService(IAccountService accountService, ICustomerService customerService, ILogger<ProfilesService> logger, FgdtESignPoContext eSignPrpoContext)
        {
            _accountService = accountService;
            _customerService = customerService;
            _logger = logger;
            _eSignPrpoContext = eSignPrpoContext;

        }

        public async Task<TbEmployee> getEmpByID(string userName) => await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpUsername == userName).FirstOrDefaultAsync();


        public async Task<Tuple<bool, string>> resetPaswordSupplier(ResetPasswordModel request)
        {
            try
            {
                var informationData = _accountService.informationUser();

                var responseCus = await _customerService.getCustomerBySupID(request?.userName);


                responseCus.SCusPassword = request?.newPassword;
                responseCus.DUpdated = DateTime.Now;
                responseCus.SUpdatedBy = informationData?.sID;


                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;


                return Tuple.Create(response, $"Reset Pasword : {request?.userName} is success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Tuple.Create(false, ex.Message); ;
            }
        }

        public async Task<Tuple<bool, string>> resetPaswordEmp(ResetPasswordModel request)
        {
            try
            {
                var informationData = _accountService.informationUser();

                var responseCus = await getEmpByID(request?.userName);


                responseCus.SEmpPassword = request?.newPassword;
                responseCus.DUpdated = DateTime.Now;
                responseCus.SUpdatedBy = informationData?.sID;


                var response = await _eSignPrpoContext.SaveChangesAsync() > 0;


                return Tuple.Create(response, $"Reset Pasword : {request?.userName} is success.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Tuple.Create(false, ex.Message); ;
            }
        }

        public async Task<bool> updateSignature(List<IFormFile> files, string empId)
        {
            var employees = new List<TbEmployee>();


            var getDataByEmpId = await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpUsername == empId).FirstOrDefaultAsync();

            getDataByEmpId.SSignature = empId + ".png";
            getDataByEmpId.DUpdated = DateTime.Now;
            getDataByEmpId.SUpdatedBy = empId;

            var response = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return response;
        }

        public async Task<List<TbEmployee>> getSignature(string empId) => await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpUsername == empId).ToListAsync();

        public async Task<bool> DeleteFile(string fileName, string empId)
        {
            var deleteFile = await _eSignPrpoContext.TbEmployees.Where(x => x.SEmpUsername == empId).FirstOrDefaultAsync();

            deleteFile.SSignature = null;
            var response = await _eSignPrpoContext.SaveChangesAsync() > 0;
            return response;

        }
    }
}
