using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using c2_eskolar.Data;
using c2_eskolar.Hubs;

namespace c2_eskolar.Services
{
    public class InAppNotification
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } = "info";
    }

    // DTO returned by the API for persisted notifications
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string UserId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } = "info";
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IDbContextFactory<c2_eskolar.Data.ApplicationDbContext> _dbFactory;

        // Keep an in-memory cap for immediate push/cache (optional)
        private readonly ConcurrentDictionary<string, List<InAppNotification>> _store = new();

        public NotificationService(IHubContext<NotificationHub> hubContext, IDbContextFactory<c2_eskolar.Data.ApplicationDbContext> dbFactory)
        {
            _hubContext = hubContext;
            _dbFactory = dbFactory;
        }

        public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(string userId, int page = 1, int pageSize = 50)
        {
            if (string.IsNullOrEmpty(userId)) return Array.Empty<NotificationDto>();
            // First try the in-memory store for immediate items
            if (_store.TryGetValue(userId, out var list) && list.Count > 0)
            {
                // Map in-memory items to DTOs
                var fromMemory = list.Select((i, idx) => new NotificationDto
                {
                    NotificationId = -1 - idx, // temporary negative id for in-memory items
                    UserId = userId,
                    Title = i.Title,
                    Message = i.Message,
                    CreatedAt = i.CreatedAt,
                    Type = i.Type,
                    IsRead = false
                }).ToList();
                return fromMemory.AsReadOnly();
            }

            // Fallback to DB
            await using var db = _dbFactory.CreateDbContext();
            var notifications = await db.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                Type = n.Type,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt
            }).ToList();

            return result.AsReadOnly();
        }

        public async Task ClearForUserAsync(string userId)
        {
            _store.TryRemove(userId, out _);
            await using var db = _dbFactory.CreateDbContext();
            var list = db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToList();
            if (list.Any()) db.Notifications.RemoveRange(list);
            await db.SaveChangesAsync();
        }

        public async Task SendToUserAsync(string userId, InAppNotification notification)
        {
            if (string.IsNullOrEmpty(userId) || notification == null) return;

            // Persist to DB
            await using (var db = _dbFactory.CreateDbContext())
            {
                var entity = new c2_eskolar.Models.Notification
                {
                    UserId = userId,
                    Title = notification.Title,
                    Message = notification.Message,
                    CreatedAt = notification.CreatedAt,
                    Type = notification.Type,
                    IsRead = false
                };
                db.Notifications.Add(entity);
                await db.SaveChangesAsync();
            }

            // Keep small in-memory cache for immediate visibility
            var list = _store.GetOrAdd(userId, _ => new List<InAppNotification>());
            lock (list)
            {
                list.Insert(0, notification);
                if (list.Count > 50) list.RemoveRange(50, list.Count - 50);
            }

            // Send via SignalR to specific user (assumes default IUserIdProvider based on ClaimsPrincipal NameIdentifier)
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }

        // Push-only helper: send a notification to connected user(s) without persisting to the DB.
        // Use this when the caller already persisted a Notification entity in the same DbContext/transaction
        // and only wants to broadcast the payload to real-time clients.
        public async Task PushToUserAsync(string userId, InAppNotification notification)
        {
            if (string.IsNullOrEmpty(userId) || notification == null) return;

            // Update in-memory cache for immediate visibility (optional)
            var list = _store.GetOrAdd(userId, _ => new List<InAppNotification>());
            lock (list)
            {
                list.Insert(0, notification);
                if (list.Count > 50) list.RemoveRange(50, list.Count - 50);
            }

            // Broadcast via SignalR to the specific user
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            await using var db = _dbFactory.CreateDbContext();
            var notif = await db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);
            if (notif == null) return;
            notif.IsRead = true;
            notif.ReadAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            // Update in-memory store
            if (_store.TryGetValue(userId, out var list))
            {
                lock (list)
                {
                    var toRemove = list.FirstOrDefault(i => i.Title == notif.Title && i.Message == notif.Message && i.CreatedAt == notif.CreatedAt);
                    if (toRemove != null) list.Remove(toRemove);
                }
            }
        }
    }
}
