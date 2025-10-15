// Enhanced Scholarship Management Service - Integrated with EskoBot Intelligence
// Bridge service connecting EnhancedScrapedScholarship with database persistence
// Provides comprehensive workflow management for AI-scraped scholarships

using Microsoft.EntityFrameworkCore;
using c2_eskolar.Data;
using c2_eskolar.Models;
using c2_eskolar.Components.Pages.Admin;
using c2_eskolar.Services.WebScraping;
using ScrapedScholarshipEntity = c2_eskolar.Models.ScrapedScholarship;
using EnhancedScrapedScholarshipDto = c2_eskolar.Services.WebScraping.ScrapedScholarship;

namespace c2_eskolar.Services
{
    /// <summary>
    /// Enhanced Service for managing AI-scraped scholarships with approval workflow
    /// Integrates with EskoBot Intelligence for comprehensive scholarship curation
    /// Bridge service connecting EnhancedScrapedScholarship with database persistence
    /// </summary>
    public interface IScrapedScholarshipService
    {
        // Core CRUD Operations
        Task<IEnumerable<c2_eskolar.Models.ScrapedScholarship>> GetAllAsync();
        Task<c2_eskolar.Models.ScrapedScholarship?> GetByIdAsync(string id);
        Task<IEnumerable<c2_eskolar.Models.ScrapedScholarship>> GetByStatusAsync(ScrapingStatus status);
        Task<IEnumerable<c2_eskolar.Models.ScrapedScholarship>> GetRecentAsync(int days = 7);
        Task<ScrapedScholarshipStatistics> GetStatisticsAsync();
        Task<c2_eskolar.Models.ScrapedScholarship> CreateAsync(c2_eskolar.Models.ScrapedScholarship scholarship);
        Task<c2_eskolar.Models.ScrapedScholarship> UpdateAsync(c2_eskolar.Models.ScrapedScholarship scholarship);
        Task<bool> DeleteAsync(string id);
        
        // Workflow Management
        Task<bool> ApproveAsync(string id, string approvedBy, string? notes = null);
        Task<bool> RejectAsync(string id, string rejectedBy, string? notes = null);
        Task<BulkOperationResult> BulkOperationAsync(BulkOperationRequest request);
        Task<bool> PublishToMainSystemAsync(string id);
        Task<IEnumerable<c2_eskolar.Models.ScrapedScholarship>> SearchAsync(ScholarshipSearchCriteria criteria);
        
        // Enhanced Features - Bridge Methods
        Task<c2_eskolar.Models.ScrapedScholarship> CreateFromEnhancedAsync(EnhancedScrapedScholarship enhanced, string? createdBy = null);
        Task<List<c2_eskolar.Models.ScrapedScholarship>> CreateBatchFromEnhancedAsync(List<EnhancedScrapedScholarship> enhanced, string? createdBy = null);
        c2_eskolar.Models.ScrapedScholarship ConvertFromEnhanced(EnhancedScrapedScholarship enhanced);
        EnhancedScrapedScholarship ConvertToEnhanced(c2_eskolar.Models.ScrapedScholarship record);
        
        // Image and Media Management
        Task<bool> AddImageAsync(string scholarshipId, string imageUrl, string? imageType = null);
        Task<bool> UpdateImageAsync(string scholarshipId, string imageUrl);
        Task<List<string>> GetImagesAsync(string scholarshipId);
        Task<bool> DeleteImageAsync(string scholarshipId, string imageUrl);
        
        // Advanced Analytics
        Task<DashboardMetrics> GetDashboardMetricsAsync();
        Task<List<SourcePerformance>> GetSourcePerformanceAsync();
        Task<ConfidenceDistribution> GetConfidenceDistributionAsync();
        Task<List<DuplicateMatch>> DetectDuplicatesAsync();
        
        // Smart Categorization
        Task<string> AutoCategorizeAsync(string scholarshipId);
        Task<List<string>> GetCategoriesAsync();
        Task<bool> AssignCategoryAsync(string scholarshipId, string category);
    }

    public class ScrapedScholarshipService : IScrapedScholarshipService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<ScrapedScholarshipService> _logger;

        public ScrapedScholarshipService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<ScrapedScholarshipService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<ScrapedScholarshipEntity>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var scholarships = await context.ScrapedScholarships
                    .Include(s => s.ProcessingLogs)
                    .OrderByDescending(s => s.ScrapedAt)
                    .ToListAsync();

                return scholarships;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all scraped scholarships");
                return new List<ScrapedScholarshipEntity>();
            }
        }

        public async Task<ScrapedScholarshipEntity?> GetByIdAsync(string id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var scholarship = await context.ScrapedScholarships
                    .Include(s => s.ProcessingLogs)
                    .FirstOrDefaultAsync(s => s.Id == id);

                return scholarship;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scraped scholarship {Id}", id);
                return null;
            }
        }

        public async Task<IEnumerable<ScrapedScholarshipEntity>> GetByStatusAsync(ScrapingStatus status)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var scholarships = await context.ScrapedScholarships
                    .Include(s => s.ProcessingLogs)
                    .Where(s => s.Status == status)
                    .OrderByDescending(s => s.ScrapedAt)
                    .ToListAsync();

                return scholarships;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scholarships by status {Status}", status);
                return new List<ScrapedScholarshipEntity>();
            }
        }

        public async Task<IEnumerable<ScrapedScholarshipEntity>> GetRecentAsync(int days = 7)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var cutoffDate = DateTime.Now.AddDays(-days);
                var scholarships = await context.ScrapedScholarships
                    .Include(s => s.ProcessingLogs)
                    .Where(s => s.ScrapedAt >= cutoffDate)
                    .OrderByDescending(s => s.ScrapedAt)
                    .ToListAsync();

                return scholarships;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent scholarships");
                return new List<ScrapedScholarshipEntity>();
            }
        }

        public async Task<ScrapedScholarshipStatistics> GetStatisticsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var today = DateTime.Today;
                var thisWeek = DateTime.Now.AddDays(-7);

                var stats = new ScrapedScholarshipStatistics
                {
                    TotalScraped = await context.ScrapedScholarships.CountAsync(),
                    TodayScraped = await context.ScrapedScholarships.CountAsync(s => s.ScrapedAt.Date == today),
                    ThisWeekScraped = await context.ScrapedScholarships.CountAsync(s => s.ScrapedAt >= thisWeek),
                    PendingReview = await context.ScrapedScholarships.CountAsync(s => 
                        s.Status == ScrapingStatus.Scraped || s.Status == ScrapingStatus.UnderReview),
                    Approved = await context.ScrapedScholarships.CountAsync(s => s.Status == ScrapingStatus.Approved),
                    Published = await context.ScrapedScholarships.CountAsync(s => s.Status == ScrapingStatus.Published),
                    Rejected = await context.ScrapedScholarships.CountAsync(s => s.Status == ScrapingStatus.Rejected),
                    Enhanced = await context.ScrapedScholarships.CountAsync(s => s.IsEnhanced),
                    AverageConfidence = await context.ScrapedScholarships.AnyAsync() ? 
                        await context.ScrapedScholarships.AverageAsync(s => s.ParsingConfidence) : 0.0,
                    TopSources = await GetTopSourcesAsync(context)
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating statistics");
                return new ScrapedScholarshipStatistics();
            }
        }

        public async Task<ScrapedScholarshipEntity> CreateAsync(ScrapedScholarshipEntity scholarship)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                scholarship.CreatedAt = DateTime.Now;
                scholarship.UpdatedAt = DateTime.Now;

                context.ScrapedScholarships.Add(scholarship);
                await context.SaveChangesAsync();

                // Log the creation
                await LogProcessAsync(scholarship.Id, "Created", "Scholarship record created", "System");

                return scholarship;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating scraped scholarship");
                throw;
            }
        }

        public async Task<ScrapedScholarshipEntity> UpdateAsync(ScrapedScholarshipEntity scholarship)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.ScrapedScholarships.FindAsync(scholarship.Id);
                if (entity == null)
                    throw new ArgumentException($"Scholarship with ID {scholarship.Id} not found");

                // Update fields
                entity.Title = scholarship.Title;
                entity.Description = scholarship.Description;
                entity.Benefits = scholarship.Benefits;
                entity.MonetaryValue = scholarship.MonetaryValue;
                entity.ApplicationDeadline = scholarship.ApplicationDeadline;
                entity.Requirements = scholarship.Requirements;
                entity.RequiredCourse = scholarship.RequiredCourse;
                entity.RequiredUniversity = scholarship.RequiredUniversity;
                entity.RequiredYearLevel = scholarship.RequiredYearLevel;
                entity.SlotsAvailable = scholarship.SlotsAvailable;
                entity.MinimumGPA = scholarship.MinimumGPA;
                entity.ExternalApplicationUrl = scholarship.ExternalApplicationUrl;
                entity.SourceUrl = scholarship.SourceUrl;
                entity.Status = scholarship.Status;
                entity.ParsingNotes = scholarship.ParsingNotes;
                entity.ReviewNotes = scholarship.ReviewNotes;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = scholarship.UpdatedBy ?? "System";

                await context.SaveChangesAsync();

                // Log the update
                await LogProcessAsync(entity.Id, "Updated", "Scholarship record updated", entity.UpdatedBy);

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scraped scholarship {Id}", scholarship.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.ScrapedScholarships.FindAsync(id);
                if (entity == null)
                    return false;

                context.ScrapedScholarships.Remove(entity);
                await context.SaveChangesAsync();

                _logger.LogInformation("Deleted scraped scholarship {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting scraped scholarship {Id}", id);
                return false;
            }
        }

        public async Task<bool> ApproveAsync(string id, string approvedBy, string? notes = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.ScrapedScholarships.FindAsync(id);
                if (entity == null)
                    return false;

                entity.Status = ScrapingStatus.Approved;
                entity.ApprovedBy = approvedBy;
                entity.ApprovedAt = DateTime.Now;
                entity.ReviewNotes = notes;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = approvedBy;

                await context.SaveChangesAsync();

                // Log the approval
                await LogProcessAsync(id, "Approved", $"Approved by {approvedBy}. Notes: {notes}", approvedBy);

                _logger.LogInformation("Approved scraped scholarship {Id} by {ApprovedBy}", id, approvedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving scraped scholarship {Id}", id);
                return false;
            }
        }

        public async Task<bool> RejectAsync(string id, string rejectedBy, string? notes = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.ScrapedScholarships.FindAsync(id);
                if (entity == null)
                    return false;

                entity.Status = ScrapingStatus.Rejected;
                entity.ReviewedBy = rejectedBy;
                entity.ReviewedAt = DateTime.Now;
                entity.ReviewNotes = notes;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = rejectedBy;

                await context.SaveChangesAsync();

                // Log the rejection
                await LogProcessAsync(id, "Rejected", $"Rejected by {rejectedBy}. Notes: {notes}", rejectedBy);

                _logger.LogInformation("Rejected scraped scholarship {Id} by {RejectedBy}", id, rejectedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting scraped scholarship {Id}", id);
                return false;
            }
        }

        public async Task<BulkOperationResult> BulkOperationAsync(BulkOperationRequest request)
        {
            var result = new BulkOperationResult
            {
                OperationType = request.OperationType,
                TotalItems = request.ScholarshipIds.Count,
                StartedAt = DateTime.Now
            };

            try
            {
                foreach (var id in request.ScholarshipIds)
                {
                    try
                    {
                        var success = request.OperationType.ToLowerInvariant() switch
                        {
                            "approve" => await ApproveAsync(id, request.ExecutedBy, request.Notes),
                            "reject" => await RejectAsync(id, request.ExecutedBy, request.Notes),
                            "delete" => await DeleteAsync(id),
                            _ => false
                        };

                        if (success)
                        {
                            result.SuccessfulItems++;
                            result.ProcessedIds.Add(id);
                        }
                        else
                        {
                            result.FailedItems++;
                            result.FailedIds.Add(id);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedItems++;
                        result.FailedIds.Add(id);
                        result.Errors.Add($"ID {id}: {ex.Message}");
                    }
                }

                result.CompletedAt = DateTime.Now;
                _logger.LogInformation("Completed bulk operation {Operation}: {Success}/{Total} successful", 
                    request.OperationType, result.SuccessfulItems, result.TotalItems);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk operation {Operation}", request.OperationType);
                result.Errors.Add($"Bulk operation failed: {ex.Message}");
                return result;
            }
        }

        public async Task<bool> PublishToMainSystemAsync(string id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var scrapedScholarship = await context.ScrapedScholarships.FindAsync(id);
                if (scrapedScholarship == null || scrapedScholarship.Status != ScrapingStatus.Approved)
                    return false;

                // Create main scholarship record with EskoBot Intelligence attribution
                var scholarship = new Scholarship
                {
                    Title = scrapedScholarship.Title,
                    Description = scrapedScholarship.Description ?? "",
                    Benefits = scrapedScholarship.Benefits ?? "",
                    MonetaryValue = scrapedScholarship.MonetaryValue,
                    ApplicationDeadline = scrapedScholarship.ApplicationDeadline ?? DateTime.Now.AddYears(1),
                    Requirements = scrapedScholarship.Requirements ?? "",
                    SlotsAvailable = scrapedScholarship.SlotsAvailable,
                    MinimumGPA = scrapedScholarship.MinimumGPA,
                    ExternalApplicationUrl = scrapedScholarship.ExternalApplicationUrl,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                context.Scholarships.Add(scholarship);
                
                // Update scraped scholarship status
                scrapedScholarship.Status = ScrapingStatus.Published;
                scrapedScholarship.PublishedScholarshipId = scholarship.ScholarshipId;
                scrapedScholarship.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();

                // Log the publication
                await LogProcessAsync(id, "Published", $"Published to main system as {scholarship.ScholarshipId}", "System");

                _logger.LogInformation("Published scraped scholarship {Id} to main system as {ScholarshipId}", 
                    id, scholarship.ScholarshipId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing scraped scholarship {Id}", id);
                return false;
            }
        }

        public async Task<IEnumerable<ScrapedScholarshipEntity>> SearchAsync(ScholarshipSearchCriteria criteria)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ScrapedScholarships.Include(s => s.ProcessingLogs).AsQueryable();

                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    var searchTerm = criteria.SearchText.ToLowerInvariant();
                    query = query.Where(s => 
                        s.Title.ToLower().Contains(searchTerm) ||
                        (s.Description != null && s.Description.ToLower().Contains(searchTerm)) ||
                        (s.Requirements != null && s.Requirements.ToLower().Contains(searchTerm)));
                }

                if (criteria.Status.HasValue)
                {
                    query = query.Where(s => s.Status == criteria.Status.Value);
                }

                if (criteria.MinConfidence.HasValue)
                {
                    query = query.Where(s => s.ParsingConfidence >= criteria.MinConfidence.Value);
                }

                if (criteria.IsEnhanced.HasValue)
                {
                    query = query.Where(s => s.IsEnhanced == criteria.IsEnhanced.Value);
                }

                if (criteria.ScrapedAfter.HasValue)
                {
                    query = query.Where(s => s.ScrapedAt >= criteria.ScrapedAfter.Value);
                }

                if (criteria.ScrapedBefore.HasValue)
                {
                    query = query.Where(s => s.ScrapedAt <= criteria.ScrapedBefore.Value);
                }

                if (!string.IsNullOrWhiteSpace(criteria.SourceUrl))
                {
                    query = query.Where(s => s.SourceUrl.Contains(criteria.SourceUrl));
                }

                var results = await query
                    .OrderByDescending(s => s.ScrapedAt)
                    .Take(criteria.MaxResults ?? 1000)
                    .ToListAsync();

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching scraped scholarships");
                return new List<ScrapedScholarshipEntity>();
            }
        }

        // Enhanced Features - Bridge Methods for EnhancedScrapedScholarship
        public async Task<ScrapedScholarshipEntity> CreateFromEnhancedAsync(EnhancedScrapedScholarship enhanced, string? createdBy = null)
        {
            var entity = ConvertFromEnhanced(enhanced);
            entity.CreatedBy = createdBy ?? "EskoBot Intelligence";
            entity.AuthorAttribution = "EskoBot Intelligence";
            return await CreateAsync(entity);
        }

        public async Task<List<ScrapedScholarshipEntity>> CreateBatchFromEnhancedAsync(List<EnhancedScrapedScholarship> enhanced, string? createdBy = null)
        {
            var results = new List<ScrapedScholarshipEntity>();
            foreach (var scholarship in enhanced)
            {
                try
                {
                    var result = await CreateFromEnhancedAsync(scholarship, createdBy);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating scholarship from enhanced: {Title}", scholarship.Title);
                }
            }
            return results;
        }

        public ScrapedScholarshipEntity ConvertFromEnhanced(EnhancedScrapedScholarship enhanced)
        {
            return new ScrapedScholarshipEntity
            {
                Id = Guid.NewGuid().ToString(),
                Title = enhanced.Title ?? "Untitled Scholarship",
                Description = enhanced.Description,
                Benefits = enhanced.Benefits,
                MonetaryValue = enhanced.MonetaryValue,
                ApplicationDeadline = enhanced.ApplicationDeadline,
                Requirements = enhanced.Requirements,
                SlotsAvailable = enhanced.SlotsAvailable,
                MinimumGPA = enhanced.MinimumGPA,
                RequiredCourse = enhanced.RequiredCourse,
                RequiredYearLevel = enhanced.RequiredYearLevel,
                RequiredUniversity = enhanced.RequiredUniversity,
                ExternalApplicationUrl = enhanced.ExternalApplicationUrl,
                SourceUrl = enhanced.SourceUrl ?? "",
                ScrapedAt = enhanced.ScrapedAt,
                ParsingConfidence = enhanced.ParsingConfidence,
                Status = DetermineStatusFromConfidence(enhanced.ParsingConfidence),
                IsEnhanced = true,
                RawText = enhanced.RawText ?? "",
                ParsingNotes = string.Join("|", enhanced.ParsingNotes ?? new List<string>()),
                AuthorAttribution = "EskoBot Intelligence",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        public EnhancedScrapedScholarship ConvertToEnhanced(ScrapedScholarshipEntity entity)
        {
            return new EnhancedScrapedScholarship
            {
                Title = entity.Title,
                Description = entity.Description,
                Benefits = entity.Benefits,
                MonetaryValue = entity.MonetaryValue,
                ApplicationDeadline = entity.ApplicationDeadline,
                Requirements = entity.Requirements,
                SlotsAvailable = entity.SlotsAvailable,
                MinimumGPA = entity.MinimumGPA,
                RequiredCourse = entity.RequiredCourse,
                RequiredYearLevel = entity.RequiredYearLevel,
                RequiredUniversity = entity.RequiredUniversity,
                ExternalApplicationUrl = entity.ExternalApplicationUrl,
                SourceUrl = entity.SourceUrl,
                RawText = entity.RawText,
                ScrapedAt = entity.ScrapedAt,
                ParsingConfidence = entity.ParsingConfidence,
                ParsingNotes = entity.ParsingNotes?.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                IsActive = true,
                IsInternal = false
            };
        }

        // Image and Media Management
        public async Task<bool> AddImageAsync(string scholarshipId, string imageUrl, string? imageType = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var scholarship = await context.ScrapedScholarships.FindAsync(scholarshipId);
                if (scholarship == null) return false;

                // Store image URL in ParsingNotes for now (could be enhanced with dedicated table)
                var imageNote = $"IMAGE:{imageType ?? "default"}:{imageUrl}";
                var existingNotes = scholarship.ParsingNotes?.Split('|').ToList() ?? new List<string>();
                existingNotes.Add(imageNote);
                scholarship.ParsingNotes = string.Join("|", existingNotes);
                scholarship.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                await LogProcessAsync(scholarshipId, "Image Added", $"Added image: {imageUrl}", "System");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding image to scholarship {Id}", scholarshipId);
                return false;
            }
        }

        public async Task<bool> UpdateImageAsync(string scholarshipId, string imageUrl)
        {
            return await AddImageAsync(scholarshipId, imageUrl, "updated");
        }

        public async Task<List<string>> GetImagesAsync(string scholarshipId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var scholarship = await context.ScrapedScholarships.FindAsync(scholarshipId);
                if (scholarship?.ParsingNotes == null) return new List<string>();

                return scholarship.ParsingNotes
                    .Split('|')
                    .Where(note => note.StartsWith("IMAGE:"))
                    .Select(note => note.Split(':').LastOrDefault())
                    .Where(url => !string.IsNullOrEmpty(url))
                    .ToList() ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<bool> DeleteImageAsync(string scholarshipId, string imageUrl)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var scholarship = await context.ScrapedScholarships.FindAsync(scholarshipId);
                if (scholarship?.ParsingNotes == null) return false;

                var notes = scholarship.ParsingNotes.Split('|').ToList();
                notes.RemoveAll(note => note.Contains($":{imageUrl}"));
                scholarship.ParsingNotes = string.Join("|", notes);
                scholarship.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Advanced Analytics
        public async Task<DashboardMetrics> GetDashboardMetricsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var now = DateTime.Now;

                return new DashboardMetrics
                {
                    TotalScholarships = await context.ScrapedScholarships.CountAsync(),
                    TodayScraped = await context.ScrapedScholarships.CountAsync(s => s.ScrapedAt.Date == now.Date),
                    WeeklyGrowth = await CalculateWeeklyGrowthAsync(context),
                    AverageConfidence = await context.ScrapedScholarships.AnyAsync() ? 
                        await context.ScrapedScholarships.AverageAsync(s => s.ParsingConfidence) : 0.0,
                    TopPerformingSources = await GetTopSourcesAsync(context),
                    RecentActivity = await GetRecentActivityAsync(context)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating dashboard metrics");
                return new DashboardMetrics();
            }
        }

        public async Task<List<SourcePerformance>> GetSourcePerformanceAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ScrapedScholarships
                    .GroupBy(s => s.SourceUrl)
                    .Select(g => new SourcePerformance
                    {
                        SourceUrl = g.Key,
                        TotalScraped = g.Count(),
                        AverageConfidence = g.Average(s => s.ParsingConfidence),
                        ApprovedCount = g.Count(s => s.Status == ScrapingStatus.Approved),
                        LastScrapedAt = g.Max(s => s.ScrapedAt)
                    })
                    .OrderByDescending(sp => sp.TotalScraped)
                    .ToListAsync();
            }
            catch
            {
                return new List<SourcePerformance>();
            }
        }

        public async Task<ConfidenceDistribution> GetConfidenceDistributionAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var scholarships = await context.ScrapedScholarships.ToListAsync();

                return new ConfidenceDistribution
                {
                    High = scholarships.Count(s => s.ParsingConfidence >= 0.8),
                    Medium = scholarships.Count(s => s.ParsingConfidence >= 0.6 && s.ParsingConfidence < 0.8),
                    Low = scholarships.Count(s => s.ParsingConfidence < 0.6),
                    Total = scholarships.Count
                };
            }
            catch
            {
                return new ConfidenceDistribution();
            }
        }

        public async Task<List<DuplicateMatch>> DetectDuplicatesAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var scholarships = await context.ScrapedScholarships.ToListAsync();
                var duplicates = new List<DuplicateMatch>();

                for (int i = 0; i < scholarships.Count; i++)
                {
                    for (int j = i + 1; j < scholarships.Count; j++)
                    {
                        var similarity = CalculateStringSimilarity(scholarships[i].Title, scholarships[j].Title);
                        if (similarity > 0.8)
                        {
                            duplicates.Add(new DuplicateMatch
                            {
                                Scholarship1Id = scholarships[i].Id,
                                Scholarship2Id = scholarships[j].Id,
                                SimilarityScore = similarity,
                                MatchType = "Title"
                            });
                        }
                    }
                }

                return duplicates.Take(50).ToList(); // Limit results
            }
            catch
            {
                return new List<DuplicateMatch>();
            }
        }

        // Smart Categorization
        public async Task<string> AutoCategorizeAsync(string scholarshipId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var scholarship = await context.ScrapedScholarships.FindAsync(scholarshipId);
                if (scholarship == null) return "General";

                // Simple categorization logic based on keywords
                var text = $"{scholarship.Title} {scholarship.Description} {scholarship.Requirements}".ToLowerInvariant();
                
                if (text.Contains("engineering") || text.Contains("technology") || text.Contains("stem"))
                    return "STEM";
                if (text.Contains("medicine") || text.Contains("health") || text.Contains("nursing"))
                    return "Healthcare";
                if (text.Contains("business") || text.Contains("management") || text.Contains("mba"))
                    return "Business";
                if (text.Contains("arts") || text.Contains("humanities") || text.Contains("literature"))
                    return "Arts & Humanities";
                if (text.Contains("education") || text.Contains("teaching"))
                    return "Education";

                return "General";
            }
            catch
            {
                return "General";
            }
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await Task.FromResult(new List<string>
            {
                "STEM", "Healthcare", "Business", "Arts & Humanities", 
                "Education", "Social Sciences", "Law", "General"
            });
        }

        public async Task<bool> AssignCategoryAsync(string scholarshipId, string category)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var scholarship = await context.ScrapedScholarships.FindAsync(scholarshipId);
                if (scholarship == null) return false;

                var categoryNote = $"CATEGORY:{category}";
                var existingNotes = scholarship.ParsingNotes?.Split('|').ToList() ?? new List<string>();
                existingNotes.RemoveAll(note => note.StartsWith("CATEGORY:"));
                existingNotes.Add(categoryNote);
                scholarship.ParsingNotes = string.Join("|", existingNotes);
                scholarship.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Private helper methods
        private async Task<List<string>> GetTopSourcesAsync(ApplicationDbContext context)
        {
            try
            {
                return await context.ScrapedScholarships
                    .GroupBy(s => s.SourceUrl)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => g.Key)
                    .ToListAsync();
            }
            catch
            {
                return new List<string>();
            }
        }

        private async Task<double> CalculateWeeklyGrowthAsync(ApplicationDbContext context)
        {
            try
            {
                var thisWeek = DateTime.Now.AddDays(-7);
                var lastWeek = DateTime.Now.AddDays(-14);
                
                var thisWeekCount = await context.ScrapedScholarships.CountAsync(s => s.ScrapedAt >= thisWeek);
                var lastWeekCount = await context.ScrapedScholarships.CountAsync(s => s.ScrapedAt >= lastWeek && s.ScrapedAt < thisWeek);
                
                if (lastWeekCount == 0) return thisWeekCount > 0 ? 100.0 : 0.0;
                return ((double)(thisWeekCount - lastWeekCount) / lastWeekCount) * 100.0;
            }
            catch
            {
                return 0.0;
            }
        }

        private async Task<List<string>> GetRecentActivityAsync(ApplicationDbContext context)
        {
            try
            {
                var recent = await context.ScrapingProcessLogs
                    .OrderByDescending(l => l.ProcessedAt)
                    .Take(5)
                    .Select(l => $"{l.ProcessType}: {l.ProcessDetails}")
                    .ToListAsync();
                return recent;
            }
            catch
            {
                return new List<string>();
            }
        }

        private ScrapingStatus DetermineStatusFromConfidence(double confidence)
        {
            return confidence switch
            {
                >= 0.9 => ScrapingStatus.Approved,
                >= 0.7 => ScrapingStatus.UnderReview, 
                >= 0.5 => ScrapingStatus.Scraped,
                _ => ScrapingStatus.Rejected
            };
        }

        private double CalculateStringSimilarity(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2)) return 0.0;
            
            var longer = str1.Length > str2.Length ? str1 : str2;
            var shorter = str1.Length > str2.Length ? str2 : str1;
            
            if (longer.Length == 0) return 1.0;
            
            var editDistance = ComputeLevenshteinDistance(longer, shorter);
            return (longer.Length - editDistance) / (double)longer.Length;
        }

        private int ComputeLevenshteinDistance(string s, string t)
        {
            var d = new int[s.Length + 1, t.Length + 1];
            
            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;
            
            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    var cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            
            return d[s.Length, t.Length];
        }

        private async Task LogProcessAsync(string scholarshipId, string processType, string details, string processedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var log = new ScrapingProcessLog
                {
                    ScrapedScholarshipId = scholarshipId,
                    ProcessType = processType,
                    ProcessDetails = details,
                    ProcessedBy = processedBy,
                    ProcessedAt = DateTime.Now
                };

                context.ScrapingProcessLogs.Add(log);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging process for scholarship {Id}", scholarshipId);
            }
        }

    }

    // Supporting classes for enhanced functionality
    public class ScrapedScholarshipStatistics
    {
        public int TotalScraped { get; set; }
        public int TodayScraped { get; set; }
        public int ThisWeekScraped { get; set; }
        public int PendingReview { get; set; }
        public int Approved { get; set; }
        public int Published { get; set; }
        public int Rejected { get; set; }
        public int Enhanced { get; set; }
        public double AverageConfidence { get; set; }
        public List<string> TopSources { get; set; } = new();
    }

    public class BulkOperationRequest
    {
        public string OperationType { get; set; } = "";
        public List<string> ScholarshipIds { get; set; } = new();
        public string ExecutedBy { get; set; } = "";
        public string? Notes { get; set; }
    }

    public class BulkOperationResult
    {
        public string OperationType { get; set; } = "";
        public int TotalItems { get; set; }
        public int SuccessfulItems { get; set; }
        public int FailedItems { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<string> ProcessedIds { get; set; } = new();
        public List<string> FailedIds { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class ScholarshipSearchCriteria
    {
        public string? SearchText { get; set; }
        public ScrapingStatus? Status { get; set; }
        public double? MinConfidence { get; set; }
        public bool? IsEnhanced { get; set; }
        public DateTime? ScrapedAfter { get; set; }
        public DateTime? ScrapedBefore { get; set; }
        public string? SourceUrl { get; set; }
        public int? MaxResults { get; set; } = 1000;
    }

    // Enhanced Analytics Classes
    public class DashboardMetrics
    {
        public int TotalScholarships { get; set; }
        public int TodayScraped { get; set; }
        public double WeeklyGrowth { get; set; }
        public double AverageConfidence { get; set; }
        public List<string> TopPerformingSources { get; set; } = new();
        public List<string> RecentActivity { get; set; } = new();
    }

    public class SourcePerformance
    {
        public string SourceUrl { get; set; } = "";
        public int TotalScraped { get; set; }
        public double AverageConfidence { get; set; }
        public int ApprovedCount { get; set; }
        public DateTime LastScrapedAt { get; set; }
    }

    public class ConfidenceDistribution
    {
        public int High { get; set; } // >= 80%
        public int Medium { get; set; } // 60-79%
        public int Low { get; set; } // < 60%
        public int Total { get; set; }
    }

    public class DuplicateMatch
    {
        public string Scholarship1Id { get; set; } = "";
        public string Scholarship2Id { get; set; } = "";
        public double SimilarityScore { get; set; }
        public string MatchType { get; set; } = "";
    }
}