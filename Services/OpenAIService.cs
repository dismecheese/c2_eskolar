
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace c2_eskolar.Services
{
    public class OpenAIService
    {
        private readonly OpenAIClient _client;
        private readonly string _deploymentName;

        public OpenAIService(IConfiguration config)
        {
            var apiKey = config["AzureOpenAI:ApiKey"] ?? throw new ArgumentNullException("AzureOpenAI:ApiKey");
            var endpoint = config["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("AzureOpenAI:Endpoint");
            _deploymentName = config["AzureOpenAI:DeploymentName"] ?? throw new ArgumentNullException("AzureOpenAI:DeploymentName");
            _client = new OpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
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
    }
}