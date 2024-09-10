using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.Profiles;

namespace Fujitsu_eSignPO.interfaces
{
    public interface IProfilesService
    {
        Task<Tuple<bool, string>> resetPaswordSupplier(ResetPasswordModel request);
        Task<Tuple<bool, string>> resetPaswordEmp(ResetPasswordModel request);
        Task<bool> updateSignature(List<IFormFile> files, string empId);
        Task<List<TbEmployee>> getSignature(string empId);
        Task<TbEmployee> getEmpByID(string userName);
        Task<bool> DeleteFile(string fileName, string empId);
    }
}
