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

        public ChatbotController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
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
    }
}