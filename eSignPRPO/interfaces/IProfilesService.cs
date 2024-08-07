using eSignPRPO.Models;
using eSignPRPO.Models.Profiles;

namespace eSignPRPO.interfaces
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
