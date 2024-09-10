using System.Xml.Linq;

namespace Fujitsu_eSignPO.Models.PRPO
{
    public class CusSupResponse
    {
        public string TCcur { get; set; }
        public string TBpid { get; set; }
        public string TNama { get; set; }
        public string TSeak { get; set; }

        public double? TRate { get; set; }
    }
}
