using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace c2_eskolar.Services
{
    public class ExtractedInstitutionIdData
    {
        public string? AdminFirstName { get; set; }
        public string? AdminMiddleName { get; set; }
        public string? AdminLastName { get; set; }
        public string? AdminEmail { get; set; }
        public string? AdminContactNumber { get; set; }
        public string? AdminPosition { get; set; }
        public string? InstitutionalEmailDomain { get; set; }
        public string? Sex { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class ExtractedInstitutionAuthLetterData
    {
        public string? InstitutionName { get; set; }
        public string? InstitutionType { get; set; }
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public string? DeanName { get; set; }
        public string? DeanEmail { get; set; }
        public string? InstitutionalEmailDomain { get; set; }
    }

    public class ExtractedBenefactorIdData
    {
        public string? AdminFirstName { get; set; }
        public string? AdminMiddleName { get; set; }
        public string? AdminLastName { get; set; }
        public string? AdminEmail { get; set; }
        public string? AdminContactNumber { get; set; }
        public string? AdminPosition { get; set; }
        public string? Sex { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
    }

    public class ExtractedBenefactorAuthLetterData
    {
        public string? OrganizationName { get; set; }
        public string? OrganizationType { get; set; }
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? Website { get; set; }
        public string? AuthorizedRepresentativeName { get; set; }
        public string? AuthorizedRepresentativeEmail { get; set; }
        public string? OfficialEmailDomain { get; set; }
    }

    public class ExtractedIdData
    {
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Sex { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Nationality { get; set; }
    }

    public class ExtractedCorData
    {
        public string? StudentNumber { get; set; }
        public string? StudentName { get; set; }
        public string? Program { get; set; }
        public string? University { get; set; }
        public string? YearLevel { get; set; }
        public string? Address { get; set; }
    }

    public class DocumentIntelligenceResponse
    {
        [JsonPropertyName("analyzeResult")]
        public AnalyzeResult? AnalyzeResult { get; set; }
    }

    public class AnalyzeResult
    {
        [JsonPropertyName("documents")]
        public Document[]? Documents { get; set; }
        
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    public class Document
    {
        [JsonPropertyName("docType")]
        public string? DocType { get; set; }
        
        [JsonPropertyName("fields")]
        public Dictionary<string, Field>? Fields { get; set; }
    }

    public class Field
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        
        [JsonPropertyName("content")]
        public string? Content { get; set; }
        
        [JsonPropertyName("valueString")]
        public string? ValueString { get; set; }
        
        [JsonPropertyName("valueDate")]
        public string? ValueDate { get; set; }
        
        [JsonPropertyName("valueAddress")]
        public Address? ValueAddress { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("streetAddress")]
        public string? StreetAddress { get; set; }
        
        [JsonPropertyName("municipality")]
        public string? Municipality { get; set; }
        
        [JsonPropertyName("state")]
        public string? State { get; set; }
        
        [JsonPropertyName("countryRegion")]
        public string? CountryRegion { get; set; }
        
        [JsonPropertyName("postalCode")]
        public string? PostalCode { get; set; }
    }

    public class DocumentIntelligenceService
    {

        private readonly OpenAIService _openAIService;

        public async Task<ExtractedInstitutionIdData?> AnalyzeInstitutionIdDocumentAsync(IBrowserFile file)
        {
            var idData = await AnalyzeIdDocumentAsync(file);
            if (idData == null) return null;

            // Step 1: Get raw text from OCR fields
            string rawText = $"{idData.FirstName} {idData.MiddleName} {idData.LastName} {idData.Sex} {idData.DateOfBirth} {idData.Address} {idData.DocumentNumber} {idData.Nationality}";

            // Step 2: Use OpenAI to extract semantic fields
            var aiExtracted = await _openAIService.ExtractInstitutionIdFieldsAsync(rawText);

            // Step 3: Merge extracted fields (AI + OCR)
            var extracted = new ExtractedInstitutionIdData
            {
                AdminFirstName = aiExtracted?.AdminFirstName ?? idData.FirstName,
                AdminMiddleName = aiExtracted?.AdminMiddleName ?? idData.MiddleName,
                AdminLastName = aiExtracted?.AdminLastName ?? idData.LastName,
                AdminEmail = aiExtracted?.AdminEmail,
                AdminContactNumber = aiExtracted?.AdminContactNumber,
                AdminPosition = aiExtracted?.AdminPosition,
                InstitutionalEmailDomain = aiExtracted?.InstitutionalEmailDomain,
                Sex = idData.Sex,
                DateOfBirth = idData.DateOfBirth
            };
            return extracted;
        }

        public async Task<ExtractedBenefactorIdData?> AnalyzeBenefactorIdDocumentAsync(IBrowserFile file)
        {
            var idData = await AnalyzeIdDocumentAsync(file);
            if (idData == null) return null;

            // Step 1: Get raw text from OCR fields
            string rawText = $"{idData.FirstName} {idData.MiddleName} {idData.LastName} {idData.Sex} {idData.DateOfBirth} {idData.Address} {idData.DocumentNumber} {idData.Nationality}";

            // Step 2: Use OpenAI to extract semantic fields for benefactor-specific data
            var aiExtracted = await _openAIService.ExtractBenefactorIdFieldsAsync(rawText);

            // Step 3: Merge extracted fields (AI + OCR), prioritizing AI extraction for names
            var extracted = new ExtractedBenefactorIdData
            {
                AdminFirstName = aiExtracted?.AdminFirstName ?? idData.FirstName,
                AdminMiddleName = aiExtracted?.AdminMiddleName ?? idData.MiddleName,
                AdminLastName = aiExtracted?.AdminLastName ?? idData.LastName,
                AdminEmail = aiExtracted?.AdminEmail,
                AdminContactNumber = aiExtracted?.AdminContactNumber,
                AdminPosition = aiExtracted?.AdminPosition,
                Sex = aiExtracted?.Sex ?? idData.Sex,
                DateOfBirth = aiExtracted?.DateOfBirth ?? idData.DateOfBirth,
                Nationality = aiExtracted?.Nationality ?? idData.Nationality
            };
            return extracted;
        }

        public async Task<ExtractedBenefactorAuthLetterData?> AnalyzeBenefactorAuthLetterAsync(IBrowserFile file)
        {
            try
            {
                if (string.IsNullOrEmpty(_endpoint) || string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogError("Document Intelligence endpoint or API key is not configured");
                    return null;
                }

                // Validate file type and size
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var fileExt = Path.GetExtension(file.Name).ToLowerInvariant();
                if (!validExtensions.Contains(fileExt))
                {
                    _logger.LogWarning($"Invalid file type: {file.Name}. Supported types: {string.Join(", ", validExtensions)}");
                    return null;
                }

                if (file.Size > 5 * 1024 * 1024) // 5MB limit
                {
                    _logger.LogWarning($"File too large: {file.Name} ({file.Size} bytes). Maximum size: 5MB");
                    return null;
                }

                _logger.LogInformation($"Analyzing benefactor authorization letter: {file.Name} ({file.Size} bytes) with endpoint: {_endpoint}");

                // Read file content into a byte array and convert to base64
                using var stream = file.OpenReadStream(5 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64Content = Convert.ToBase64String(fileBytes);

                // Use layout analysis model for general document analysis
                var url = $"{_endpoint}/documentintelligence/documentModels/prebuilt-layout:analyze?_overload=analyzeDocument&api-version=2024-11-30";

                // Create JSON request body with base64Source as required by the API
                var requestBody = new
                {
                    base64Source = base64Content
                };
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

                // Make the request
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

                var response = await _httpClient.PostAsync(url, jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Document Intelligence analysis started. Polling for results...");
                    var operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                    if (string.IsNullOrEmpty(operationLocation))
                    {
                        _logger.LogError("Operation-Location header not found in response");
                        return null;
                    }

                    // Poll for results
                    var maxAttempts = 30;
                    var attempt = 0;
                    while (attempt < maxAttempts)
                    {
                        await Task.Delay(1000); // Wait 1 second before checking
                        attempt++;

                        var pollResponse = await _httpClient.GetAsync(operationLocation);
                        if (pollResponse.IsSuccessStatusCode)
                        {
                            var pollContent = await pollResponse.Content.ReadAsStringAsync();
                            var pollResult = JsonSerializer.Deserialize<DocumentIntelligenceResponse>(pollContent);

                            if (pollResult?.AnalyzeResult?.Content != null)
                            {
                                _logger.LogInformation("Document analysis completed successfully");
                                _logger.LogInformation($"OCR text length: {pollResult.AnalyzeResult.Content.Length} characters");
                                _logger.LogInformation($"OCR preview: {pollResult.AnalyzeResult.Content.Substring(0, Math.Min(200, pollResult.AnalyzeResult.Content.Length))}...");
                                
                                // Use OpenAI to extract benefactor-specific fields from the raw text
                                var extractedData = await _openAIService.ExtractBenefactorAuthLetterFieldsAsync(pollResult.AnalyzeResult.Content);
                                
                                if (extractedData != null)
                                {
                                    _logger.LogInformation($"Successfully extracted data: OrganizationName={extractedData.OrganizationName}, OrganizationType={extractedData.OrganizationType}");
                                }
                                else
                                {
                                    _logger.LogWarning("OpenAI extraction returned null - check extraction logic");
                                }
                                
                                return extractedData;
                            }
                        }
                    }

                    _logger.LogWarning("Polling timeout reached");
                    return null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Document Intelligence API error: {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing benefactor authorization letter");
                return null;
            }
        }

        public async Task<ExtractedInstitutionAuthLetterData?> AnalyzeInstitutionAuthLetterAsync(IBrowserFile file)
        {
            try
            {
                if (string.IsNullOrEmpty(_endpoint) || string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogError("Document Intelligence endpoint or API key is not configured");
                    return null;
                }

                // Validate file type and size
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var fileExt = Path.GetExtension(file.Name).ToLowerInvariant();
                if (!validExtensions.Contains(fileExt))
                {
                    _logger.LogWarning($"Invalid file type: {file.Name}. Supported types: {string.Join(", ", validExtensions)}");
                    return null;
                }

                if (file.Size > 5 * 1024 * 1024) // 5MB limit
                {
                    _logger.LogWarning($"File too large: {file.Name} ({file.Size} bytes). Maximum size: 5MB");
                    return null;
                }

                _logger.LogInformation($"Analyzing authorization letter: {file.Name} ({file.Size} bytes) with endpoint: {_endpoint}");

                // Read file content into a byte array and convert to base64
                using var stream = file.OpenReadStream(5 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64Content = Convert.ToBase64String(fileBytes);

                // Use layout analysis model for general document analysis instead of prebuilt-idDocument
                var url = $"{_endpoint}/documentintelligence/documentModels/prebuilt-layout:analyze?_overload=analyzeDocument&api-version=2024-11-30";

                // Create JSON request body with base64Source as required by the API
                var requestBody = new
                {
                    base64Source = base64Content
                };
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

                // Make the request
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

                var response = await _httpClient.PostAsync(url, jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Document Intelligence analysis started. Polling for results...");
                    var operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                    if (string.IsNullOrEmpty(operationLocation))
                    {
                        _logger.LogError("Operation-Location header not found in response");
                        return null;
                    }

                    // Poll for results
                    var resultJson = await PollForResults(operationLocation);
                    if (string.IsNullOrEmpty(resultJson))
                    {
                        _logger.LogError("Failed to get analysis results from Document Intelligence");
                        return null;
                    }

                    _logger.LogInformation($"Document Intelligence response received for {file.Name}");
                    
                    var docResponse = JsonSerializer.Deserialize<DocumentIntelligenceResponse>(resultJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (docResponse != null)
                    {
                        _logger.LogInformation($"Document analysis completed successfully for {file.Name}");
                        
                        // Get the raw OCR content for AI processing
                        string rawContent = docResponse.AnalyzeResult?.Content ?? "";
                        
                        if (!string.IsNullOrWhiteSpace(rawContent))
                        {
                            _logger.LogInformation($"Raw content extracted ({rawContent.Length} characters), sending to AI for field extraction...");
                            
                            // Use AI to extract structured fields from the raw content
                            var aiExtracted = await _openAIService.ExtractInstitutionFieldsAsync(rawContent);
                            
                            if (aiExtracted != null)
                            {
                                _logger.LogInformation($"AI successfully extracted fields: InstitutionName={aiExtracted.InstitutionName}, DeanName={aiExtracted.DeanName}");
                                return aiExtracted;
                            }
                            else
                            {
                                _logger.LogWarning("AI extraction returned null");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("No raw content extracted from document");
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to deserialize Document Intelligence response");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Document Intelligence API error: {response.StatusCode}, Content: {errorContent}");
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error analyzing authorization letter: {file.Name}");
                return null;
            }
        }

        // ...existing methods (AnalyzeIdDocumentAsync, AnalyzeCorDocumentAsync, MapIdDocumentFields, PollForResults, ExtractCorDataFromContent)...
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly ILogger<DocumentIntelligenceService> _logger;

        public DocumentIntelligenceService(HttpClient httpClient, IConfiguration config, ILogger<DocumentIntelligenceService> logger, OpenAIService openAIService)
        {
            _httpClient = httpClient;
            _endpoint = config["AzureDocumentIntelligence:Endpoint"] ?? "";
            _apiKey = config["AzureDocumentIntelligence:ApiKey"] ?? "";
            _logger = logger;
            _openAIService = openAIService;
        }

        public async Task<ExtractedIdData?> AnalyzeIdDocumentAsync(IBrowserFile file)
        {
            try
            {
                if (string.IsNullOrEmpty(_endpoint) || string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogError("Document Intelligence endpoint or API key is not configured");
                    return null;
                }

                // Validate file type and size
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var fileExt = Path.GetExtension(file.Name).ToLowerInvariant();
                if (!validExtensions.Contains(fileExt))
                {
                    _logger.LogWarning($"Invalid file type: {file.Name}. Supported types: {string.Join(", ", validExtensions)}");
                    return null;
                }

                if (file.Size > 5 * 1024 * 1024) // 5MB limit
                {
                    _logger.LogWarning($"File too large: {file.Name} ({file.Size} bytes). Maximum size: 5MB");
                    return null;
                }

                // Read file content into a byte array and convert to base64
                using var stream = file.OpenReadStream(5 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64Content = Convert.ToBase64String(fileBytes);

                // Create JSON request body with base64Source as required by the API
                var requestBody = new
                {
                    base64Source = base64Content
                };
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody), 
                    System.Text.Encoding.UTF8, 
                    "application/json"
                );

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

                // Fix the URL format - use proper endpoint structure with _overload parameter
                var cleanEndpoint = _endpoint.TrimEnd('/');
                var url = $"{cleanEndpoint}/documentintelligence/documentModels/prebuilt-idDocument:analyze?_overload=analyzeDocument&api-version=2024-11-30";
                
                _logger.LogInformation($"Analyzing ID document: {file.Name} ({file.Size} bytes) with endpoint: {url}");
                var response = await _httpClient.PostAsync(url, jsonContent);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    // Get the operation location from headers for polling
                    var operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                    if (string.IsNullOrEmpty(operationLocation))
                    {
                        _logger.LogError("No Operation-Location header found in Document Intelligence response");
                        return null;
                    }

                    _logger.LogInformation($"Document Intelligence analysis started. Polling for results...");
                    
                    // Poll for results
                    var resultJson = await PollForResults(operationLocation);
                    if (string.IsNullOrEmpty(resultJson))
                    {
                        _logger.LogError("Failed to get analysis results from Document Intelligence");
                        return null;
                    }

                    _logger.LogInformation($"Document Intelligence response received for {file.Name}");
                    _logger.LogDebug($"Raw response: {resultJson}");

                    var docResponse = JsonSerializer.Deserialize<DocumentIntelligenceResponse>(resultJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Log all detected fields for debugging
                    if (docResponse?.AnalyzeResult?.Documents != null && docResponse.AnalyzeResult.Documents.Length > 0)
                    {
                        var fields = docResponse.AnalyzeResult.Documents[0].Fields;
                        if (fields != null)
                        {
                            foreach (var kvp in fields)
                            {
                                _logger.LogInformation($"Field: {kvp.Key} => ValueString: {kvp.Value.ValueString}, Content: {kvp.Value.Content}, ValueDate: {kvp.Value.ValueDate}");
                            }
                        }
                    }

                    var extractedData = MapIdDocumentFields(docResponse);

                    // Fallback extraction from raw content if key fields are missing
                    if (extractedData != null && (string.IsNullOrWhiteSpace(extractedData.FirstName) || string.IsNullOrWhiteSpace(extractedData.LastName) || string.IsNullOrWhiteSpace(extractedData.Sex) || extractedData.DateOfBirth == null))
                    {
                        var rawContent = docResponse?.AnalyzeResult?.Content;
                        if (!string.IsNullOrWhiteSpace(rawContent))
                        {
                            // Name extraction fallback
                            var nameMatch = System.Text.RegularExpressions.Regex.Match(rawContent, @"Given Names[:]?\s*([A-Z ]+)");
                            if (nameMatch.Success)
                            {
                                var givenNames = nameMatch.Groups[1].Value.Trim();
                                var nameParts = givenNames.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (nameParts.Length > 0)
                                    extractedData.FirstName = nameParts[0];
                                if (nameParts.Length > 1)
                                    extractedData.MiddleName = nameParts[1];
                            }
                            var lastNameMatch = System.Text.RegularExpressions.Regex.Match(rawContent, @"Last Name[:]?\s*([A-Z ]+)");
                            if (lastNameMatch.Success)
                                extractedData.LastName = lastNameMatch.Groups[1].Value.Trim();

                                // Sex extraction fallback: look for 'Sex' or 'Kasarian' followed by MALE/FEMALE, or just MALE/FEMALE anywhere
                                var sexMatch = System.Text.RegularExpressions.Regex.Match(rawContent, @"(Sex|Kasarian)[:]?[ \t]*([Mm][Aa][Ll][Ee]|[Ff][Ee][Mm][Aa][Ll][Ee])", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                if (sexMatch.Success)
                                    extractedData.Sex = sexMatch.Groups[2].Value.ToUpper();
                                else
                                {
                                    // Fallback: look for MALE or FEMALE anywhere in the content
                                    var maleMatch = System.Text.RegularExpressions.Regex.Match(rawContent, @"\bMALE\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                    var femaleMatch = System.Text.RegularExpressions.Regex.Match(rawContent, @"\bFEMALE\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                    if (maleMatch.Success)
                                        extractedData.Sex = "MALE";
                                    else if (femaleMatch.Success)
                                        extractedData.Sex = "FEMALE";
                                }

                            // Birthday extraction fallback
                            var dobMatch = System.Text.RegularExpressions.Regex.Match(rawContent, @"Date of Birth[:]?\s*([A-Z0-9, ]+)");
                            if (dobMatch.Success)
                            {
                                var dobStr = dobMatch.Groups[1].Value.Trim();
                                DateTime parsedDate;
                                var formats = new[] { "MMMM dd, yyyy", "MMM dd, yyyy", "MMMM d, yyyy", "MMM d, yyyy" };
                                foreach (var fmt in formats)
                                {
                                    if (DateTime.TryParseExact(dobStr, fmt, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate))
                                    {
                                        extractedData.DateOfBirth = parsedDate;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Use AI to improve extraction, especially for Philippine ID naming conventions
                    if (extractedData != null && docResponse?.AnalyzeResult?.Content != null)
                    {
                        _logger.LogInformation($"Initial extraction: FirstName={extractedData.FirstName}, MiddleName={extractedData.MiddleName}, LastName={extractedData.LastName}");
                        
                        var improvedData = await _openAIService.ImprovePhilippineIdExtractionAsync(docResponse.AnalyzeResult.Content, extractedData);
                        if (improvedData != null)
                        {
                            _logger.LogInformation($"AI improved extraction: FirstName={improvedData.FirstName}, MiddleName={improvedData.MiddleName}, LastName={improvedData.LastName}, Address={improvedData.Address}");
                            // Only overwrite address if AI returns a non-empty value
                            if (!string.IsNullOrWhiteSpace(improvedData.Address))
                                extractedData.Address = improvedData.Address;
                            // Overwrite other fields as usual
                            extractedData.FirstName = improvedData.FirstName;
                            extractedData.MiddleName = improvedData.MiddleName;
                            extractedData.LastName = improvedData.LastName;
                            extractedData.Sex = improvedData.Sex;
                            extractedData.DateOfBirth = improvedData.DateOfBirth;
                            extractedData.DocumentNumber = improvedData.DocumentNumber;
                            extractedData.Nationality = improvedData.Nationality;
                        }
                    }

                    if (extractedData != null)
                    {
                        _logger.LogInformation($"Successfully extracted data from {file.Name}: FirstName={extractedData.FirstName}, MiddleName={extractedData.MiddleName}, LastName={extractedData.LastName}, Sex={extractedData.Sex}, DOB={extractedData.DateOfBirth}");
                    }
                    else
                    {
                        _logger.LogWarning($"No data could be extracted from {file.Name}");
                    }

                    return extractedData;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Document Intelligence API error: {response.StatusCode}, Content: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error analyzing ID document: {file.Name}");
                return null;
            }
        }

        public async Task<ExtractedCorData?> AnalyzeCorDocumentAsync(IBrowserFile file)
        {
            try
            {
                if (string.IsNullOrEmpty(_endpoint) || string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogError("Document Intelligence endpoint or API key is not configured");
                    return null;
                }

                // Validate file type and size
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var fileExt = Path.GetExtension(file.Name).ToLowerInvariant();
                if (!validExtensions.Contains(fileExt))
                {
                    _logger.LogWarning($"Invalid file type: {file.Name}. Supported types: {string.Join(", ", validExtensions)}");
                    return null;
                }

                if (file.Size > 5 * 1024 * 1024) // 5MB limit
                {
                    _logger.LogWarning($"File too large: {file.Name} ({file.Size} bytes). Maximum size: 5MB");
                    return null;
                }

                // Read file content into a byte array and convert to base64
                using var stream = file.OpenReadStream(5 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64Content = Convert.ToBase64String(fileBytes);

                // Create JSON request body with base64Source as required by the API
                var requestBody = new
                {
                    base64Source = base64Content
                };
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody), 
                    System.Text.Encoding.UTF8, 
                    "application/json"
                );

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

                // Fix the URL format - use proper endpoint structure for layout model with _overload parameter
                var cleanEndpoint = _endpoint.TrimEnd('/');
                var url = $"{cleanEndpoint}/documentintelligence/documentModels/prebuilt-layout:analyze?_overload=analyzeDocument&api-version=2024-11-30";
                
                _logger.LogInformation($"Analyzing COR document: {file.Name} ({file.Size} bytes) with endpoint: {url}");
                var response = await _httpClient.PostAsync(url, jsonContent);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    // Get the operation location from headers for polling
                    var operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                    if (string.IsNullOrEmpty(operationLocation))
                    {
                        _logger.LogError("No Operation-Location header found in Document Intelligence response");
                        return null;
                    }

                    _logger.LogInformation($"Document Intelligence analysis started. Polling for results...");
                    
                    // Poll for results
                    var resultJson = await PollForResults(operationLocation);
                    if (string.IsNullOrEmpty(resultJson))
                    {
                        _logger.LogError("Failed to get analysis results from Document Intelligence");
                        return null;
                    }

                    _logger.LogInformation($"Document Intelligence response received for {file.Name}");

                    var docResponse = JsonSerializer.Deserialize<DocumentIntelligenceResponse>(resultJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    _logger.LogDebug($"Raw COR content extracted: {docResponse?.AnalyzeResult?.Content?.Substring(0, Math.Min(500, docResponse?.AnalyzeResult?.Content?.Length ?? 0))}...");

                    var extractedData = ExtractCorDataFromContent(docResponse?.AnalyzeResult?.Content);
                    
                    if (extractedData != null)
                    {
                        _logger.LogInformation($"Successfully extracted COR data from {file.Name}: Student={extractedData.StudentNumber}, Program={extractedData.Program}");
                    }
                    else
                    {
                        _logger.LogWarning($"No COR data could be extracted from {file.Name}");
                    }

                    return extractedData;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Document Intelligence API error: {response.StatusCode}, Content: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error analyzing COR document: {file.Name}");
                return null;
            }
        }

        private ExtractedIdData? MapIdDocumentFields(DocumentIntelligenceResponse? response)
        {
            if (response?.AnalyzeResult?.Documents == null || response.AnalyzeResult.Documents.Length == 0)
                return null;

            var document = response.AnalyzeResult.Documents[0];
            var fields = document.Fields;
            
            if (fields == null) return null;

            var extractedData = new ExtractedIdData();

            // Improved name extraction
            string? givenNames = null;
            if (fields.TryGetValue("FirstName", out var firstName))
                givenNames = firstName.ValueString ?? firstName.Content;
            if (!string.IsNullOrWhiteSpace(givenNames))
            {
                var nameParts = givenNames.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length == 1)
                {
                    extractedData.FirstName = nameParts[0];
                }
                else if (nameParts.Length == 2)
                {
                    extractedData.FirstName = nameParts[0];
                    extractedData.MiddleName = nameParts[1];
                }
                else if (nameParts.Length > 2)
                {
                    extractedData.FirstName = nameParts[0];
                    extractedData.MiddleName = string.Join(" ", nameParts.Skip(1));
                }
            }
            // If MiddleName field exists and is not empty, use it
            if (fields.TryGetValue("MiddleName", out var middleName))
            {
                var middle = middleName.ValueString ?? middleName.Content;
                if (!string.IsNullOrWhiteSpace(middle))
                    extractedData.MiddleName = middle;
            }
            // Last name
            if (fields.TryGetValue("LastName", out var lastName))
                extractedData.LastName = lastName.ValueString ?? lastName.Content;

            // Sex/gender (handle common Philippine ID field names)
            if (fields.TryGetValue("Sex", out var sex))
            {
                var sexValue = sex.ValueString ?? sex.Content;
                if (!string.IsNullOrWhiteSpace(sexValue))
                    extractedData.Sex = sexValue.Trim().ToUpper();
            }
            else if (fields.TryGetValue("Kasarian", out var kasarian))
            {
                var sexValue = kasarian.ValueString ?? kasarian.Content;
                if (!string.IsNullOrWhiteSpace(sexValue))
                    extractedData.Sex = sexValue.Trim().ToUpper();
            }

            // Date of birth (robust parsing for formats like "JANUARY 09, 2004")
            if (fields.TryGetValue("DateOfBirth", out var dob))
            {
                var dobString = dob.ValueDate ?? dob.Content;
                if (!string.IsNullOrWhiteSpace(dobString))
                {
                    DateTime parsedDate;
                    if (DateTime.TryParse(dobString, out parsedDate))
                    {
                        extractedData.DateOfBirth = parsedDate;
                    }
                    else
                    {
                        // Try parsing with custom formats
                        var formats = new[] { "MMMM dd, yyyy", "MMM dd, yyyy", "MMMM d, yyyy", "MMM d, yyyy" };
                        foreach (var fmt in formats)
                        {
                            if (DateTime.TryParseExact(dobString.Trim(), fmt, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate))
                            {
                                extractedData.DateOfBirth = parsedDate;
                                break;
                            }
                        }
                    }
                }
            }

            // Address
            if (fields.TryGetValue("Address", out var address))
            {
                if (address.ValueAddress != null)
                {
                    var addr = address.ValueAddress;
                    var addressParts = new List<string>();
                    if (!string.IsNullOrEmpty(addr.StreetAddress)) addressParts.Add(addr.StreetAddress);
                    if (!string.IsNullOrEmpty(addr.Municipality)) addressParts.Add(addr.Municipality);
                    if (!string.IsNullOrEmpty(addr.State)) addressParts.Add(addr.State);
                    if (!string.IsNullOrEmpty(addr.PostalCode)) addressParts.Add(addr.PostalCode);
                    extractedData.Address = string.Join(", ", addressParts);
                }
                else
                {
                    extractedData.Address = address.ValueString ?? address.Content;
                }
            }

            // Document number
            if (fields.TryGetValue("DocumentNumber", out var docNumber))
                extractedData.DocumentNumber = docNumber.ValueString ?? docNumber.Content;

            // Nationality
            if (fields.TryGetValue("Nationality", out var nationality))
                extractedData.Nationality = nationality.ValueString ?? nationality.Content;

            return extractedData;
        }

        private async Task<string?> PollForResults(string operationLocation)
        {
            var maxAttempts = 30; // 30 attempts with 2-second delays = 1 minute max
            var delay = TimeSpan.FromSeconds(2);

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

                    var response = await _httpClient.GetAsync(operationLocation);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Error polling for results: {response.StatusCode}");
                        return null;
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var resultDoc = JsonDocument.Parse(responseContent);
                    
                    if (resultDoc.RootElement.TryGetProperty("status", out var statusElement))
                    {
                        var status = statusElement.GetString();
                        _logger.LogDebug($"Polling attempt {attempt + 1}: Status = {status}");

                        if (status == "succeeded")
                        {
                            _logger.LogInformation("Document analysis completed successfully");
                            return responseContent;
                        }
                        else if (status == "failed")
                        {
                            _logger.LogError("Document analysis failed");
                            return null;
                        }
                        // If status is "running" or "notStarted", continue polling
                    }

                    if (attempt < maxAttempts - 1)
                    {
                        await Task.Delay(delay);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error during polling attempt {attempt + 1}");
                    if (attempt < maxAttempts - 1)
                    {
                        await Task.Delay(delay);
                    }
                }
            }

            _logger.LogError("Polling timeout - analysis did not complete within expected time");
            return null;
        }

        private ExtractedCorData? ExtractCorDataFromContent(string? content)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            var extractedData = new ExtractedCorData();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Basic pattern matching for common COR fields
            foreach (var line in lines)
            {
                var normalizedLine = line.Trim().ToUpperInvariant();

                // Look for student number patterns
                if ((normalizedLine.Contains("STUDENT") && normalizedLine.Contains("NUMBER")) ||
                    (normalizedLine.Contains("STUDENT") && normalizedLine.Contains("ID")))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"[\d-]+");
                    if (match.Success && match.Value.Length >= 6)
                        extractedData.StudentNumber = match.Value;
                }

                // Look for program/course patterns
                if (normalizedLine.Contains("COURSE") || normalizedLine.Contains("PROGRAM") || 
                    normalizedLine.Contains("DEGREE"))
                {
                    // Extract the program name (everything after the keyword)
                    var keywords = new[] { "COURSE", "PROGRAM", "DEGREE" };
                    foreach (var keyword in keywords)
                    {
                        var index = normalizedLine.IndexOf(keyword);
                        if (index >= 0)
                        {
                            var programPart = line.Substring(index + keyword.Length).Trim();
                            if (!string.IsNullOrEmpty(programPart) && programPart.Length > 3)
                            {
                                extractedData.Program = programPart.Split(':').LastOrDefault()?.Trim();
                                break;
                            }
                        }
                    }
                }

                // Look for university name
                if ((normalizedLine.Contains("UNIVERSITY") || normalizedLine.Contains("COLLEGE")) &&
                    !normalizedLine.Contains("COURSE"))
                {
                    extractedData.University = line.Trim();
                }

                // Look for year level
                if (normalizedLine.Contains("YEAR") && (normalizedLine.Contains("1ST") || 
                    normalizedLine.Contains("2ND") || normalizedLine.Contains("3RD") || 
                    normalizedLine.Contains("4TH") || normalizedLine.Contains("5TH")))
                {
                    var yearMatch = System.Text.RegularExpressions.Regex.Match(normalizedLine, @"(\d)(ST|ND|RD|TH)\s*YEAR");
                    if (yearMatch.Success)
                        extractedData.YearLevel = $"{yearMatch.Groups[1].Value}{yearMatch.Groups[2].Value.ToLower()} Year";
                }
            }

            return extractedData;
        }
    }
    }