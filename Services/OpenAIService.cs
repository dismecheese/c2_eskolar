
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace c2_eskolar.Services
{
    public class OpenAIService
    {
        private readonly OpenAIClient _client;
        private readonly string _deploymentName;
        private readonly ProfileSummaryService _profileSummaryService;

        public OpenAIService(IConfiguration config, ProfileSummaryService profileSummaryService)
        {
            var apiKey = config["AzureOpenAI:ApiKey"] ?? throw new ArgumentNullException("AzureOpenAI:ApiKey");
            var endpoint = config["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("AzureOpenAI:Endpoint");
            _deploymentName = config["AzureOpenAI:DeploymentName"] ?? throw new ArgumentNullException("AzureOpenAI:DeploymentName");
            _client = new OpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
            _profileSummaryService = profileSummaryService;
        }

        public async Task<string> GetChatCompletionAsync(string userMessage)
        {
            var chatOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _deploymentName,
                Messages = { new ChatRequestUserMessage(userMessage) }
            };
            var response = await _client.GetChatCompletionsAsync(chatOptions);
            return response.Value.Choices[0].Message.Content.Trim();
        }

        // New method: includes the user's profile summary in the prompt
        public async Task<string> GetChatCompletionWithProfileAsync(string userMessage, IdentityUser user, bool isFirstMessage = false)
        {
            var profileSummary = await _profileSummaryService.GetProfileSummaryAsync(user);
            var firstName = await _profileSummaryService.GetUserFirstNameAsync(user);
            
            // Create a personalized greeting only for the first message
            string greeting = "";
            if (isFirstMessage)
            {
                greeting = !string.IsNullOrEmpty(firstName) 
                    ? $"Hello {firstName}! How can I help you today?\n\n"
                    : "Hello! How can I help you today?\n\n";
            }

            string systemPrompt = profileSummary != null
                ? $"You are an AI assistant for eSkolar, a scholarship platform. You are helping a {profileSummary.Role.ToLower()}. " +
                  (isFirstMessage ? $"Start your response with: '{greeting.Trim()}'\n\n" : "") +
                  $"You have access to the user's profile information and should use it to answer their questions. " +
                  $"When they ask about their personal information (like 'what is my name?', 'what is my GPA?', 'what course am I taking?'), " +
                  $"answer using the specific data from their profile below. Be conversational and helpful.\n\n" +
                  $"If they ask about scholarships or need help finding scholarships, acknowledge that you can help them search for scholarships " +
                  $"that match their profile (this feature is being enhanced).\n\n" +
                  $"Always be friendly, professional, and specific when referencing their profile data."
                : (isFirstMessage 
                    ? $"You are an AI assistant for eSkolar, a scholarship platform. Start your response with: '{greeting.Trim()}' " +
                      $"I don't have access to your profile information yet, but I'm here to help with general questions about scholarships and the platform."
                    : "You are an AI assistant for eSkolar, a scholarship platform. Answer questions as best you can.");

            var messages = new List<ChatRequestMessage>
            {
                new ChatRequestSystemMessage(systemPrompt)
            };

            if (profileSummary != null)
            {
                // Create a more natural profile context
                var profileContext = $"User Profile Information:\n{profileSummary.Summary}";
                messages.Add(new ChatRequestSystemMessage(profileContext));
            }
            
            messages.Add(new ChatRequestUserMessage(userMessage));

            var chatOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _deploymentName,
                Messages = { }
            };
            foreach (var msg in messages)
            {
                chatOptions.Messages.Add(msg);
            }
            var response = await _client.GetChatCompletionsAsync(chatOptions);
            return response.Value.Choices[0].Message.Content.Trim();
        }
    }
}