using Microsoft.AspNetCore.Mvc;
using EcoFashionBackEnd.Services;
using EcoFashionBackEnd.Common;

namespace EcoFashionBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("unread-count/{userId}")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var result = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(result);
        }

        [HttpPost("mark-as-read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId, [FromQuery] int userId)
        {
            var result = await _notificationService.MarkAsReadAsync(notificationId, userId);
            return Ok(result);
        }

        [HttpPost("mark-all-as-read/{userId}")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            var result = await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(result);
        }
    }
}
