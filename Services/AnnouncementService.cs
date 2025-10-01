using c2_eskolar.Data;
using c2_eskolar.Models;
using c2_eskolar.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Services
{
    public class AnnouncementService
    {
        // Database context for accessing announcements table
        private readonly ApplicationDbContext _context;

        // Constructor injects the database context (Dependency Injection)
        public AnnouncementService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        // GET ANNOUNCEMENTS

        // Get announcements visible to students (public + active + within publish/expiry date)
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

        // Get announcements that are still active (ignores public/private restriction)
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

        // Get all announcements (ignores active/public checks — admin view)
        public async Task<List<Announcement>> GetAllAnnouncementsAsync()
        {
            return await _context.Announcements
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // Search announcements by keyword (title, content, author, category)
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

        // Get announcements filtered by category (ex: "Funding", "Applications")
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

        // Get announcements by author ID (used for admin management)
        public async Task<List<Announcement>> GetAnnouncementsByAuthorAsync(string authorId)
        {
            return await _context.Announcements
                .Where(a => a.AuthorId == authorId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // Get announcements by author type (Institution / Benefactor)
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

        // Get single announcement
        public async Task<Announcement?> GetAnnouncementByIdAsync(Guid id)
        {
            return await _context.Announcements.FindAsync(id);
        }

        // CREATE / UPDATE

        // Create new announcement
        public async Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
        {
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();
            return announcement;
        }

        // Update announcement
        public async Task<Announcement?> UpdateAnnouncementAsync(Guid id, Announcement updatedAnnouncement)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return null;

            // Copy fields from updated object
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
            announcement.UpdatedAt = DateTime.UtcNow;

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

        // DELETE
         
        // Delete announcement if it belongs to given author
        public async Task<bool> DeleteAnnouncementAsync(Guid id, string authorId)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == id && a.AuthorId == authorId);

            if (announcement == null) return false;

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return true;
        }

        // Delete announcement by ID only (admin override) (overload with just ID)
        public async Task<bool> DeleteAnnouncementAsync(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return false;

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return true;
        }

        // MANAGEMENT / HELPERS

        // Toggle pin status (pinned items always appear at top)
        public async Task<bool> TogglePinAsync(Guid id, string authorId)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == id && a.AuthorId == authorId);

            if (announcement == null) return false;

            announcement.IsPinned = !announcement.IsPinned;
            announcement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Increment view count (tracks how many times announcement viewed)
        public async Task IncrementViewCountAsync(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                announcement.ViewCount++;
                await _context.SaveChangesAsync();
            }
        }

        // Check if a user is the author (authorization helper)
        public async Task<bool> CanUserManageAnnouncementAsync(Guid announcementId, string userId)
        {
            return await _context.Announcements
                .AnyAsync(a => a.AnnouncementId == announcementId && a.AuthorId == userId);
        }

        // Toggle active/inactive status (used for archiving instead of hard delete)
        public async Task<bool> ToggleActiveAsync(Guid id, string authorId)
        {
            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == id && a.AuthorId == authorId);
            
            if (announcement == null) return false;

            announcement.IsActive = !announcement.IsActive;
            announcement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
