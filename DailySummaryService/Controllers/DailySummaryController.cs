using Microsoft.AspNetCore.Mvc;
using CashFlowControl.Application.Services;
using System;
using System.Threading.Tasks;

namespace CashFlowControl.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DailySummaryController : ControllerBase
    {
        private readonly DailySummaryService _dailySummaryService;

        public DailySummaryController(DailySummaryService dailySummaryService)
        {
            _dailySummaryService = dailySummaryService;
        }

        // GET: api/DailySummary/{date}
        [HttpGet("{date}")]
        public async Task<IActionResult> GetDailySummary(DateTime date)
        {
            var summary = await _dailySummaryService.GetDailySummary(date);
            if (summary == null)
            {
                return NotFound();
            }
            return Ok(summary);
        }
    }
}
