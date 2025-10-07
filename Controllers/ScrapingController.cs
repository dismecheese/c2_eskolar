using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using c2_eskolar.Services.WebScraping;
using Microsoft.Extensions.Logging;

namespace c2_eskolar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only admins can trigger scraping
    public class ScrapingController : ControllerBase
    {
        private readonly IWebScrapingService _scrapingService;
        private readonly ILogger<ScrapingController> _logger;

        public ScrapingController(IWebScrapingService scrapingService, ILogger<ScrapingController> logger)
        {
            _scrapingService = scrapingService;
            _logger = logger;
        }

        [HttpPost("scholarships")]
        public async Task<IActionResult> ScrapeScholarships([FromBody] ScrapeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Url))
                {
                    return BadRequest("URL is required");
                }

                var scholarships = await _scrapingService.ScrapeScholarshipsAsync(request.Url);
                
                return Ok(new
                {
                    Success = true,
                    Count = scholarships.Count,
                    Scholarships = scholarships
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scraping scholarships from {request.Url}");
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("verify-institution")]
        public async Task<IActionResult> VerifyInstitution([FromBody] VerifyInstitutionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InstitutionName))
                {
                    return BadRequest("Institution name is required");
                }

                var result = await _scrapingService.VerifyInstitutionAsync(request.InstitutionName, request.Website);
                
                return Ok(new
                {
                    Success = true,
                    Result = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying institution {request.InstitutionName}");
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("news")]
        public async Task<IActionResult> ScrapeNews()
        {
            try
            {
                var news = await _scrapingService.ScrapeScholarshipNewsAsync();
                
                return Ok(new
                {
                    Success = true,
                    Count = news.Count,
                    News = news
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scraping scholarship news");
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("verify-organization")]
        public async Task<IActionResult> VerifyOrganization([FromBody] VerifyOrganizationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.OrganizationName))
                {
                    return BadRequest("Organization name is required");
                }

                var isVerified = await _scrapingService.VerifyOrganizationAsync(request.OrganizationName, request.Website);
                
                return Ok(new
                {
                    Success = true,
                    IsVerified = isVerified
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying organization {request.OrganizationName}");
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
    }

    public class ScrapeRequest
    {
        public string Url { get; set; } = "";
    }

    public class VerifyInstitutionRequest
    {
        public string InstitutionName { get; set; } = "";
        public string Website { get; set; } = "";
    }

    public class VerifyOrganizationRequest
    {
        public string OrganizationName { get; set; } = "";
        public string Website { get; set; } = "";
    }
}