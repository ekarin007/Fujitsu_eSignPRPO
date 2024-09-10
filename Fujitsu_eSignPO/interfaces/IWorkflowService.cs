using Fujitsu_eSignPO.Models;
using Fujitsu_eSignPO.Models.Account;

namespace Fujitsu_eSignPO.interfaces
{
    public interface IWorkflowService
    {
        List<TbFlow> getStatusFlow();
        Task<bool> generateWorkflow(string department, string prNo);
        Task<bool> approveRejectFlow(informationData informationData, string remark, string prNo, int approveStatus);
        Task<bool> approveReprocessFlow(informationData informationData, string remark, string prNo, int approveStatus);
       // Task<Tuple<bool, string>> convertPOFlow(informationData informationData, string remark, string prNo, int approveStatus);
        Task<byte[]> generateFile(string prNo);

        //Task<bool> isVat(string prNo, string isChecked);
    }
}
