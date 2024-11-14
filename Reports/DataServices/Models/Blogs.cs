using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServices.Models
{
    public class Blogs : AuditData
    {
        public string Title { get; set; }
        public string AuthorId { get; set; }
        public Employees Author { get; set; }
    }
}
