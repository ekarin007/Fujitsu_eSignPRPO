using eSignPRPO.Models;
using eSignPRPO.Models.Customer;

namespace eSignPRPO.interfaces
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
