using DataServices.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServices.Data
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options) { }

        public DbSet<Webinars> ReportWebinar { get; set; }
        public DbSet<Employees> ReportEmployee { get; set; }
        public DbSet<Blogs> ReportBlog { get; set; }

    }
}
