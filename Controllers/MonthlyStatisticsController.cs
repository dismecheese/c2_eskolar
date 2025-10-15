using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using c2_eskolar.Services;
using Microsoft.Extensions.Logging;

namespace c2_eskolar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class MonthlyStatisticsController : ControllerBase
    {
        private readonly MonthlyStatisticsService _statisticsService;
        private readonly ILogger<MonthlyStatisticsController> _logger;

        public MonthlyStatisticsController(
            MonthlyStatisticsService statisticsService,
            ILogger<MonthlyStatisticsController> logger)
        {
            _statisticsService = statisticsService;
            _logger = logger;
        }

        /// <summary>
        /// Manually trigger backfill of all historical monthly data
        /// </summary>
        [HttpPost("backfill")]
        public async Task<IActionResult> BackfillHistoricalData()
        {
            try
            {
                _logger.LogInformation("Manual backfill triggered by SuperAdmin");
                await _statisticsService.BackfillHistoricalDataAsync();
                return Ok(new { message = "Historical data backfill completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual backfill");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Aggregate data for a specific month
        /// </summary>
        [HttpPost("aggregate/{year}/{month}")]
        public async Task<IActionResult> AggregateMonth(int year, int month)
        {
            try
            {
                if (month < 1 || month > 12)
                    return BadRequest(new { error = "Month must be between 1 and 12" });

                _logger.LogInformation($"Manual aggregation triggered for {year}-{month:D2}");
                var result = await _statisticsService.AggregateMonthlyDataAsync(year, month);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error aggregating {year}-{month:D2}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get monthly statistics for last N months
        /// </summary>
        [HttpGet("recent/{monthsBack?}")]
        public async Task<IActionResult> GetRecentStatistics(int monthsBack = 6)
        {
            try
            {
                var stats = await _statisticsService.GetMonthlyStatisticsAsync(monthsBack);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly statistics");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
