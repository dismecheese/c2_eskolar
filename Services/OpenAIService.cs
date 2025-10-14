using Azure.AI.OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using c2_eskolar.Models;
using c2_eskolar.Services.AI;

namespace c2_eskolar.Services
{
        public class OpenAIService
        {
        private readonly AzureOpenAIClient _client;
        private readonly string _deploymentName;
        private readonly ProfileSummaryService _profileSummaryService;
        private readonly ScholarshipRecommendationService _scholarshipRecommendationService;
        private readonly AnnouncementRecommendationService _announcementRecommendationService;
        private readonly ContextGenerationService _contextGenerationService;
        private readonly DisplayContextAwarenessService _displayContextAwarenessService;
        private readonly AITokenTrackingService _tokenTrackingService;

        public OpenAIService(IConfiguration config, ProfileSummaryService profileSummaryService, ScholarshipRecommendationService scholarshipRecommendationService, AnnouncementRecommendationService announcementRecommendationService, ContextGenerationService contextGenerationService, DisplayContextAwarenessService displayContextAwarenessService, AITokenTrackingService tokenTrackingService)
        {
            var apiKey = config["AzureOpenAI:ApiKey"] ?? throw new ArgumentNullException("AzureOpenAI:ApiKey");
            var endpoint = config["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("AzureOpenAI:Endpoint");
            _deploymentName = config["AzureOpenAI:DeploymentName"] ?? throw new ArgumentNullException("AzureOpenAI:DeploymentName");
            _client = new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
            _profileSummaryService = profileSummaryService;
            _scholarshipRecommendationService = scholarshipRecommendationService;
            _announcementRecommendationService = announcementRecommendationService;
            _contextGenerationService = contextGenerationService;
            _displayContextAwarenessService = displayContextAwarenessService;
            _tokenTrackingService = tokenTrackingService;
        }

        // New method: Forecast applicant volume for next 7 days
        public async Task<string> GetApplicantVolumeForecastAsync(List<(DateTime Date, int Count)> applicantsPerDay)
        {
            // Format the data for the prompt
            var dataRows = applicantsPerDay.Select(x => $"{x.Date:yyyy-MM-dd}: {x.Count}");
            var dataString = string.Join("\n", dataRows);
            string prompt = $@"You are an expert data analyst. Here is the number of scholarship applicants per day for the past period:\n\n{dataString}\n\nBased on this data, forecast the expected number of applicants per day for the next 7 days. Return your forecast as a numbered list with dates and expected counts, and provide a brief explanation of the trend.";

            return await GetChatCompletionAsync(prompt);
        }

        public async Task<string> GetChatCompletionAsync(string userMessage)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var chatClient = _client.GetChatClient(_deploymentName);
                var messages = new[]
                {
                    new UserChatMessage(userMessage)
                };
                
                var response = await chatClient.CompleteChatAsync(messages);
                stopwatch.Stop();
                
                // Track token usage
                await _tokenTrackingService.TrackChatCompletionUsageAsync(
                    completion: response.Value,
                    operation: "BasicChatCompletion",
                    userId: null, // No user context in basic method
                    additionalDetails: $"Message length: {userMessage.Length}",
                    requestDuration: stopwatch.Elapsed
                );
                
                return response.Value.Content[0].Text.Trim();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Track failed request
                await _tokenTrackingService.TrackFailedRequestAsync(
                    operation: "BasicChatCompletion",
                    errorMessage: ex.Message,
                    userId: null,
                    requestDuration: stopwatch.Elapsed
                );
                
                throw; // Re-throw the exception
            }
        }

        // Enhanced method: includes user profile and current page context
        public async Task<string> GetChatCompletionWithProfileAndContextAsync(string userMessage, IdentityUser user, ChatContext? context = null, bool isFirstMessage = false)
        {
            var profileSummary = await _profileSummaryService.GetProfileSummaryAsync(user);
            var firstName = await _profileSummaryService.GetUserFirstNameAsync(user);
            var scholarshipRecommendations = await _scholarshipRecommendationService.GetScholarshipRecommendationsAsync(user);
            
            // Create a personalized greeting only for the first message
            string greeting = "";
            if (isFirstMessage)
            {
                greeting = !string.IsNullOrEmpty(firstName) 
                    ? $"Hello {firstName}! How can I help you today?"
                    : "Hello! How can I help you today?";
            }

            // Check if user is asking about scholarships, announcements, or similar queries
            var isScholarshipQuery = IsScholarshipRelatedQuery(userMessage);
            var isAnnouncementQuery = IsAnnouncementRelatedQuery(userMessage);
            
            // Get announcement recommendations if this is an announcement query
            List<AnnouncementRecommendation> announcementRecommendations = new();
            if (isAnnouncementQuery)
            {
                announcementRecommendations = await _announcementRecommendationService.GetAnnouncementRecommendationsAsync(user, userMessage);
            }

            // For greeting requests, return simple greeting without complex system prompts
            if (isFirstMessage && (userMessage.Contains("welcome", StringComparison.OrdinalIgnoreCase) || 
                                  userMessage.Contains("brief", StringComparison.OrdinalIgnoreCase) ||
                                  userMessage.Contains("hello", StringComparison.OrdinalIgnoreCase)))
            {
                return greeting;
            }

            // Generate context-aware system prompt
            string contextInfo = context != null ? _displayContextAwarenessService.GeneratePageContext(context) : "";
            
            string systemPrompt = profileSummary != null
                ? $"You are an AI assistant for eSkolar, a scholarship platform. You are helping a {profileSummary.Role.ToLower()}. " +
                  $"You have access to the user's profile information and should use it to answer their questions. " +
                  $"When they ask about their personal information (like 'what is my name?', 'what is my GPA?', 'what course am I taking?'), " +
                  $"answer using the specific data from their profile below. Be conversational and helpful.\n\n" +
                  
                  contextInfo +
                  
                  $"FORMATTING GUIDELINES (Apply to ALL content - scholarships, announcements, profiles, etc.):\n" +
                  $"• When listing items, use NUMBERED format: 1. Title, 2. Title, 3. Title\n" +
                  $"• DO NOT use bullet points (•) for main titles\n" +
                  $"• DO NOT put ** around main titles (but DO use ** for ALL property labels like **Author:**, **Match Score:**, **Posted:**)\n" +
                  $"• Use numbered lists (1. 2. 3.) for step-by-step instructions\n" +
                  $"• Add relevant emojis to make content easy to scan: 🎓📚💰📅⚠️📢✅❗👤🕒🏷️📌🏢📄\n" +
                  $"• Use ---- as separators between different items or sections\n" +
                  $"• Keep formatting clean and readable in a chat interface\n" +
                  $"• Use minimal line breaks between details to keep content compact\n" +
                  $"• Emphasize urgent deadlines, important announcements, and key information\n" +
                  $"• For ALL content types, use **Label:** format for properties (Author, Date, Category, etc.)\n\n" +
                  
                  $"When they ask about scholarships, recommendations, or what scholarships they're eligible for, " +
                  $"use the scholarship recommendations provided below and explain why each scholarship matches their profile. " +
                  $"Present scholarships in order of best match first.\n\n" +
                  $"When they ask about announcements, news, or updates, use the announcement recommendations provided below " +
                  $"and explain why each announcement is relevant to them. Present announcements by relevance.\n\n" +
                  $"Always be friendly, professional, and specific when referencing their profile data. Use emojis to make responses engaging and readable."
                
                : $"You are an AI assistant for eSkolar, a scholarship platform. " +
                  $"I don't have access to your profile information yet, but I'm here to help with general questions about scholarships and the platform. " +
                  $"Please use clear formatting with emojis and bullet points when presenting information.";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt)
            };

            if (profileSummary != null)
            {
                // Create a more natural profile context with better formatting instructions
                var profileContext = $"📋 USER PROFILE INFORMATION:\n{profileSummary.Summary}\n\n" +
                                   $"FORMATTING INSTRUCTIONS FOR PROFILE DATA:\n" +
                                   $"• When presenting personal information, use clear labels and emojis\n" +
                                   $"• Format academic info with 📚, contact info with 📧📱, dates with 📅\n" +
                                   $"• Use simple bullet points for multiple items\n" +
                                   $"• Highlight important details like GPA, verification status\n" +
                                   $"• Present information in a conversational but organized way\n";
                messages.Add(new SystemChatMessage(profileContext));

                // Add scholarship recommendations if this is a scholarship-related query or user is a student
                if (profileSummary.Role == "Student" && (isScholarshipQuery || isFirstMessage || scholarshipRecommendations.Any()))
                {
                    var scholarshipContext = _contextGenerationService.GenerateScholarshipContext(scholarshipRecommendations);
                    if (!string.IsNullOrEmpty(scholarshipContext))
                    {
                        messages.Add(new SystemChatMessage(scholarshipContext));
                    }
                }

                // Add announcement recommendations if this is an announcement-related query
                if (profileSummary.Role == "Student" && isAnnouncementQuery && announcementRecommendations.Any())
                {
                    var announcementContext = _contextGenerationService.GenerateAnnouncementContext(announcementRecommendations);
                    if (!string.IsNullOrEmpty(announcementContext))
                    {
                        messages.Add(new SystemChatMessage(announcementContext));
                    }
                }
            }
            
            messages.Add(new UserChatMessage(userMessage));

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var chatClient = _client.GetChatClient(_deploymentName);
                var response = await chatClient.CompleteChatAsync(messages);
                stopwatch.Stop();
                
                // Track token usage with enhanced context
                var operationDetails = new
                {
                    HasProfile = profileSummary != null,
                    UserRole = profileSummary?.Role ?? "Unknown",
                    IsScholarshipQuery = isScholarshipQuery,
                    IsAnnouncementQuery = isAnnouncementQuery,
                    IsFirstMessage = isFirstMessage,
                    MessageCount = messages.Count,
                    UserMessageLength = userMessage.Length,
                    ScholarshipRecommendations = scholarshipRecommendations.Count,
                    AnnouncementRecommendations = announcementRecommendations.Count
                };
                
                await _tokenTrackingService.TrackChatCompletionUsageAsync(
                    completion: response.Value,
                    operation: "EnhancedChatCompletion",
                    userId: user?.Id,
                    additionalDetails: System.Text.Json.JsonSerializer.Serialize(operationDetails),
                    requestDuration: stopwatch.Elapsed
                );
                
                string aiResponse = response.Value.Content[0].Text.Trim();
                
                // Combine greeting with AI response if this is the first message
                if (isFirstMessage && !string.IsNullOrEmpty(greeting))
                {
                    return $"{greeting}\n\n{aiResponse}";
                }
                
                return aiResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Track failed request
                await _tokenTrackingService.TrackFailedRequestAsync(
                    operation: "EnhancedChatCompletion",
                    errorMessage: ex.Message,
                    userId: user?.Id,
                    requestDuration: stopwatch.Elapsed
                );
                
                throw; // Re-throw the exception
            }
        }


        private bool IsScholarshipRelatedQuery(string userMessage)
        {
            var scholarshipKeywords = new[] 
            { 
                "scholarship", "scholarships", "recommend", "recommendation", "eligible", 
                "apply", "funding", "financial aid", "grant", "grants", "money", 
                "tuition", "study", "education", "degree", "program", "course",
                "what scholarships", "find scholarship", "search scholarship", 
                "scholarship for me", "scholarship opportunities", "available scholarships",
                "match", "suitable", "qualify", "qualified"
            };
            
            return scholarshipKeywords.Any(keyword => 
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsAnnouncementRelatedQuery(string userMessage)
        {
            var announcementKeywords = new[]
            {
                "announcement", "announcements", "news", "updates", "latest", "recent",
                "what's new", "notifications", "alerts", "bulletin", "notice",
                "institution announcement", "school announcement", "university announcement",
                "application announcement", "deadline announcement", "requirement announcement",
                "grant announcement", "scholarship announcement", "funding announcement",
                "pinned announcement", "important announcement", "urgent announcement",
                "benefactor announcement", "sponsor announcement", "organization announcement",
                "all announcements", "any announcements", "show announcements"
            };

            return announcementKeywords.Any(keyword =>
                userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private string GetGeneralFormattingInstructions()
        {
            return "💡 GENERAL FORMATTING GUIDELINES:\n" +
                   "• Use clear headings with emojis for different sections\n" +
                   "• Organize information in bullet points or numbered lists\n" +
                   "• Use visual separators (───) between different items or sections\n" +
                   "• Include relevant emojis to make content scannable and engaging\n" +
                   "• Use consistent indentation and spacing\n" +
                   "• Group related information together\n" +
                   "• Make urgent or important items stand out with warning emojis\n" +
                   "• Keep descriptions concise but informative\n" +
                   "• Always explain why information is relevant to the user\n";
        }

        public async Task<ExtractedInstitutionAuthLetterData?> ExtractInstitutionFieldsAsync(string rawText)
        {
            string prompt = $@"You are an expert at extracting structured data from institution authorization letters and official documents.

            Analyze the OCR text below from an institution authorization letter and extract the following information. Return ONLY a JSON object with these exact field names:

            InstitutionName: The official name of the educational institution (university, college, school)
            InstitutionType: Type of institution (University, College, School, Institute, etc.)
            Address: Physical address of the institution
            ContactNumber: Phone number or contact number of the institution
            Website: Institution website URL (if mentioned)
            Description: Brief description of the institution or its mission
            DeanName: Name of the Dean, Director, President, or head of institution mentioned
            DeanEmail: Email address of the Dean or institutional head
            InstitutionalEmailDomain: The email domain used by the institution (like @university.edu.ph)

            Look for:
            - Institution headers, letterheads, official names
            - Contact information, addresses, phone numbers
            - Official signatures from deans, directors, presidents
            - Email addresses and domains
            - Institutional descriptions or mission statements
            - Website URLs or social media

            Raw OCR Text:
            {rawText}

            Return only valid JSON:";

            var chatClient = _client.GetChatClient(_deploymentName);
            var messages = new OpenAI.Chat.ChatMessage[]
            {
                new OpenAI.Chat.SystemChatMessage("You are an expert at extracting structured data from institution authorization letters and official documents. Focus on finding official institution information, contact details, and administrative personnel. Always return only valid JSON for the requested fields."),
                new OpenAI.Chat.UserChatMessage(prompt)
            };
            
            try
            {
                var response = await chatClient.CompleteChatAsync(messages);
                string json = response.Value.Content[0].Text.Trim();
                
                // Clean up the JSON response (remove markdown code block markers if present)
                if (json.StartsWith("```json"))
                    json = json.Substring(7);
                if (json.StartsWith("```"))
                    json = json.Substring(3);
                if (json.EndsWith("```"))
                    json = json.Substring(0, json.Length - 3);
                json = json.Trim();

                var extracted = System.Text.Json.JsonSerializer.Deserialize<ExtractedInstitutionAuthLetterData>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return extracted;
            }
            catch (Exception ex)
            {
                // If parsing fails, return null
                Console.WriteLine($"Authorization letter extraction failed: {ex.Message}");
                return null;
            }
        }

        public async Task<ExtractedInstitutionIdData?> ExtractInstitutionIdFieldsAsync(string rawText)
        {
            string prompt = $@"Extract the following fields from the institution admin ID document text below. Return only the fields in JSON format:
AdminFirstName, AdminMiddleName, AdminLastName, AdminEmail, AdminContactNumber, AdminPosition, InstitutionalEmailDomain.

Text:
{rawText}

JSON:";

            var chatClient = _client.GetChatClient(_deploymentName);
            var messages = new OpenAI.Chat.ChatMessage[]
            {
                new OpenAI.Chat.SystemChatMessage("You are an expert at extracting structured data from institution admin ID documents. Always return only valid JSON for the requested fields."),
                new OpenAI.Chat.UserChatMessage(prompt)
            };
            var response = await chatClient.CompleteChatAsync(messages);
            string json = response.Value.Content[0].Text.Trim();

            try
            {
                var extracted = System.Text.Json.JsonSerializer.Deserialize<ExtractedInstitutionIdData>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return extracted;
            }
            catch
            {
                return null;
            }
        }

        public async Task<ExtractedBenefactorIdData?> ExtractBenefactorIdFieldsAsync(string rawText)
        {
            string prompt = $@"Extract the following fields from the Philippine ID document text below. Return only the fields in JSON format:
        AdminFirstName, AdminMiddleName, AdminLastName, AdminEmail, AdminContactNumber, AdminPosition, Sex, DateOfBirth (in YYYY-MM-DD format), Nationality.

        CRITICAL Philippine ID naming conventions - follow exactly:
        - Names are formatted as ""LAST NAME, GIVEN NAMES"" (e.g., ""ALONZO, ADRIAN FRANCIS TECSON"")
        - Everything BEFORE the comma = AdminLastName (e.g., ""ALONZO"")
        - Everything AFTER the comma = Given names that need to be split correctly
        - For given names like ""ADRIAN FRANCIS TECSON"":
        * The LAST word is typically the middle name (""TECSON"")
        * Everything BEFORE the last word is the first name (""ADRIAN FRANCIS"")
        - So ""ALONZO, ADRIAN FRANCIS TECSON"" should extract as:
        * AdminLastName: ""ALONZO""
        * AdminFirstName: ""ADRIAN FRANCIS""
        * AdminMiddleName: ""TECSON""
        - Look for patterns like ""Last Name, First Name, Middle Name"" or similar
        - Sex: Look for ""M""/""MALE"" or ""F""/""FEMALE""
        - Dates: Convert formats like ""2003/05/15"" to ""2003-05-15""
        - Nationality: ""PHL"" or ""FILIPINO"" for Philippine documents

        Text:
        {rawText}

        JSON:";

            var chatClient = _client.GetChatClient(_deploymentName);
            var messages = new OpenAI.Chat.ChatMessage[]
            {
                new OpenAI.Chat.SystemChatMessage("You are an expert at extracting structured data from Philippine identification documents. CRITICAL: For names like 'ALONZO, ADRIAN FRANCIS TECSON', the last word after the comma is the middle name (TECSON), and everything before that last word is the first name (ADRIAN FRANCIS). Never split compound first names. Always return only valid JSON."),
                new OpenAI.Chat.UserChatMessage(prompt)
            };
            var response = await chatClient.CompleteChatAsync(messages);
            string json = response.Value.Content[0].Text.Trim();

            try
            {
                // Clean up the JSON response (remove markdown code block markers if present)
                if (json.StartsWith("```json"))
                    json = json.Substring(7);
                if (json.StartsWith("```"))
                    json = json.Substring(3);
                if (json.EndsWith("```"))
                    json = json.Substring(0, json.Length - 3);
                json = json.Trim();

                var extracted = System.Text.Json.JsonSerializer.Deserialize<ExtractedBenefactorIdData>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return extracted;
            }
            catch
            {
                return null;
            }
        }

        public async Task<ExtractedCorData?> ExtractCorFieldsAsync(string rawText)
        {
            string prompt = $@"You are an expert AI assistant specialized in extracting structured data from Philippine Certificate of Registration (COR) and academic enrollment documents. Your task is to carefully analyze OCR text and extract specific academic information.

EXTRACT THE FOLLOWING FIELDS (return null for any field that cannot be found):

StudentNumber: Student ID or registration number (examples: 2021-12345, IT-2023-001, 202112345, 21-1234, A21-0001)
StudentName: Full student name exactly as written in document
Program: Complete program/course name (examples: Bachelor of Science in Computer Science, BS Information Technology, Bachelor of Arts in Psychology)
University: Full official institution name (examples: University of the Philippines Manila, Ateneo de Manila University)
YearLevel: Student's year level (examples: 1st Year, 2nd Year, 3rd Year, 4th Year, 5th Year, First Year, Second Year)
Address: Student's complete address if mentioned
PhoneNumber: Contact number (Philippine format: 09xxxxxxxxx, +63-xxx-xxx-xxxx, (02) xxxx-xxxx)
Semester: Current semester (examples: First Semester, Second Semester, Summer, 1st Semester, 2nd Semester)
AcademicYear: Academic year (examples: 2023-2024, AY 2024-2025, School Year 2023-24, SY 2024-2025)
SchoolYear: Alternative school year format if different from AcademicYear
College: College/department (examples: College of Engineering, School of Computer Studies, College of Liberal Arts)
Campus: Campus location if mentioned (examples: Manila Campus, Quezon City Campus, Main Campus)
EnrollmentStatus: Student status (examples: Regular, Irregular, Scholar, Transferee, New Student)
UnitsEnrolled: Total academic units/credits for the term (examples: 21 units, 18 credits, 24)
DateIssued: Date when COR was issued (examples: January 15, 2024, 01/15/2024, Jan 15, 2024)
TotalFees: Total fees or tuition amount (examples: ₱25,000, PHP 30,000, 25000.00)
GPA: Grade Point Average (examples: 1.25, 3.75, 89.5%)
YearStanding: Academic classification (examples: Freshman, Sophomore, Junior, Senior, 1st Year Standing)

ADVANCED EXTRACTION TECHNIQUES:
1. **Headers & Titles**: Look for 'CERTIFICATE OF REGISTRATION', 'ENROLLMENT FORM', 'STUDENT RECORD', 'COR'
2. **Student Numbers**: Match patterns like xxxx-xxxxx, letters+numbers, or pure numeric IDs with 6+ digits
3. **Names**: Handle formats like 'SURNAME, FIRSTNAME MIDDLENAME' or 'FIRSTNAME MIDDLENAME SURNAME'
4. **Programs**: Extract complete degree names, not just abbreviations (expand BS to Bachelor of Science, etc.)
5. **Institutions**: Use full official names, including University/College designation
6. **Year Levels**: Convert numeric to ordinal (1 → 1st Year, 2 → 2nd Year, etc.)
7. **Academic Years**: Recognize formats like AY 2023-24, SY 2023-2024, School Year 2023-2024
8. **Semesters**: Identify First/1st, Second/2nd, Summer terms
9. **Colleges**: Look for 'College of...', 'School of...', 'Department of...', 'Institute of...'
10. **Phone Numbers**: Extract Philippine mobile (09xxxxxxxxx) and landline formats
11. **Addresses**: Combine street, barangay, city, province information
12. **Status**: Identify enrollment classifications and student types
13. **Financial**: Extract peso amounts with various currency notations
14. **Dates**: Parse multiple date formats including Filipino month names
15. **Units**: Extract credit/unit counts, often near subject listings

QUALITY CHECKS:
- Ensure StudentNumber contains letters/numbers and is 5+ characters
- Verify Program contains 'Bachelor', 'Master', 'BS', 'BA', 'MS', etc.
- Check University contains 'University', 'College', 'Institute', or 'School'
- Validate YearLevel follows Philippine format (1st Year, 2nd Year, etc.)
- Ensure PhoneNumber starts with 09, +63, or has area code format
- Verify AcademicYear contains year range (YYYY-YYYY format)

Raw OCR Text:
{rawText}

Return ONLY a valid JSON object with the exact field names listed above. Use null for fields that cannot be confidently extracted:";

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var chatClient = _client.GetChatClient(_deploymentName);
                var messages = new OpenAI.Chat.ChatMessage[]
                {
                    new OpenAI.Chat.SystemChatMessage("You are an expert AI assistant specialized in extracting structured data from Philippine Certificate of Registration (COR) documents. You have deep knowledge of Philippine academic document formats, naming conventions, and educational systems. Always return properly formatted JSON with accurate field extraction."),
                    new OpenAI.Chat.UserChatMessage(prompt)
                };
                
                Console.WriteLine($"[COR Extraction] Starting extraction for text length: {rawText.Length} characters");
                Console.WriteLine($"[COR Extraction] Text preview: {rawText.Substring(0, Math.Min(200, rawText.Length))}...");
                
                var response = await chatClient.CompleteChatAsync(messages);
                stopwatch.Stop();
                
                // Track token usage
                await _tokenTrackingService.TrackChatCompletionUsageAsync(
                    completion: response.Value,
                    operation: "CorDocumentExtraction",
                    userId: null,
                    additionalDetails: $"COR text length: {rawText.Length}",
                    requestDuration: stopwatch.Elapsed
                );
                
                string json = response.Value.Content[0].Text.Trim();
                Console.WriteLine($"[COR Extraction] Raw AI response: {json}");
                
                // Clean up the JSON response (remove markdown code block markers if present)
                if (json.StartsWith("```json"))
                    json = json.Substring(7);
                if (json.StartsWith("```"))
                    json = json.Substring(3);
                if (json.EndsWith("```"))
                    json = json.Substring(0, json.Length - 3);
                json = json.Trim();
                
                Console.WriteLine($"[COR Extraction] Cleaned JSON: {json}");

                var extracted = System.Text.Json.JsonSerializer.Deserialize<ExtractedCorData>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (extracted != null)
                {
                    Console.WriteLine($"[COR Extraction] Successfully extracted: StudentNumber={extracted.StudentNumber}, Program={extracted.Program}, University={extracted.University}");
                }
                else
                {
                    Console.WriteLine("[COR Extraction] Warning: Extraction returned null object");
                }
                
                return extracted;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Track failed request
                await _tokenTrackingService.TrackFailedRequestAsync(
                    operation: "CorDocumentExtraction",
                    errorMessage: ex.Message,
                    userId: null,
                    requestDuration: stopwatch.Elapsed
                );
                
                // Enhanced error logging
                Console.WriteLine($"[COR Extraction] ERROR: {ex.Message}");
                Console.WriteLine($"[COR Extraction] Stack trace: {ex.StackTrace}");
                Console.WriteLine($"[COR Extraction] Input text length: {rawText?.Length ?? 0}");
                
                // Log specific JSON deserialization errors
                if (ex is System.Text.Json.JsonException jsonEx)
                {
                    Console.WriteLine($"[COR Extraction] JSON Error: {jsonEx.Message}");
                    Console.WriteLine($"[COR Extraction] Line: {jsonEx.LineNumber}, Position: {jsonEx.BytePositionInLine}");
                }
                
                return null;
            }
        }

        public async Task<ExtractedBenefactorAuthLetterData?> ExtractBenefactorAuthLetterFieldsAsync(string rawText)
        {
            string prompt = $@"You are an expert at extracting structured data from benefactor/donor organization authorization letters and official documents.

        Analyze the OCR text below from a benefactor/donor organization authorization letter and extract the following information. Return ONLY a JSON object with these exact field names:

        OrganizationName: The official name of the organization, company, foundation, or donor entity
        OrganizationType: Type of organization (Corporation, Foundation, Non-Profit Organization, Government Agency, Educational Institution, Healthcare Organization, Religious Organization, Individual Donor, etc.)
        Address: Physical or official address of the organization
        ContactNumber: Phone number or contact number of the organization
        Website: Organization website URL (if mentioned)
        AuthorizedRepresentativeName: Name of the PERSON WHO SIGNED/AUTHORIZED the letter - the supervisor, director, head, or person with authority who is GRANTING permission. Look for:
        - Person who signed at the bottom (e.g., 'Respectfully, Dr. John Smith, Director')
        - Person mentioned as Director, Head, Supervisor, Manager
        - The person GRANTING authorization (not the person receiving it)
        - Names near titles like 'Director', 'President', 'Head', 'Supervisor'
        - Person who appears in signature blocks or closing statements
        AuthorizedRepresentativeEmail: Email address mentioned anywhere in the document - contact info, letter body, or signatures
        OfficialEmailDomain: The email domain extracted from any email addresses found in the document

        CRITICAL EXTRACTION RULES:
        1. AuthorizedRepresentativeName: Find the person who SIGNED or AUTHORIZED the letter (the boss/supervisor), NOT the person being authorized
        2. Look for signature blocks, closing statements with titles (Director, Head, etc.)
        3. The authorizing person is usually at the end of the letter with their title
        4. AuthorizedRepresentativeEmail: Extract ANY email address found in the document

        Example patterns to look for:
        - 'Respectfully, Dr. Jane Smith, Director'
        - 'Sincerely, John Doe, Head of Department'
        - Signature sections with names and titles
        - Email: contact@organization.com or admin@company.gov.ph

Raw OCR Text:
{rawText}

Return only valid JSON:";

            var chatClient = _client.GetChatClient(_deploymentName);
            var messages = new OpenAI.Chat.ChatMessage[]
            {
                new OpenAI.Chat.SystemChatMessage("You are an expert at extracting structured data from benefactor/donor organization authorization letters. Always return only valid JSON for the requested fields."),
                new OpenAI.Chat.UserChatMessage(prompt)
            };
            var response = await chatClient.CompleteChatAsync(messages);
            string json = response.Value.Content[0].Text.Trim();

            try
            {
                // Clean up the JSON response (remove markdown code block markers if present)
                if (json.StartsWith("```json"))
                    json = json.Substring(7);
                if (json.StartsWith("```"))
                    json = json.Substring(3);
                if (json.EndsWith("```"))
                    json = json.Substring(0, json.Length - 3);
                json = json.Trim();

                var extracted = System.Text.Json.JsonSerializer.Deserialize<ExtractedBenefactorAuthLetterData>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return extracted;
            }
            catch (Exception ex)
            {
                // Log the error and return null
                Console.WriteLine($"Benefactor authorization letter extraction failed: {ex.Message}");
                Console.WriteLine($"Raw JSON response: {json}");
                return null;
            }
        }

        public async Task<ExtractedIdData?> ImprovePhilippineIdExtractionAsync(string rawText, ExtractedIdData? initialExtraction)
        {
            string prompt = $@"You are an expert at extracting data from Philippine identification documents. 

            The initial extraction found these fields:
            FirstName: {initialExtraction?.FirstName}
            MiddleName: {initialExtraction?.MiddleName}
            LastName: {initialExtraction?.LastName}
            Sex: {initialExtraction?.Sex}
            DateOfBirth: {initialExtraction?.DateOfBirth}

            Please analyze the raw OCR text below and improve the extraction, particularly for Philippine naming conventions:
            - In Philippine IDs, ""Mga Pangalan/Given Names"" typically contains first and middle names separated by space (like ""RALPH LORENZ"")
            - ""Apellido/Last Name"" is the family name (like ""MARILAO"")
            - ""Gitnang Apellido/Middle Name"" is typically shown separately and should be used as the middle name (like ""MANZON"")
            - Sex is usually shown as ""MALE"" or ""FEMALE"" and may be preceded by ""Kasarian/Sex:"" or just appear as the word itself
            - Dates are often in format ""JANUARY 09, 2004"" and should be converted to YYYY-MM-DD format
            - Look carefully for the word ""MALE"" or ""FEMALE"" anywhere in the text

            Raw OCR Text:
            {rawText}

            Please return ONLY a JSON object with these exact field names:
            FirstName, MiddleName, LastName, Sex, DateOfBirth (in YYYY-MM-DD format if found)

            JSON:";

            var chatClient = _client.GetChatClient(_deploymentName);
            var messages = new OpenAI.Chat.ChatMessage[]
            {
                new OpenAI.Chat.SystemChatMessage("You are an expert at extracting structured data from Philippine identification documents. Focus on proper name parsing according to Philippine naming conventions. Always return only valid JSON."),
                new OpenAI.Chat.UserChatMessage(prompt)
            };
            
            try
            {
                var response = await chatClient.CompleteChatAsync(messages);
                string json = response.Value.Content[0].Text.Trim();
                
                // Clean up the JSON response (remove markdown code block markers if present)
                if (json.StartsWith("```json"))
                    json = json.Substring(7);
                if (json.StartsWith("```"))
                    json = json.Substring(3);
                if (json.EndsWith("```"))
                    json = json.Substring(0, json.Length - 3);
                json = json.Trim();

                var extracted = System.Text.Json.JsonSerializer.Deserialize<ExtractedIdData>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return extracted;
            }
            catch (Exception ex)
            {
                // Log the error and return the original extraction
                Console.WriteLine($"AI improvement failed: {ex.Message}");
                return initialExtraction;
            }
        }
    }
}