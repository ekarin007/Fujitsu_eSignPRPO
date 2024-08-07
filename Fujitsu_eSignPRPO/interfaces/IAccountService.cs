using eSignPRPO.Models;
using eSignPRPO.Models.Account;
using eSignPRPO.Models.Login;

namespace eSignPRPO.interfaces
{
    public interface IAccountService
    {
        Task<TbEmployee> checkLoginUser(Credential credential);
        Task<TbCustomer> checkSupplierLogin(Credential credential);
        informationData informationUser();
    }
}
