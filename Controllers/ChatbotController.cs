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
            var aiResponse = await _openAIService.GetChatCompletionWithProfileAsync(request.Message, user, request.IsFirstMessage);
            return Ok(new { response = aiResponse });
        }

        public class ChatRequestDto
        {
            public string? Message { get; set; }
            public bool IsFirstMessage { get; set; } = false;
        }

        [HttpGet("start-message")]
        [Authorize]
        public async Task<IActionResult> GetStartMessage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var firstName = await _profileSummaryService.GetUserFirstNameAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            
            string greeting = !string.IsNullOrEmpty(firstName) 
                ? $"Hello {firstName}! " 
                : "Hello! ";
            
            string message = greeting + "How can I help you today?";

            if (roles.Contains("Student"))
                message = greeting + "I can help you find scholarships that match your profile, answer questions about your applications, or provide guidance on the platform. What would you like to know?";
            else if (roles.Contains("Benefactor"))
                message = greeting + "I can help you manage your scholarships, review applicants, or answer questions about the platform. How can I assist you?";
            else if (roles.Contains("Institution"))
                message = greeting + "I can help you with your managed scholarships, partnerships, or provide platform guidance. What do you need help with?";

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