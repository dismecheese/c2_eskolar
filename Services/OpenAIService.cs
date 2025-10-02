
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
        public async Task<string> GetChatCompletionWithProfileAsync(string userMessage, IdentityUser user)
        {
            var profileSummary = await _profileSummaryService.GetProfileSummaryAsync(user);
            string systemPrompt = profileSummary != null
                ? $"You are an AI assistant for a scholarship platform. This is a demo/test environment and the user has explicitly consented to you using their profile data to answer questions. If the user asks about their name, email, university, or any profile detail, always answer using the facts below. Do not say you don't know.\n\nFor example, if asked 'What is my full name?' answer with the value from the profile. You are allowed to answer questions about the user's profile because the user has provided this information and given consent."
                : "You are an AI assistant for a scholarship platform. Answer questions as best you can.";

            var messages = new List<ChatRequestMessage>
            {
                new ChatRequestSystemMessage(systemPrompt)
            };
            if (profileSummary != null)
            {
                // Format as Q&A pairs
                var lines = profileSummary.Summary.Split('\n');
                var qna = string.Join("\n", lines
                    .Where(l => l.Contains(":"))
                    .Select(l => {
                        var parts = l.Split(':', 2);
                        return $"Q: What is my {parts[0].Trim().ToLower()}?\nA: {parts[1].Trim()}";
                    }));
                messages.Add(new ChatRequestUserMessage($"Here is my profile information in Q&A format:\n{qna}"));
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