using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServices.Models
{
    public class Webinars : AuditData
    {
        public string Title { get; set; }
        public string SpeakerId { get; set; }
        public Employees Speaker { get; set; }
    }
}
