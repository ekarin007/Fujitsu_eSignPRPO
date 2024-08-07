using System.Xml.Linq;

namespace eSignPRPO.Models.PRPO
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
