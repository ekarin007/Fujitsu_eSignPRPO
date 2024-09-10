using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.Customer;

namespace Fujitsu_eSignPO.interfaces
{
    public interface ICustomerService
    {
        Task<List<TbCustomer>> getCustomer();
        Task<TbCustomer> getCustomerBySupID(string supID);
        Task<Tuple<bool, string>> insertCustomer(CustomerInsertUpdateModel request);
        Task<Tuple<bool, string>> updateCustomer(CustomerInsertUpdateModel request);
        Task<Tuple<bool, string>> deleteCustomer(string supID);
    }
}
