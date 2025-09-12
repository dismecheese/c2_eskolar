using c2_eskolar.Data;
using c2_eskolar.Models;
using c2_eskolar.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Services
{
    public class AnnouncementService
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get announcements for students (public + active)
        public async Task<List<Announcement>> GetPublicAnnouncementsAsync()
        {
            return await _context.Announcements
                .Where(a => a.IsPublic && a.IsActive && 
                           (a.PublishDate == null || a.PublishDate <= DateTime.Now) &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now))
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // Get active announcements (for student view)
        public async Task<List<Announcement>> GetActiveAnnouncementsAsync()
        {
            return await _context.Announcements
                .Where(a => a.IsActive && 
                           (a.PublishDate == null || a.PublishDate <= DateTime.Now) &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now))
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // Get ALL announcements (for shared viewing across roles)
        public async Task<List<Announcement>> GetAllAnnouncementsAsync()
        {
            return await _context.Announcements
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // Search announcements by text
        public async Task<List<Announcement>> SearchAnnouncementsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveAnnouncementsAsync();

            return await _context.Announcements
                .Where(a => a.IsActive && 
                           (a.PublishDate == null || a.PublishDate <= DateTime.Now) &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now) &&
                           (a.Title.Contains(searchTerm) || 
                            a.Content.Contains(searchTerm) ||
                            a.AuthorName.Contains(searchTerm) ||
                            (a.Category != null && a.Category.Contains(searchTerm))))
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // Get announcements by category
        public async Task<List<Announcement>> GetAnnouncementsByCategoryAsync(string category)
        {
            return await _context.Announcements
                .Where(a => a.IsActive && 
                           (a.PublishDate == null || a.PublishDate <= DateTime.Now) &&
                           (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now) &&
                           (a.Category != null && a.Category.Contains(category)))
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // Get announcements by author (for admin pages)
        public async Task<List<Announcement>> GetAnnouncementsByAuthorAsync(string authorId)
        {
            return await _context.Announcements
                .Where(a => a.AuthorId == authorId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // Get announcements by author type (Institution/Benefactor)
        public async Task<List<Announcement>> GetAnnouncementsByAuthorTypeAsync(string authorType)
        {
            if (Enum.TryParse<UserRole>(authorType, out var userRole))
            {
                return await _context.Announcements
                    .Where(a => a.AuthorType == userRole)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }
            return new List<Announcement>();
        }

        // Create new announcement
        public async Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
        {
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();
            return announcement;
        }

        // Update announcement
        public async Task<Announcement?> UpdateAnnouncementAsync(int id, Announcement updatedAnnouncement)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return null;

            announcement.Title = updatedAnnouncement.Title;
            announcement.Content = updatedAnnouncement.Content;
            announcement.Summary = updatedAnnouncement.Summary;
            announcement.Category = updatedAnnouncement.Category;
            announcement.OrganizationName = updatedAnnouncement.OrganizationName;
            announcement.Priority = updatedAnnouncement.Priority;
            announcement.IsPublic = updatedAnnouncement.IsPublic;
            announcement.IsPinned = updatedAnnouncement.IsPinned;
            announcement.PublishDate = updatedAnnouncement.PublishDate;
            announcement.ExpiryDate = updatedAnnouncement.ExpiryDate;
            announcement.Tags = updatedAnnouncement.Tags;
            announcement.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return announcement;
        }

        // Update announcement (overload with object parameter)
        public async Task<Announcement?> UpdateAnnouncementAsync(Announcement updatedAnnouncement)
        {
            var announcement = await _context.Announcements.FindAsync(updatedAnnouncement.AnnouncementId);
            if (announcement == null) return null;

            announcement.Title = updatedAnnouncement.Title;
            announcement.Content = updatedAnnouncement.Content;
            announcement.Summary = updatedAnnouncement.Summary;
            announcement.Category = updatedAnnouncement.Category;
            announcement.OrganizationName = updatedAnnouncement.OrganizationName;
            announcement.Priority = updatedAnnouncement.Priority;
            announcement.IsActive = updatedAnnouncement.IsActive;
            announcement.IsPinned = updatedAnnouncement.IsPinned;
            announcement.PublishDate = updatedAnnouncement.PublishDate;
            announcement.ExpiryDate = updatedAnnouncement.ExpiryDate;
            announcement.Tags = updatedAnnouncement.Tags;
            announcement.UpdatedAt = updatedAnnouncement.UpdatedAt;

            await _context.SaveChangesAsync();
            return announcement;
        }

        // Delete announcement
        public async Task<bool> DeleteAnnouncementAsync(int id, string authorId)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == id && a.AuthorId == authorId);
            
            if (announcement == null) return false;

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return true;
        }

        // Delete announcement (overload with just ID)
        public async Task<bool> DeleteAnnouncementAsync(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return false;

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get single announcement
        public async Task<Announcement?> GetAnnouncementByIdAsync(int id)
        {
            return await _context.Announcements.FindAsync(id);
        }

        // Toggle pin status
        public async Task<bool> TogglePinAsync(int id, string authorId)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == id && a.AuthorId == authorId);
            
            if (announcement == null) return false;

            announcement.IsPinned = !announcement.IsPinned;
            announcement.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        // Increment view count
        public async Task IncrementViewCountAsync(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                announcement.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }

        // Check if user can manage announcement (authorization helper)
        public async Task<bool> CanUserManageAnnouncementAsync(int announcementId, string userId)
        {
            return await _context.Announcements
                .AnyAsync(a => a.AnnouncementId == announcementId && a.AuthorId == userId);
        }

        // Toggle active status (for soft delete/archive)
        public async Task<bool> ToggleActiveAsync(int id, string authorId)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == id && a.AuthorId == authorId);
            
            if (announcement == null) return false;

            announcement.IsActive = !announcement.IsActive;
            announcement.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
