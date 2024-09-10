using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.Account;
using Fujitsu_eSignPO.Models.Login;

namespace Fujitsu_eSignPO.interfaces
{
    public interface IAccountService
    {
        Task<TbEmployee> checkLoginUser(Credential credential);
        Task<TbCustomer> checkSupplierLogin(Credential credential);
        informationData informationUser();

       
    }
}
