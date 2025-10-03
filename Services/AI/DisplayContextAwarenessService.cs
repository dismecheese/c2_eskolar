using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace c2_eskolar.Services.AI
{
    public class ChatContext
    {
        public string CurrentPage { get; set; } = "Dashboard";
        public string CurrentSection { get; set; } = "";
        public Dictionary<string, object> PageData { get; set; } = new();
        public List<object> VisibleItems { get; set; } = new();
    }

    public class DisplayContextAwarenessService
    {
        public string GeneratePageContext(ChatContext chatContext)
        {
            if (chatContext == null)
                return "";

            var contextInfo = new StringBuilder();
            contextInfo.AppendLine($"**Current Context:**");
            contextInfo.AppendLine($"- Page: {chatContext.CurrentPage}");
            
            if (!string.IsNullOrEmpty(chatContext.CurrentSection))
                contextInfo.AppendLine($"- Section: {chatContext.CurrentSection}");

            // Add page data context
            if (chatContext.PageData?.Any() == true)
            {
                contextInfo.AppendLine("- Page Data:");
                foreach (var kvp in chatContext.PageData)
                {
                    var value = kvp.Value?.ToString() ?? "null";
                    if (value.Length > 100)
                        value = value.Substring(0, 100) + "...";
                    contextInfo.AppendLine($"  - {kvp.Key}: {value}");
                }
            }

            // Add visible items context
            if (chatContext.VisibleItems?.Any() == true)
            {
                contextInfo.AppendLine($"- Visible Items: {chatContext.VisibleItems.Count} items");
                
                // Use reflection to get object details
                for (int i = 0; i < Math.Min(chatContext.VisibleItems.Count, 3); i++)
                {
                    var item = chatContext.VisibleItems[i];
                    if (item != null)
                    {
                        contextInfo.AppendLine($"  Item {i + 1}: {GetObjectSummary(item)}");
                    }
                }
                
                if (chatContext.VisibleItems.Count > 3)
                {
                    contextInfo.AppendLine($"  ... and {chatContext.VisibleItems.Count - 3} more items");
                }
            }

            contextInfo.AppendLine();
            contextInfo.AppendLine("Use this context to provide relevant, helpful responses about what the user is currently viewing.");
            
            return contextInfo.ToString();
        }

        private string GetObjectSummary(object obj)
        {
            if (obj == null) return "null";
            
            var type = obj.GetType();
            var summary = new StringBuilder($"{type.Name}: ");
            
            try
            {
                // Get a few key properties using reflection
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                    .Take(3);
                
                var values = new List<string>();
                foreach (var prop in properties)
                {
                    try
                    {
                        var value = prop.GetValue(obj);
                        if (value != null)
                        {
                            var valueStr = value.ToString();
                            if (!string.IsNullOrEmpty(valueStr) && valueStr.Length > 50)
                                valueStr = valueStr.Substring(0, 50) + "...";
                            values.Add($"{prop.Name}={valueStr}");
                        }
                    }
                    catch
                    {
                        // Skip properties that throw exceptions
                    }
                }
                
                summary.Append(string.Join(", ", values));
            }
            catch
            {
                summary.Append(obj.ToString());
            }
            
            return summary.ToString();
        }

        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || 
                   type == typeof(string) || 
                   type == typeof(DateTime) || 
                   type == typeof(decimal) || 
                   type == typeof(Guid) ||
                   (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                    IsSimpleType(type.GetGenericArguments()[0]));
        }
    }
}