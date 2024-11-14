using DataServices.Data;
using DataServices.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenerateReportsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly DataBaseContext _context;

        public ReportsController(DataBaseContext context)
        {
            _context = context;
        }

        [HttpGet("generateReport")]
        public IActionResult GenerateReport(string category, string employeeId, string period, string month = null, string quarter = null, int? year = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            IQueryable<object> query;

            // Select the appropriate table and filter by employee reference
            switch (category.ToLower())
            {
                case "webinars":
                    var webinarQuery = _context.ReportWebinar.AsQueryable();
                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        webinarQuery = webinarQuery.Where(w => w.SpeakerId == employeeId);
                    }
                    query = webinarQuery.Cast<object>();
                    break;

                case "blogs":
                    var blogQuery = _context.ReportBlog.AsQueryable();
                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        blogQuery = blogQuery.Where(b => b.AuthorId == employeeId);
                    }
                    query = blogQuery.Cast<object>();
                    break;

                // Add other cases as needed for each category with specific reference column
                default:
                    return BadRequest("Invalid category.");
            }

            // Apply date filters based on the period type
            if (year.HasValue)
            {
                // Step 1: Filter by year only if period is "yearly"
                if (period == "yearly")
                {
                    query = query.Where(w => EF.Property<DateTime>(w, "CreatedDate").Year == year.Value);
                }
                // Step 2: Filter by year and quarter if period is "quarterly" and quarter is provided
                else if (period == "quarterly" && quarter != null)
                {
                    var quarterMonths = GetQuarterMonths(quarter);
                    query = query.Where(w => quarterMonths.Contains(EF.Property<DateTime>(w, "CreatedDate").Month) &&
                                             EF.Property<DateTime>(w, "CreatedDate").Year == year.Value);
                }
                // Step 3: Filter by year, quarter, and month if period is "monthly" and month is provided
                else if (period == "monthly" && month != null)
                {
                    int? monthNumber = GetMonthNumber(month);
                    if (!monthNumber.HasValue)
                    {
                        return BadRequest("Invalid month name.");
                    }
                    query = query.Where(w => EF.Property<DateTime>(w, "CreatedDate").Month == monthNumber.Value &&
                                             EF.Property<DateTime>(w, "CreatedDate").Year == year.Value);
                }
            }

            // Step 4: Filter by specific date range if period is "specificDates" and both dates are provided
            else if (period == "specificDates" && fromDate.HasValue && toDate.HasValue)
            {
                query = query.Where(w => EF.Property<DateTime>(w, "CreatedDate") >= fromDate.Value &&
                                         EF.Property<DateTime>(w, "CreatedDate") <= toDate.Value);
            }

            // Execute query and map results to the desired output format
            var reports = query
                .Select(w => new
                {
                    Id = EF.Property<string>(w, "Id"),
                    Title = EF.Property<string>(w, "Title"),
                    ReferenceName = GetReferenceName(category, w),
                    CreatedDate = EF.Property<DateTime>(w, "CreatedDate")
                })
                .ToList();

            return Ok(reports);
        }

        // Helper method to convert month name to month number
        private int? GetMonthNumber(string month)
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

            return months.TryGetValue(month, out int monthNumber) ? monthNumber : (int?)null;
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

        // Helper method to determine the reference name based on category
        public static string GetReferenceName(string category, object w)
        {
            switch (category.ToLower())
            {
                case "webinars":
                    var webinar = w as Webinars;
                    return webinar?.SpeakerId?? "No reference";
                case "blogs":
                    var blog = w as Blogs;
                    return blog?.AuthorId ?? "No reference";
                default:
                    return "No reference";
            }
        }
    }
}
