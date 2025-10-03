using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core.Common
{
    internal class Fight
    {
        public double TotalTime { get; set; } = 0;
        public int Id {  get; set; }
        public int EncounterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReportCode { get; set; } = String.Empty;

    }
}
