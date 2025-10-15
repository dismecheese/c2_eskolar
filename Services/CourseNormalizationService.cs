using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace c2_eskolar.Services
{
    /// <summary>
    /// Service for normalizing course/program names from acronyms to full names
    /// Handles common Philippine academic program abbreviations
    /// </summary>
    public class CourseNormalizationService
    {
        // Map of acronyms to full course names
        private static readonly Dictionary<string, string> CourseNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Computer Science & IT
            { "BSIT", "Bachelor of Science in Information Technology" },
            { "BS IT", "Bachelor of Science in Information Technology" },
            { "BSCS", "Bachelor of Science in Computer Science" },
            { "BS CS", "Bachelor of Science in Computer Science" },
            { "BSIS", "Bachelor of Science in Information Systems" },
            { "BS IS", "Bachelor of Science in Information Systems" },
            { "BSCpE", "Bachelor of Science in Computer Engineering" },
            { "BS CpE", "Bachelor of Science in Computer Engineering" },
            
            // Engineering
            { "BSCE", "Bachelor of Science in Civil Engineering" },
            { "BS CE", "Bachelor of Science in Civil Engineering" },
            { "BSEE", "Bachelor of Science in Electrical Engineering" },
            { "BS EE", "Bachelor of Science in Electrical Engineering" },
            { "BSME", "Bachelor of Science in Mechanical Engineering" },
            { "BS ME", "Bachelor of Science in Mechanical Engineering" },
            { "BSChE", "Bachelor of Science in Chemical Engineering" },
            { "BS ChE", "Bachelor of Science in Chemical Engineering" },
            { "BSIE", "Bachelor of Science in Industrial Engineering" },
            { "BS IE", "Bachelor of Science in Industrial Engineering" },
            { "BSECE", "Bachelor of Science in Electronics and Communications Engineering" },
            { "BS ECE", "Bachelor of Science in Electronics and Communications Engineering" },
            
            // Business & Management
            { "BSBA", "Bachelor of Science in Business Administration" },
            { "BS BA", "Bachelor of Science in Business Administration" },
            { "BSA", "Bachelor of Science in Accountancy" },
            { "BS Accountancy", "Bachelor of Science in Accountancy" },
            { "BSAIS", "Bachelor of Science in Accounting Information Systems" },
            { "BS AIS", "Bachelor of Science in Accounting Information Systems" },
            { "BSAc", "Bachelor of Science in Accountancy" },
            { "BS Ac", "Bachelor of Science in Accountancy" },
            { "BSMA", "Bachelor of Science in Management Accounting" },
            { "BS MA", "Bachelor of Science in Management Accounting" },
            { "BSM", "Bachelor of Science in Management" },
            { "BS Management", "Bachelor of Science in Management" },
            { "BSHRM", "Bachelor of Science in Hotel and Restaurant Management" },
            { "BS HRM", "Bachelor of Science in Hotel and Restaurant Management" },
            { "BSTM", "Bachelor of Science in Tourism Management" },
            { "BS TM", "Bachelor of Science in Tourism Management" },
            { "BSE", "Bachelor of Science in Entrepreneurship" },
            { "BS Entrepreneurship", "Bachelor of Science in Entrepreneurship" },
            
            // Education
            { "BSEd", "Bachelor of Secondary Education" },
            { "BS Ed", "Bachelor of Secondary Education" },
            { "BEEd", "Bachelor of Elementary Education" },
            { "BE Ed", "Bachelor of Elementary Education" },
            // BEED removed - duplicate of BEEd (case-insensitive)
            { "BECEd", "Bachelor of Early Childhood Education" },
            // BECED removed - duplicate of BECEd (case-insensitive)
            { "BPEd", "Bachelor of Physical Education" },
            { "BP Ed", "Bachelor of Physical Education" },
            
            // Health Sciences
            { "BSN", "Bachelor of Science in Nursing" },
            { "BS Nursing", "Bachelor of Science in Nursing" },
            { "BSPh", "Bachelor of Science in Pharmacy" },
            { "BS Pharmacy", "Bachelor of Science in Pharmacy" },
            { "BSPT", "Bachelor of Science in Physical Therapy" },
            { "BS PT", "Bachelor of Science in Physical Therapy" },
            { "BSMT", "Bachelor of Science in Medical Technology" },
            { "BS MT", "Bachelor of Science in Medical Technology" },
            { "BSRT", "Bachelor of Science in Radiologic Technology" },
            { "BS RT", "Bachelor of Science in Radiologic Technology" },
            { "BSPsy", "Bachelor of Science in Psychology" },
            { "BS Psychology", "Bachelor of Science in Psychology" },
            { "BSND", "Bachelor of Science in Nutrition and Dietetics" },
            { "BS ND", "Bachelor of Science in Nutrition and Dietetics" },
            
            // Natural Sciences
            { "BSBio", "Bachelor of Science in Biology" },
            { "BS Biology", "Bachelor of Science in Biology" },
            { "BSChem", "Bachelor of Science in Chemistry" },
            { "BS Chemistry", "Bachelor of Science in Chemistry" },
            { "BSPhys", "Bachelor of Science in Physics" },
            { "BS Physics", "Bachelor of Science in Physics" },
            { "BSMath", "Bachelor of Science in Mathematics" },
            { "BS Mathematics", "Bachelor of Science in Mathematics" },
            { "BSES", "Bachelor of Science in Environmental Science" },
            { "BS ES", "Bachelor of Science in Environmental Science" },
            { "BSMBio", "Bachelor of Science in Marine Biology" },
            { "BS Marine Biology", "Bachelor of Science in Marine Biology" },
            
            // Social Sciences
            { "AB", "Bachelor of Arts" },
            { "AB PolSci", "Bachelor of Arts in Political Science" },
            { "AB Political Science", "Bachelor of Arts in Political Science" },
            { "ABPS", "Bachelor of Arts in Political Science" },
            { "AB Socio", "Bachelor of Arts in Sociology" },
            { "AB Sociology", "Bachelor of Arts in Sociology" },
            { "AB Psych", "Bachelor of Arts in Psychology" },
            { "AB Psychology", "Bachelor of Arts in Psychology" },
            { "AB History", "Bachelor of Arts in History" },
            { "AB Comm", "Bachelor of Arts in Communication" },
            { "AB Communication", "Bachelor of Arts in Communication" },
            { "ABMC", "Bachelor of Arts in Mass Communication" },
            { "AB Mass Communication", "Bachelor of Arts in Mass Communication" },
            { "ABIS", "Bachelor of Arts in International Studies" },
            { "AB International Studies", "Bachelor of Arts in International Studies" },
            
            // Arts & Design
            { "BFA", "Bachelor of Fine Arts" },
            { "BSOA", "Bachelor of Science in Office Administration" },
            { "BS OA", "Bachelor of Science in Office Administration" },
            { "BSArchi", "Bachelor of Science in Architecture" },
            { "BS Architecture", "Bachelor of Science in Architecture" },
            
            // Agriculture
            // Note: BSA conflicts with Accountancy, use BSAgri or BSAg instead
            { "BS Agriculture", "Bachelor of Science in Agriculture" },
            { "BSAgri", "Bachelor of Science in Agriculture" },
            { "BSAg", "Bachelor of Science in Agriculture" },
            { "BSABE", "Bachelor of Science in Agricultural and Biosystems Engineering" },
            { "BS ABE", "Bachelor of Science in Agricultural and Biosystems Engineering" },
            
            // Criminology
            { "BSCrim", "Bachelor of Science in Criminology" },
            { "BS Criminology", "Bachelor of Science in Criminology" },
            
            // Marine Sciences
            { "BSMarE", "Bachelor of Science in Marine Engineering" },
            { "BS Marine Engineering", "Bachelor of Science in Marine Engineering" },
            // Note: BSMT conflicts with Medical Technology, use BSMarT instead
            { "BSMarT", "Bachelor of Science in Marine Transportation" },
            { "BS Marine Transportation", "Bachelor of Science in Marine Transportation" },
            
            // Communication
            { "BSC", "Bachelor of Science in Communication" },
            { "BS Communication", "Bachelor of Science in Communication" },
            { "BJMC", "Bachelor of Journalism and Mass Communication" },
            
            // Other Programs
            { "BLIS", "Bachelor of Library and Information Science" },
            { "BSW", "Bachelor of Social Work" },
            { "BS Social Work", "Bachelor of Social Work" },
            { "BSDC", "Bachelor of Science in Development Communication" },
            { "BS DC", "Bachelor of Science in Development Communication" },
        };

        /// <summary>
        /// Normalizes a course name from acronym to full name
        /// </summary>
        /// <param name="courseName">The course name or acronym to normalize</param>
        /// <returns>The full course name, or the original if no match found</returns>
        public string NormalizeCourseName(string courseName)
        {
            if (string.IsNullOrWhiteSpace(courseName))
                return courseName ?? "";

            var trimmed = courseName.Trim();
            
            // Check if it's already a full name (contains "Bachelor" or "Master")
            if (trimmed.Contains("Bachelor", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("Master", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            // Try exact match first
            if (CourseNames.TryGetValue(trimmed, out var fullName))
            {
                return fullName;
            }

            // Try with common variations (remove dots, extra spaces)
            var normalized = Regex.Replace(trimmed, @"[.\s]+", " ").Trim();
            if (CourseNames.TryGetValue(normalized, out fullName))
            {
                return fullName;
            }

            // Try without spaces
            var noSpaces = Regex.Replace(trimmed, @"\s+", "");
            if (CourseNames.TryGetValue(noSpaces, out fullName))
            {
                return fullName;
            }

            // Return original if no match found
            return trimmed;
        }

        /// <summary>
        /// Checks if a course name is likely an acronym that should be normalized
        /// </summary>
        public bool IsAcronym(string courseName)
        {
            if (string.IsNullOrWhiteSpace(courseName))
                return false;

            var trimmed = courseName.Trim();
            
            // Check if it's already a full name
            if (trimmed.Contains("Bachelor", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("Master", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Check if it exists in our acronym dictionary
            return CourseNames.ContainsKey(trimmed);
        }
    }
}
