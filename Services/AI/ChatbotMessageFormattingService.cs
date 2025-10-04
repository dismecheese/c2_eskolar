using System.Text.RegularExpressions;

namespace c2_eskolar.Services.AI
{
    /// <summary>
    /// Service responsible for formatting AI chatbot messages with proper HTML rendering,
    /// bold text conversion, and professional message structure.
    /// </summary>
    public class ChatbotMessageFormattingService
    {
        /// <summary>
        /// Formats raw AI response text into properly styled HTML for display in the chatbot interface.
        /// Handles bold formatting, emojis, titles, and maintains professional message structure.
        /// Works for scholarships, announcements, and any other content with **label:** patterns.
        /// </summary>
        /// <param name="text">Raw AI response text with markdown-style formatting</param>
        /// <returns>HTML-formatted string ready for display</returns>
        public string FormatBotMessage(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            var formatted = text;
            
            // Handle numbered titles (1. 2. 3.) for scholarships, announcements, etc.
            formatted = Regex.Replace(
                formatted,
                @"^(\d+\.\s*)([^\n]+?)(?:\n|$)",
                match => $"<div style='font-size: 1.6em !important; font-weight: bold !important; color: #000000 !important; margin: 16px 0 8px 0; line-height: 1.2; padding-left: 0 !important;'>{match.Groups[1].Value}{match.Groups[2].Value.Trim()}</div>",
                RegexOptions.Multiline);

            // Convert line breaks to HTML first
            formatted = formatted.Replace("\n", "<br>");

            // Handle ALL **label:** patterns (scholarships, announcements, and any other content)
            formatted = Regex.Replace(
                formatted,
                @"\*\*([^*]+?):\*\*\s*(.+?)(?=<br>|$)",
                match => $"<strong>{match.Groups[1].Value}:</strong> {match.Groups[2].Value}",
                RegexOptions.Multiline | RegexOptions.Singleline);

            // Handle remaining **text** patterns (general bold for any other content)
            formatted = Regex.Replace(
                formatted,
                @"\*\*([^*]+?)\*\*",
                match => $"<strong>{match.Groups[1].Value}</strong>",
                RegexOptions.Multiline);

            // Handle separators (--- or ----) with proper styling
            formatted = Regex.Replace(
                formatted,
                @"<br>\s*-{3,}\s*<br>",
                "<br><div style='border-top: 1px solid #e0e0e0; margin: 12px 0; padding-top: 8px;'></div>",
                RegexOptions.Multiline);

            // Clean up excessive line breaks
            formatted = Regex.Replace(formatted, @"(<br>\s*){3,}", "<br><br>");
            
            // Remove leading/trailing breaks
            formatted = formatted.Trim();
            if (formatted.StartsWith("<br>"))
                formatted = formatted.Substring(4);
            if (formatted.EndsWith("<br>"))
                formatted = formatted.Substring(0, formatted.Length - 4);
                
            return formatted;
        }
    }
}