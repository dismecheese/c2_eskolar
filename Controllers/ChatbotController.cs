using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace c2_eskolar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Services.ProfileSummaryService _profileSummaryService;
        private readonly Services.OpenAIService _openAIService;

        public ChatbotController(UserManager<IdentityUser> userManager, Services.ProfileSummaryService profileSummaryService, Services.OpenAIService openAIService)
        {
            _userManager = userManager;
            _profileSummaryService = profileSummaryService;
            _openAIService = openAIService;
        }
        // POST: /api/chatbot/chat
        // Receives a user message and returns an AI response with user profile context
        [HttpPost("chat")]
        [Authorize]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest("Message is required.");
            var aiResponse = await _openAIService.GetChatCompletionWithProfileAsync(request.Message, user);
            return Ok(new { response = aiResponse });
        }

        public class ChatRequestDto
        {
            public string? Message { get; set; }
        }

        [HttpGet("start-message")]
        [Authorize]
        public async Task<IActionResult> GetStartMessage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            string message = "Welcome! How can I help you today?";

            if (roles.Contains("Student"))
                message = "Hi student! Ask me about scholarships, your applications, or how to get started.";
            else if (roles.Contains("Benefactor"))
                message = "Hello benefactor! I can help you manage your scholarships, view applicants, or answer your questions.";
            else if (roles.Contains("Institution"))
                message = "Welcome institution admin! Ask me about your managed scholarships, partnerships, or platform guidance.";

            return Ok(new { message });
        }

        // Returns the current user's profile summary for AI context
        [HttpGet("profile-summary")]
        [Authorize]
        public async Task<IActionResult> GetProfileSummary()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            var summary = await _profileSummaryService.GetProfileSummaryAsync(user);
            if (summary == null)
                return NotFound();
            return Ok(summary);
        }
    }
}