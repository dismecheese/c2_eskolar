// using Microsoft.AspNetCore.Components.Forms;
// using System.Net.Http.Headers;
// using System.Text.Json;
// using System.Text.Json.Serialization;

// namespace c2_eskolar.Services
// {
//     public class ExtractedData
//     {
//         public string? StudentId { get; set; }
//         public double? GWA { get; set; }
//         public string? FirstSemesterGrades { get; set; }
//         public string? SecondSemesterGrades { get; set; }
//         // Add more fields as needed
//     }

//     public class DocumentIntelligenceService
//     {
//         private readonly HttpClient _httpClient;
//         private readonly string _endpoint;
//         private readonly string _apiKey;

//         public DocumentIntelligenceService(HttpClient httpClient, IConfiguration config)
//         {
//             _httpClient = httpClient;
//             _endpoint = config["DocumentIntelligence:Endpoint"] ?? "";
//             _apiKey = config["DocumentIntelligence:ApiKey"] ?? "";
//         }

//         public async Task<ExtractedData?> AnalyzeDocumentAsync(IBrowserFile file)
//         {
//             if (string.IsNullOrEmpty(_endpoint) || string.IsNullOrEmpty(_apiKey))
//                 return null;

//             using var content = new MultipartFormDataContent();
//             var stream = file.OpenReadStream(20 * 1024 * 1024);
//             content.Add(new StreamContent(stream), "file", file.Name);

//             _httpClient.DefaultRequestHeaders.Clear();
//             _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

//             var url = $"{_endpoint}/formrecognizer/documentModels/prebuilt-document:analyze?api-version=2023-07-31";
//             var response = await _httpClient.PostAsync(url, content);
//             if (!response.IsSuccessStatusCode)
//                 return null;

//             var resultJson = await response.Content.ReadAsStringAsync();
//             // TODO: Map the resultJson to ExtractedData. This is a simplified example.
//             var extracted = new ExtractedData();
//             using (JsonDocument doc = JsonDocument.Parse(resultJson))
//             {
//                 var root = doc.RootElement;
//                 // Example: try to extract fields by key name
//                 if (root.TryGetProperty("documents", out var docs) && docs.GetArrayLength() > 0)
//                 {
//                     var fields = docs[0].GetProperty("fields");
//                     if (fields.TryGetProperty("StudentId", out var studentId))
//                         extracted.StudentId = studentId.GetProperty("content").GetString();
//                     if (fields.TryGetProperty("GWA", out var gwa))
//                         extracted.GWA = double.TryParse(gwa.GetProperty("content").GetString(), out var g) ? g : null;
//                     if (fields.TryGetProperty("FirstSemesterGrades", out var fsg))
//                         extracted.FirstSemesterGrades = fsg.GetProperty("content").GetString();
//                     if (fields.TryGetProperty("SecondSemesterGrades", out var ssg))
//                         extracted.SecondSemesterGrades = ssg.GetProperty("content").GetString();
//                 }
//             }
//             return extracted;
//         }
//     }
// }