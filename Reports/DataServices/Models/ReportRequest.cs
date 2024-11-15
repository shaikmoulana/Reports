using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServices.Models
{
    public class ReportRequest
    {
        public List<string> Categories { get; set; }
        public List<string>? EmployeeIds { get; set; }
        public string Period { get; set; }
        public string? Month { get; set; }
        public string? Quarter { get; set; }
        public int? Year { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
