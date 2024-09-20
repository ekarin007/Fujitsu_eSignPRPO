using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.AccountCode;

namespace Fujitsu_eSignPO.interfaces
{
    public interface IAccountCodeService
    {
        Task<List<TbAccountCode>> getAccountCode();
        Task<TbAccountCode> GetAccountCodeByGuid(Guid? guid);
        Task<List<TbNormalCode>> getNormalCode();
        Task<Tuple<bool, string>> deleteAccountCode(Guid guid);

        Task<Tuple<bool, string>> insertAccountCode(AccCodeInsertUpdateModel request);

        Task<Tuple<bool, string>> updateAccountCode(AccCodeInsertUpdateModel request);

        Task<List<string>> getSubCode1(string mainCode);
        Task<List<string>> getSubCode2(string mainCode);

    }
}
