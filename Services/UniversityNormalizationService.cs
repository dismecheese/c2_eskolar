using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace c2_eskolar.Services
{
    /// <summary>
    /// Service for normalizing university names to canonical forms
    /// Handles typos, variations, prefixes, and abbreviations
    /// Source: https://en.wikipedia.org/wiki/List_of_colleges_and_universities_in_the_Philippines
    /// </summary>
    public class UniversityNormalizationService
    {
        // Canonical university names from Wikipedia
        private static readonly Dictionary<string, string> CanonicalNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Major state universities
            { "Polytechnic University of the Philippines", "Polytechnic University of the Philippines" },
            { "University of the Philippines", "University of the Philippines" },
            { "University of the Philippines Diliman", "University of the Philippines - Diliman" },
            { "University of the Philippines Manila", "University of the Philippines - Manila" },
            { "University of the Philippines Los Baños", "University of the Philippines - Los Baños" },
            { "University of the Philippines Cebu", "University of the Philippines - Cebu" },
            { "University of the Philippines Baguio", "University of the Philippines - Baguio" },
            { "University of the Philippines Visayas", "University of the Philippines - Visayas" },
            { "University of the Philippines Tacloban", "University of the Philippines - Tacloban" },
            { "Mindanao State University", "Mindanao State University" },
            { "Technological University of the Philippines", "Technological University of the Philippines" },
            { "Philippine Normal University", "Philippine Normal University" },
            
            // Major private universities in Metro Manila
            { "Ateneo de Manila University", "Ateneo de Manila University" },
            { "De La Salle University", "De La Salle University" },
            { "University of Santo Tomas", "University of Santo Tomas" },
            { "Far Eastern University", "Far Eastern University" },
            { "Adamson University", "Adamson University" },
            { "University of the East", "University of the East" },
            { "Mapua University", "Mapua University" },
            { "San Beda University", "San Beda University" },
            { "Centro Escolar University", "Centro Escolar University" },
            { "Lyceum of the Philippines University", "Lyceum of the Philippines University" },
            
            // Central Luzon
            { "Bulacan State University", "Bulacan State University" },
            { "Tarlac State University", "Tarlac State University" },
            { "Nueva Ecija University of Science and Technology", "Nueva Ecija University of Science and Technology" },
            { "Don Honorio Ventura Technological State University", "Don Honorio Ventura Technological State University" },
            { "Pampanga State Agricultural University", "Pampanga State Agricultural University" },
            { "Tarlac Agricultural University", "Tarlac Agricultural University" },
            
            // Southern Tagalog (Calabarzon)
            { "Batangas State University", "Batangas State University" },
            { "Cavite State University", "Cavite State University" },
            { "Laguna State Polytechnic University", "Laguna State Polytechnic University" },
            { "University of Rizal System", "University of Rizal System" },
            { "Southern Luzon State University", "Southern Luzon State University" },
            { "De La Salle University - Dasmariñas", "De La Salle University - Dasmariñas" },
            { "Lyceum of the Philippines University - Batangas", "Lyceum of the Philippines University - Batangas" },
            
            // Bicol Region
            { "Bicol University", "Bicol University" },
            { "Central Bicol State University of Agriculture", "Central Bicol State University of Agriculture" },
            { "Partido State University", "Partido State University" },
            { "Camarines Norte State College", "Camarines Norte State College" },
            
            // Western Visayas
            { "West Visayas State University", "West Visayas State University" },
            { "Iloilo Science and Technology University", "Iloilo Science and Technology University" },
            { "University of San Agustin", "University of San Agustin" },
            { "Central Philippine University", "Central Philippine University" },
            { "University of Saint La Salle", "University of Saint La Salle" },
            
            // Central Visayas
            { "Cebu Normal University", "Cebu Normal University" },
            { "Cebu Technological University", "Cebu Technological University" },
            { "University of San Carlos", "University of San Carlos" },
            { "University of Cebu", "University of Cebu" },
            { "Silliman University", "Silliman University" },
            { "Southwestern University", "Southwestern University" },
            
            // Northern Mindanao
            { "Mindanao State University - Iligan Institute of Technology", "Mindanao State University - Iligan Institute of Technology" },
            { "Xavier University", "Xavier University - Ateneo de Cagayan" },
            { "Central Mindanao University", "Central Mindanao University" },
            { "Bukidnon State University", "Bukidnon State University" },
            
            // Add more as needed from the Wikipedia list
        };

        // Common variations and mappings
        private static readonly Dictionary<string, string> Variations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // PUP variations
            { "Polyechnic University of the Philippines", "Polytechnic University of the Philippines" },
            { "Republic of the Philippines Polytechnic University of the Philippines", "Polytechnic University of the Philippines" },
            { "Polytechnic Univ of the Philippines", "Polytechnic University of the Philippines" },
            { "Polytechnic Univ. of the Philippines", "Polytechnic University of the Philippines" },
            { "PUP", "Polytechnic University of the Philippines" },
            
            // UP variations
            { "UP Diliman", "University of the Philippines - Diliman" },
            { "UP Manila", "University of the Philippines - Manila" },
            { "UP Los Baños", "University of the Philippines - Los Baños" },
            { "UP Los Banos", "University of the Philippines - Los Baños" },
            { "UP Cebu", "University of the Philippines - Cebu" },
            { "UP Baguio", "University of the Philippines - Baguio" },
            { "UP Visayas", "University of the Philippines - Visayas" },
            { "University of Philippines", "University of the Philippines" },
            { "Univ of the Philippines", "University of the Philippines" },
            
            // Ateneo variations
            { "Ateneo", "Ateneo de Manila University" },
            { "ADMU", "Ateneo de Manila University" },
            { "Ateneo de Manila", "Ateneo de Manila University" },
            
            // La Salle variations
            { "DLSU", "De La Salle University" },
            { "La Salle", "De La Salle University" },
            { "De La Salle Univ", "De La Salle University" },
            { "De La Salle University Manila", "De La Salle University" },
            
            // UST variations
            { "UST", "University of Santo Tomas" },
            { "Univ of Santo Tomas", "University of Santo Tomas" },
            { "University of Sto. Tomas", "University of Santo Tomas" },
            
            // FEU variations
            { "FEU", "Far Eastern University" },
            
            // UE variations
            { "UE", "University of the East" },
            { "Univ of the East", "University of the East" },
            
            // Add more variations as needed
        };

        // Words to remove when normalizing
        private static readonly string[] StopWords = new[]
        {
            "the", "of", "a", "an", "and", "or", "in", "at", "to", "for",
            "republic", "city", "college", "institute", "school", "campus"
        };

        /// <summary>
        /// Normalizes a university name to its canonical form
        /// </summary>
        public string NormalizeUniversityName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input ?? string.Empty;

            // First, check exact match in variations dictionary
            if (Variations.TryGetValue(input.Trim(), out string? exactMatch) && exactMatch != null)
                return exactMatch;

            // Check exact match in canonical names
            if (CanonicalNames.ContainsKey(input.Trim()))
                return CanonicalNames[input.Trim()];

            // Clean the input
            string cleaned = CleanUniversityName(input);

            // Try fuzzy matching
            var bestMatch = FindBestMatch(cleaned);
            if (bestMatch != null)
                return bestMatch;

            // If no match found, return original cleaned name
            return input.Trim();
        }

        /// <summary>
        /// Cleans university name by removing common prefixes/suffixes
        /// </summary>
        private string CleanUniversityName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            // Trim whitespace
            name = name.Trim();

            // Remove "Republic of the Philippines" prefix
            if (name.StartsWith("Republic of the Philippines", StringComparison.OrdinalIgnoreCase))
                name = name.Substring("Republic of the Philippines".Length).Trim();

            // Remove leading/trailing hyphens and spaces
            name = name.Trim('-', ' ');

            // Normalize whitespace
            name = Regex.Replace(name, @"\s+", " ");

            // Fix common typos
            name = name.Replace("Polyechnic", "Polytechnic", StringComparison.OrdinalIgnoreCase);
            name = name.Replace("Univ", "University", StringComparison.OrdinalIgnoreCase);
            name = name.Replace("Univ.", "University", StringComparison.OrdinalIgnoreCase);

            return name.Trim();
        }

        /// <summary>
        /// Finds the best matching canonical name using fuzzy matching
        /// </summary>
        private string? FindBestMatch(string input)
        {
            const int SIMILARITY_THRESHOLD = 85; // 85% similarity required
            
            string? bestMatch = null;
            int bestScore = 0;

            // Check all canonical names
            foreach (var canonical in CanonicalNames.Values.Distinct())
            {
                int score = CalculateSimilarity(input, canonical);
                if (score > bestScore && score >= SIMILARITY_THRESHOLD)
                {
                    bestScore = score;
                    bestMatch = canonical;
                }
            }

            // Also check variations
            foreach (var variation in Variations.Keys)
            {
                int score = CalculateSimilarity(input, variation);
                if (score > bestScore && score >= SIMILARITY_THRESHOLD)
                {
                    bestScore = score;
                    bestMatch = Variations[variation];
                }
            }

            return bestMatch;
        }

        /// <summary>
        /// Calculates similarity between two strings using Levenshtein distance
        /// Returns a percentage (0-100)
        /// </summary>
        private int CalculateSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0;

            source = source.ToLowerInvariant();
            target = target.ToLowerInvariant();

            // If exact match, return 100%
            if (source == target)
                return 100;

            // Calculate Levenshtein distance
            int distance = LevenshteinDistance(source, target);
            int maxLength = Math.Max(source.Length, target.Length);
            
            // Convert to percentage similarity
            int similarity = (int)(((double)(maxLength - distance) / maxLength) * 100);
            
            return Math.Max(0, Math.Min(100, similarity));
        }

        /// <summary>
        /// Calculates Levenshtein distance between two strings
        /// </summary>
        private int LevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return target?.Length ?? 0;
            
            if (string.IsNullOrEmpty(target))
                return source.Length;

            int n = source.Length;
            int m = target.Length;
            int[,] d = new int[n + 1, m + 1];

            // Initialize first column and row
            for (int i = 0; i <= n; i++)
                d[i, 0] = i;
            for (int j = 0; j <= m; j++)
                d[0, j] = j;

            // Calculate distance
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        /// <summary>
        /// Gets all known variations for a canonical university name
        /// </summary>
        public List<string> GetAllVariations(string canonicalName)
        {
            var variations = new List<string> { canonicalName };
            
            foreach (var kvp in Variations.Where(v => v.Value.Equals(canonicalName, StringComparison.OrdinalIgnoreCase)))
            {
                variations.Add(kvp.Key);
            }
            
            return variations;
        }

        /// <summary>
        /// Batch normalizes a list of university names
        /// </summary>
        public Dictionary<string, string> NormalizeBatch(IEnumerable<string> universities)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var university in universities.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                if (!result.ContainsKey(university))
                {
                    result[university] = NormalizeUniversityName(university);
                }
            }
            
            return result;
        }
    }
}
