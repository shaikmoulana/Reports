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

        [HttpPost("generateReport")]
        public IActionResult GenerateReport([FromBody] ReportRequest request)
        {
            IQueryable<object> query;

            // Select the appropriate table and filter by employee reference
            switch (request.Category.ToLower())
            {
                case "webinars":
                    var webinarQuery = _context.ReportWebinar.AsQueryable();
                    if (request.EmployeeIds != null && request.EmployeeIds.Any())
                    {
                        webinarQuery = webinarQuery.Where(w => request.EmployeeIds.Contains(w.SpeakerId));
                    }
                    query = webinarQuery.Cast<object>();
                    break;

                case "blogs":
                    var blogQuery = _context.ReportBlog.AsQueryable();
                    if (request.EmployeeIds != null && request.EmployeeIds.Any())
                    {
                        blogQuery = blogQuery.Where(b => request.EmployeeIds.Contains(b.AuthorId));
                    }
                    query = blogQuery.Cast<object>();
                    break;

                // Add other cases as needed for each category with specific reference column
                default:
                    return BadRequest("Invalid category.");
            }

            // Apply date filters based on the period type
            if (request.Year.HasValue)
            {
                if (request.Period == "yearly")
                {
                    query = query.Where(w => EF.Property<DateTime>(w, "CreatedDate").Year == request.Year.Value);
                }
                else if (request.Period == "quarterly" && request.Quarter != null)
                {
                    var quarterMonths = GetQuarterMonths(request.Quarter);
                    query = query.Where(w => quarterMonths.Contains(EF.Property<DateTime>(w, "CreatedDate").Month) &&
                                             EF.Property<DateTime>(w, "CreatedDate").Year == request.Year.Value);
                }
                else if (request.Period == "monthly" && request.Month != null)
                {
                    int? monthNumber = GetMonthNumber(request.Month);
                    if (!monthNumber.HasValue)
                    {
                        return BadRequest("Invalid month name.");
                    }
                    query = query.Where(w => EF.Property<DateTime>(w, "CreatedDate").Month == monthNumber.Value &&
                                             EF.Property<DateTime>(w, "CreatedDate").Year == request.Year.Value);
                }
            }
            else if (request.Period == "specificDates" && request.FromDate.HasValue && request.ToDate.HasValue)
            {
                query = query.Where(w => EF.Property<DateTime>(w, "CreatedDate") >= request.FromDate.Value &&
                                         EF.Property<DateTime>(w, "CreatedDate") <= request.ToDate.Value);
            }

            // Execute query and map results to the desired output format
            var reports = query
                .Select(w => new
                {
                    Id = EF.Property<string>(w, "Id"),
                    Title = EF.Property<string>(w, "Title"),
                    ReferenceName = GetReferenceName(request.Category, w),
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
                    return webinar?.SpeakerId ?? "No reference";
                case "blogs":
                    var blog = w as Blogs;
                    return blog?.AuthorId ?? "No reference";
                default:
                    return "No reference";
            }
        }
    }
}