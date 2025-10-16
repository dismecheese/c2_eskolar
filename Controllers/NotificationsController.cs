using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using c2_eskolar.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

// Added endpoints to list and mark notifications

namespace c2_eskolar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationsController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("send/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> SendTestNotification(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest("userId required");

            var note = new InAppNotification
            {
                Title = "Test Notification",
                Message = "This is a test notification sent from the server.",
                CreatedAt = System.DateTime.UtcNow,
                Type = "info"
            };

            await _notificationService.SendToUserAsync(userId, note);
            return Ok(new { success = true });
        }

        // GET api/notifications?page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetNotifications(int page = 1, int pageSize = 50)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var items = await _notificationService.GetForUserAsync(userId, page, pageSize);
            return Ok(items);
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _notificationService.MarkAsReadAsync(id, userId);
            return Ok(new { success = true });
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Naive approach: mark all unread as read
            var items = await _notificationService.GetForUserAsync(userId, 1, 1000);
            // We only stored limited metadata in InAppNotification, so simply clear the user's unread in DB
            await _notificationService.ClearForUserAsync(userId);
            return Ok(new { success = true });
        }
    }
}
