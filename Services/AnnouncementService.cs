using c2_eskolar.Data;
using c2_eskolar.Models;
using c2_eskolar.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace c2_eskolar.Services
{
    public class AnnouncementService
    {
        // DbContext factory for creating new contexts (Blazor Server concurrency safety)
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        // Constructor injects the database context factory (Dependency Injection)
        public AnnouncementService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }
        
        // GET ANNOUNCEMENTS

        // Get announcements visible to students (public + active + within publish/expiry date)
        public async Task<List<Announcement>> GetPublicAnnouncementsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Announcements
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
            using var context = _contextFactory.CreateDbContext();
            return await context.Announcements
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
            using var context = _contextFactory.CreateDbContext();
            return await context.Announcements
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

            using var context = _contextFactory.CreateDbContext();
            return await context.Announcements
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
            using var context = _contextFactory.CreateDbContext();
            return await context.Announcements
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
            using var context = _contextFactory.CreateDbContext();
            return await context.Announcements
                .Where(a => a.AuthorId == authorId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // Get announcements by author type (Institution / Benefactor)
        public async Task<List<Announcement>> GetAnnouncementsByAuthorTypeAsync(string authorType)
        {
            if (Enum.TryParse<UserRole>(authorType, out var userRole))
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Announcements
                    .Where(a => a.AuthorType == userRole)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }
            return new List<Announcement>();
        }

        // Get single announcement
        public async Task<Announcement?> GetAnnouncementByIdAsync(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Announcements.FindAsync(id);
        }

        // CREATE / UPDATE

        // Create new announcement
        public async Task<Announcement> CreateAnnouncementAsync(Announcement announcement)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Announcements.Add(announcement);
            await context.SaveChangesAsync();
            return announcement;
        }

        // Update announcement
        public async Task<Announcement?> UpdateAnnouncementAsync(Guid id, Announcement updatedAnnouncement)
        {
            using var context = _contextFactory.CreateDbContext();
            var announcement = await context.Announcements.FindAsync(id);
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

            await context.SaveChangesAsync();
            return announcement;
        }

        // Update announcement (overload with object parameter)
        public async Task<Announcement?> UpdateAnnouncementAsync(Announcement updatedAnnouncement)
        {
            using var context = _contextFactory.CreateDbContext();
            var announcement = await context.Announcements.FindAsync(updatedAnnouncement.AnnouncementId);
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

            await context.SaveChangesAsync();
            return announcement;
        }

        // DELETE
         
        // Delete announcement if it belongs to given author
        public async Task<bool> DeleteAnnouncementAsync(Guid id, string authorId)
        {
            using var context = _contextFactory.CreateDbContext();
            var announcement = await context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == id && a.AuthorId == authorId);

            if (announcement == null) return false;

            context.Announcements.Remove(announcement);
            await context.SaveChangesAsync();
            return true;
        }

        // Delete announcement by ID only (admin override) (overload with just ID)
        public async Task<bool> DeleteAnnouncementAsync(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();
            var announcement = await context.Announcements.FindAsync(id);
            if (announcement == null) return false;

            context.Announcements.Remove(announcement);
            await context.SaveChangesAsync();
            return true;
        }

        // MANAGEMENT / HELPERS

        // Toggle pin status (pinned items always appear at top)
        public async Task<bool> TogglePinAsync(Guid id, string authorId)
        {
            using var context = _contextFactory.CreateDbContext();
            var announcement = await context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == id && a.AuthorId == authorId);

            if (announcement == null) return false;

            announcement.IsPinned = !announcement.IsPinned;
            announcement.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return true;
        }

        // Increment view count (tracks how many times announcement viewed)
        public async Task IncrementViewCountAsync(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();
            var announcement = await context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                announcement.ViewCount++;
                await context.SaveChangesAsync();
            }
        }

        // Check if a user is the author (authorization helper)
        public async Task<bool> CanUserManageAnnouncementAsync(Guid announcementId, string userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Announcements
                .AnyAsync(a => a.AnnouncementId == announcementId && a.AuthorId == userId);
        }

        // Toggle active/inactive status (used for archiving instead of hard delete)
        public async Task<bool> ToggleActiveAsync(Guid id, string authorId)
        {
            using var context = _contextFactory.CreateDbContext();
            var announcement = await context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == id && a.AuthorId == authorId);
            
            if (announcement == null) return false;

            announcement.IsActive = !announcement.IsActive;
            announcement.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return true;
        }

        // PHOTO MANAGEMENT

        // Add photos to an announcement and persist their display order
        public async Task AddPhotosToAnnouncementAsync(Guid announcementId, List<string> photoUrls)
        {
            // Assign SortOrder based on the list order (0-based)
            var photos = photoUrls.Select((url, index) => new Photo
            {
                PhotoId = Guid.NewGuid(),
                AnnouncementId = announcementId,
                Url = url,
                SortOrder = index,
                UploadedAt = DateTime.UtcNow
            }).ToList();

            using var context = _contextFactory.CreateDbContext();
            context.Photos.AddRange(photos);
            await context.SaveChangesAsync();
        }

        // Remove a photo from an announcement
        public async Task<bool> RemovePhotoFromAnnouncementAsync(Guid photoId)
        {
            using var context = _contextFactory.CreateDbContext();
            var photo = await context.Photos.FindAsync(photoId);
            if (photo == null) return false;

            context.Photos.Remove(photo);
            await context.SaveChangesAsync();
            return true;
        }

        // Get photos for an announcement, ordered by SortOrder with UploadedAt as a fallback
        public async Task<List<Photo>> GetAnnouncementPhotosAsync(Guid announcementId)
        {
            using var context = _contextFactory.CreateDbContext();
            var photos = await context.Photos
                .Where(p => p.AnnouncementId == announcementId)
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.UploadedAt)
                .ToListAsync();

            // Convert blob URLs to streaming URLs
            foreach (var photo in photos)
            {
                var fileName = ExtractFileNameFromUrl(photo.Url);
                if (!string.IsNullOrEmpty(fileName))
                {
                    // URL encode the filename to handle spaces and special characters
                    var encodedFileName = Uri.EscapeDataString(fileName);
                    // Add cache-busting parameter to force browser refresh
                    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    photo.Url = $"/api/photo/stream/{encodedFileName}?v={timestamp}";
                }
                // If fileName is empty, keep the original URL (for external URLs like placeholders)
            }

            return photos;
        }

        // Get announcement with photos included
        public async Task<Announcement?> GetAnnouncementWithPhotosAsync(Guid id)
        {
            using var context = _contextFactory.CreateDbContext();
            var announcement = await context.Announcements
                .Include(a => a.Photos)
                .FirstOrDefaultAsync(a => a.AnnouncementId == id);

            // Ensure photos collection is ordered by SortOrder for consumers
            if (announcement?.Photos != null)
            {
                announcement.Photos = announcement.Photos
                    .OrderBy(p => p.SortOrder)
                    .ThenBy(p => p.UploadedAt)
                    .ToList();
            }

            return announcement;
        }

        // Get all announcements with photos included
        public async Task<List<Announcement>> GetAllAnnouncementsWithPhotosAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            var announcements = await context.Announcements
                .Include(a => a.Photos)
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            // Convert blob URLs to streaming URLs and ensure photo ordering by SortOrder
            foreach (var announcement in announcements)
            {
                if (announcement.Photos != null)
                {
                    // Order the in-memory collection by SortOrder so consumers receive consistent order
                    announcement.Photos = announcement.Photos
                        .OrderBy(p => p.SortOrder)
                        .ThenBy(p => p.UploadedAt)
                        .ToList();

                    foreach (var photo in announcement.Photos)
                    {
                        // Extract filename from the blob URL and convert to streaming URL
                        var fileName = ExtractFileNameFromUrl(photo.Url);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            // URL encode the filename to handle spaces and special characters
                            var encodedFileName = Uri.EscapeDataString(fileName);
                            // Add cache-busting parameter to force browser refresh
                            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            photo.Url = $"/api/photo/stream/{encodedFileName}?v={timestamp}";
                        }
                        // If fileName is empty, keep the original URL (for external URLs like placeholders)
                    }
                }
            }

            return announcements;
        }

        /// <summary>
        /// Extracts the filename from a blob storage URL.
        /// </summary>
        private string ExtractFileNameFromUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return string.Empty;
                
                // Handle already converted URLs - don't re-process them
                if (url.StartsWith("/api/photo/stream/"))
                {
                    // Return empty string to skip re-processing already converted URLs
                    return string.Empty;
                }
                
                // Don't convert external URLs (like placeholder URLs for testing)
                if (url.StartsWith("http://") || url.StartsWith("https://"))
                {
                    var uri = new Uri(url);
                    // Only convert blob storage URLs, leave other external URLs as-is
                    if (uri.Host.Contains("blob.core.windows.net") || uri.Host.Contains("eskolarblob"))
                    {
                        return Path.GetFileName(uri.LocalPath);
                    }
                    else
                    {
                        // Return empty string to skip conversion for external URLs
                        return string.Empty;
                    }
                }
                
                var defaultUri = new Uri(url);
                return Path.GetFileName(defaultUri.LocalPath);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Fixes double-encoded URLs in the database that were created before the fix
        /// </summary>
        public async Task FixDoubleEncodedUrlsAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var photos = await context.Photos
                    .Where(p => p.Url.Contains("%2520") || p.Url.Contains("%2525"))
                    .ToListAsync();

                foreach (var photo in photos)
                {
                    // Decode the URL to get the original filename
                    var originalUrl = photo.Url;
                    
                    // If it starts with /api/photo/stream/, extract just the filename part
                    if (originalUrl.StartsWith("/api/photo/stream/"))
                    {
                        var filenamePart = originalUrl.Substring("/api/photo/stream/".Length);
                        
                        // Remove any query parameters (like ?v=timestamp)
                        var queryIndex = filenamePart.IndexOf('?');
                        if (queryIndex > 0)
                        {
                            filenamePart = filenamePart.Substring(0, queryIndex);
                        }
                        
                        // Double decode to fix the double encoding
                        var decodedFilename = Uri.UnescapeDataString(Uri.UnescapeDataString(filenamePart));
                        
                        // Re-encode properly
                        var properlyEncodedFilename = Uri.EscapeDataString(decodedFilename);
                        
                        // Add cache-busting parameter
                        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        photo.Url = $"/api/photo/stream/{properlyEncodedFilename}?v={timestamp}";
                        
                        Console.WriteLine($"Fixed URL: {originalUrl} -> {photo.Url}");
                    }
                }

                await context.SaveChangesAsync();
                Console.WriteLine($"Fixed {photos.Count} double-encoded URLs");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fixing double-encoded URLs: {ex.Message}");
            }
        }
    }
}
