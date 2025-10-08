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

        public async Task<ExtractedInstitutionIdData?> AnalyzeInstitutionIdDocumentAsync(IBrowserFile file)
        {
            var idData = await AnalyzeIdDocumentAsync(file);
            if (idData == null) return null;
            var extracted = new ExtractedInstitutionIdData
            {
                AdminFirstName = idData.FirstName,
                AdminMiddleName = idData.MiddleName,
                AdminLastName = idData.LastName,
                AdminEmail = idData.Nationality, // fallback: use Nationality as email if not present (should be improved)
                AdminContactNumber = idData.DocumentNumber, // fallback: use DocumentNumber as contact
                AdminPosition = idData.Sex, // fallback: use Sex as position (should be improved)
                InstitutionalEmailDomain = null // Will be enhanced with proper AI extraction later
            };
            return extracted;
        }

        public async Task<ExtractedInstitutionAuthLetterData?> AnalyzeInstitutionAuthLetterAsync(IBrowserFile file)
        {
            var corData = await AnalyzeCorDocumentAsync(file);
            if (corData == null) return null;
            var extracted = new ExtractedInstitutionAuthLetterData
            {
                InstitutionName = corData.University,
                InstitutionType = null, // Not extracted from layout
                Address = corData.Address,
                ContactNumber = corData.StudentNumber, // fallback: use StudentNumber as contact
                Website = null,
                Description = corData.Program,
                DeanName = corData.StudentName,
                DeanEmail = null,
                InstitutionalEmailDomain = null // Will be enhanced with proper AI extraction later
            };
            return extracted;
        }

        // ...existing methods (AnalyzeIdDocumentAsync, AnalyzeCorDocumentAsync, MapIdDocumentFields, PollForResults, ExtractCorDataFromContent)...
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly ILogger<DocumentIntelligenceService> _logger;

        public DocumentIntelligenceService(HttpClient httpClient, IConfiguration config, ILogger<DocumentIntelligenceService> logger)
        {
            _httpClient = httpClient;
            _endpoint = config["AzureDocumentIntelligence:Endpoint"] ?? "";
            _apiKey = config["AzureDocumentIntelligence:ApiKey"] ?? "";
            _logger = logger;
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

                    var extractedData = MapIdDocumentFields(docResponse);
                    
                    if (extractedData != null)
                    {
                        _logger.LogInformation($"Successfully extracted data from {file.Name}: Name={extractedData.FirstName} {extractedData.LastName}, DOB={extractedData.DateOfBirth}");
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

            // Extract first name
            if (fields.TryGetValue("FirstName", out var firstName))
                extractedData.FirstName = firstName.ValueString ?? firstName.Content;

            // Extract middle name  
            if (fields.TryGetValue("MiddleName", out var middleName))
                extractedData.MiddleName = middleName.ValueString ?? middleName.Content;

            // Extract last name
            if (fields.TryGetValue("LastName", out var lastName))
                extractedData.LastName = lastName.ValueString ?? lastName.Content;

            // Extract sex/gender
            if (fields.TryGetValue("Sex", out var sex))
                extractedData.Sex = sex.ValueString ?? sex.Content;

            // Extract date of birth
            if (fields.TryGetValue("DateOfBirth", out var dob))
            {
                var dobString = dob.ValueDate ?? dob.Content;
                if (DateTime.TryParse(dobString, out var parsedDate))
                    extractedData.DateOfBirth = parsedDate;
            }

            // Extract address
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

            // Extract document number (could be used as reference)
            if (fields.TryGetValue("DocumentNumber", out var docNumber))
                extractedData.DocumentNumber = docNumber.ValueString ?? docNumber.Content;

            // Extract nationality
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