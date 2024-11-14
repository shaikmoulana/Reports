using DataServices.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebinarsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebinarController : ControllerBase
    {
        private readonly DataBaseContext _context;

        public WebinarController(DataBaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetWebinars()
        {
            var webinars = await _context.ReportWebinar.Include(w => w.Speaker).ToListAsync();
            return Ok(webinars);
        }

        [HttpGet("generateReport")]
        public IActionResult GenerateReport(string category, string employeeId, string period, string month = null, string quarter = null, int? year = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (category == "webinars")
            {
                var query = _context.ReportWebinar.AsQueryable();

                if (!string.IsNullOrEmpty(employeeId))
                {
                    query = query.Where(w => w.SpeakerId == employeeId);
                }

                if (period == "monthly" && month != null && year.HasValue)
                {
                    // Convert the month name to an integer (e.g., "July" -> 7)
                    int monthNumber = GetMonthNumber(month);
                    if (monthNumber == 0)
                    {
                        return BadRequest("Invalid month name.");
                    }

                    query = query.Where(w => w.CreatedDate.Month == monthNumber &&
                                             w.CreatedDate.Year == year.Value);
                }
                else if (period == "quarterly" && quarter != null && year.HasValue)
                {
                    var quarterMonths = GetQuarterMonths(quarter);
                    query = query.Where(w => quarterMonths.Contains(w.CreatedDate.Month) && w.CreatedDate.Year == year.Value);
                }
                else if (period == "yearly" && year.HasValue)
                {
                    query = query.Where(w => w.CreatedDate.Year == year.Value);
                }
                else if (period == "specificDates" && fromDate.HasValue && toDate.HasValue)
                {
                    query = query.Where(w => w.CreatedDate >= fromDate.Value && w.CreatedDate <= toDate.Value);
                }

                // Select only the fields you need, including Speaker.Name
                var reports = query
                    .Select(w => new
                    {
                        w.Id,
                        w.Title,
                        SpeakerName = w.Speaker != null ? w.Speaker.Name : "No speaker", // Access Speaker.Name directly
                        w.CreatedDate
                    })
                    .ToList();

                return Ok(reports);
            }

            return BadRequest("Invalid category");
        }

        // Helper method to convert month name to month number
        private int GetMonthNumber(string month)
        {
            var months = new Dictionary<string, int>
    {
        { "January", 1 },
        { "February", 2 },
        { "March", 3 },
        { "April", 4 },
        { "May", 5 },
        { "June", 6 },
        { "July", 7 },
        { "August", 8 },
        { "September", 9 },
        { "October", 10 },
        { "November", 11 },
        { "December", 12 }
    };

            return months.TryGetValue(month, out int monthNumber) ? monthNumber : 0;
        }

        // Helper method to get months for a given quarter
        private List<int> GetQuarterMonths(string quarter)
        {
            return quarter switch
            {
                "Q1" => new List<int> { 1, 2, 3 },
                "Q2" => new List<int> { 4, 5, 6 },
                "Q3" => new List<int> { 7, 8, 9 },
                "Q4" => new List<int> { 10, 11, 12 },
                _ => new List<int>()
            };
        }


    }
}
